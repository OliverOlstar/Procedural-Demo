
using System.Collections.Generic;
using UnityEngine;

public interface IActSequenceControllerOwner : ActSequencer.IActSequenceOwner
{
	ActParams CreateParams();
}

public interface IActParamsFilter
{
	bool AllowParams(ActParams actParams);
}

public class ActSequenceController
{
	private static readonly int PARAMS_LIST_SIZE = 10;

	public enum UpdateMode
	{
		GameTime = 0,
		Realtime
	}

	protected ActSequencer m_Sequencer = null;
	public ActSequencer GetSequencer() { return m_Sequencer; }
	public string SequencerToString() { return m_Sequencer == null ? Core.Str.EMPTY : m_Sequencer.ToString(); }
	public string SequencerDebugString() { return m_Sequencer == null ? Core.Str.EMPTY : m_Sequencer.GetDebugString(); }

	protected List<ActParams> m_NewParamsList = new List<ActParams>(PARAMS_LIST_SIZE);

	private List<ActTreeRT> m_AllTrees = new List<ActTreeRT>();
	public IEnumerable<ActTreeRT> GetAllRuntimeTrees() { return m_AllTrees; }
	public ActTreeRT GetMainRuntimTree() { return m_AllTrees.Count > 0 ? m_AllTrees[m_AllTrees.Count - 1] : null; }

	private string m_ScriptedNode = Core.Str.EMPTY;
	private ActParams m_ScriptedParams = null;

	private string m_StartNode = Core.Str.EMPTY;
	private int m_StartNodeID = Act.Node.INVALID_ID;
	private ActParams m_StartNodeParams = null;

	private int m_NodeID = Act.Node.INVALID_ID;
	private ActParams m_NodeIDParams = null;

	private IActSequenceControllerOwner m_Owner = null;
	private string GetDebugName() => Core.Str.Build(
				m_Owner != null ? m_Owner.name : Core.Str.EMPTY, ".",
				m_AllTrees.Count > 0 ? m_AllTrees[m_AllTrees.Count - 1].GetName() : Core.Str.EMPTY);

	private bool m_Resequence = false;

	private bool m_UpdatedThisFrame = false;

	private UpdateMode m_UpdateMode = UpdateMode.GameTime;
	public void SetUpdateMode(UpdateMode mode) { m_UpdateMode = mode; }

	private float m_Speed = 1.0f;

	public ActSequenceController(
		ActTree mainTree,
		ActTree[] secondaryTrees,
		IActSequenceControllerOwner owner,
		UpdateMode updateMode = UpdateMode.GameTime)
	{
		for (int i = 0; i < secondaryTrees.Length; i++)
		{
			ActTreeRT rtTree = new ActTreeRT(secondaryTrees[i]);
			m_AllTrees.Add(rtTree);
		}
		ActTreeRT mainTreeRT = new ActTreeRT(mainTree);
		m_AllTrees.Add(mainTreeRT);
		m_Owner = owner;
		m_UpdateMode = updateMode;
	}

	public void SetStartNode(int id, ActParams scriptedParams = null)
	{
		m_StartNodeID = id;
		m_StartNodeParams = scriptedParams;
	}
	public void SetStartNodeName(string name, ActParams scriptedParams = null)
	{
		m_StartNode = name;
		m_StartNodeParams = scriptedParams;
	}
	public void PlayNode(string nodeName, ActParams scriptedParams = null)
	{
		if (m_NodeID != Act.Node.INVALID_ID) // Scripted node ID's take priority
		{
			Debug.LogWarning(GetDebugName() + " ActSequenceController.PlayNode() Can't play " + nodeName + " node id " + m_NodeID);
			return;
		}
		m_ScriptedNode = nodeName;
		m_ScriptedParams = scriptedParams;
		ClearParams(); // Clear the params here, we should handle params that come after
	}
	public void PlayNode(int id, ActParams scriptedParams = null)
	{
		if (!Core.Str.IsEmpty(m_ScriptedNode)) // Scripted node ID's take priority
		{
			Debug.LogWarning(GetDebugName() + " ActSequenceController.PlayNodeID() Node id " + id + " overrides node name " + m_ScriptedNode);
			m_ScriptedNode = Core.Str.EMPTY;
			m_ScriptedParams = null;
		}
		m_NodeID = id;
		m_NodeIDParams = scriptedParams;
		ClearParams(); // Clear the params here, we should handle params that come after
	}

	public void AddTree(ActTree tree)
	{
		// Want to insert new tree right one spot before the main tree
		m_AllTrees.Insert(m_AllTrees.Count - 1, new ActTreeRT(tree, m_Owner.CreateParams()));
	}

	public void Reset()
	{
		// Clear the params here, we should handle params that come after the resequence
		ClearParams();
		if (m_StartNodeID != Act.Node.INVALID_ID)
		{
			PlayNode(m_StartNodeID, m_StartNodeParams);
		}
		else if (!Core.Str.IsEmpty(m_StartNode))
		{
			PlayNode(m_StartNode, m_StartNodeParams);
		}
		else
		{
			m_Resequence = true;
		}
	}

	protected void ClearParams()
	{
		foreach (ActParams actParams in m_NewParamsList)
		{
			if (actParams != null)
			{
				actParams.Process();
			}
		}
		m_NewParamsList.Clear();
	}

	public void SendParams(ActParams stateParams)
	{
		if (stateParams == null)
		{
			Debug.LogError(GetDebugName() + " ActSequenceController.SendParams() sent null params!");
			return;
		}

		if (m_NewParamsList.Count == m_NewParamsList.Capacity)
		{
			Core.Str.Add(GetDebugName(), " ActSequenceController.SendParams() ", m_NewParamsList.Count.ToString(), " params recived this frame");
			if (!Core.Util.IsRelease())
			{
				foreach (ActParams param in m_NewParamsList)
				{
					Core.Str.AddNewLine(param.GetType().Name);
				}
			}
			Debug.LogWarning(Core.Str.Finish());
		}

//		Debug.LogWarning(name + " " + this.GetType().Name + ".SendParams() " + mNewParamsList.Count 
//			+ " " + stateParams.ToString() + " " + Time.time);

		m_NewParamsList.Add(stateParams);
	}

	public void FilterParams(IActParamsFilter filter)
	{
		int count = m_NewParamsList.Count;
		for (int i = 0; i < count; i++)
		{
			ActParams newParams = m_NewParamsList[i];
			if (!filter.AllowParams(newParams))
			{
				newParams.Process();
				m_NewParamsList[i] = null;
			}
		}
	}

	protected bool UpdateParams()
	{
		if (m_Sequencer == null || !m_Sequencer.IsPlaying())
		{
			return false;
		}

		bool oppTaken = false;
		for (int i = 0; i < m_NewParamsList.Count; i++)
		{
			// This can happen because we need to remove button params from the list sometimes
			ActParams newParams = m_NewParamsList[i];
			if (newParams == null)
			{
				continue;
			}

//#if UNITY_EDITOR
//			string oldPath = mActSequencer.ToString();
//#endif
			m_NewParamsList[i] = null; // Remove params from list before new condtions and tracks fire
			oppTaken = m_Sequencer.NewParams(newParams);
			if (!oppTaken)
			{
				for (int j = 0; j < m_AllTrees.Count; j++)
				{
					ActTreeRT tree = m_AllTrees[j];
					if (tree.GetSourceTree().GetInstanceID() == m_Sequencer.GetTree().GetSourceTree().GetInstanceID())
					{
						continue; // If this is our current sequencer's source tree it has already got its opportunity to respond to new params
					}
					newParams.Reset();
					ActSequencer newActSequencer = ActSequencer.TryCreateAndSequence(tree, newParams, m_Owner);
					if (newActSequencer != null)
					{
						m_Sequencer.StopAndDispose();
						m_Sequencer = newActSequencer;
						m_Sequencer.Play();
						break;
					}
				}
			}

//#if UNITY_EDITOR
//			GameUtil.Log(this.GetType().Name + ".UpdateParams() " + temp + " \n" 
//				+ (oppTaken ? oldPath + " -> " + mActSequencer.ToString() : "opp NOT taken " + mActSequencer.ToString()), 
//				gameObject, oppTaken);
//#endif
		}

		ClearParams();

		return oppTaken;
	}

	protected void ForceUpdate()
	{
		Update();
		m_UpdatedThisFrame = true;
	}

	// We want to initialize the trees after Awake() has been called
	// This is meant to be called from Monobehaviour.Start()
	public void Start(bool update = true)
	{
		// Only try to set start node if we we're asked to play a different node
		// We should respect calls to PlayNode() that happen before Start()
		if (m_NodeID == Act.Node.INVALID_ID && string.IsNullOrEmpty(m_ScriptedNode))
		{
			if (m_StartNodeID != Act.Node.INVALID_ID)
			{
				PlayNode(m_StartNodeID, m_StartNodeParams);
			}
			else if (!string.IsNullOrEmpty(m_StartNode))
			{
				PlayNode(m_StartNode, m_StartNodeParams);
			}
		}

		ActParams defaultParams = m_Owner.CreateParams();
		for (int i = 0; i < m_AllTrees.Count; i++)
		{
			m_AllTrees[i].Initialize(defaultParams);
		}

		// Typically we want to force an update in Start() depending on when this behaviour gets instatiated sometimes Monobehaviour can have
		// Start() called but not Update() so this guarentees the tree will update on the frame we're instatiated
		// In some cases we might have to disable this behaviour, if we want to initialize an ActSequenceController but not have it start playing anything yet
		if (update)
		{
			ForceUpdate();
		}
	}

	void ReSequence()
	{
		if (m_Sequencer != null)
		{
			m_Sequencer.StopAndDispose();
		}
		m_Sequencer = null;

		ActParams actParams = m_Owner.CreateParams();
		for (int i = 0; i < m_AllTrees.Count; i++)
		{
			actParams.Reset();
			ActSequencer temp = ActSequencer.TryCreateAndSequence(m_AllTrees[i], actParams, m_Owner, m_Speed);
			if (temp != null)
			{
				m_Sequencer = temp;
				m_Sequencer.Play();
				m_Sequencer.Update(0.0f); // Make sure events fire
				break;
			}
		}

		if (m_Sequencer == null) Debug.LogError(GetDebugName() + " ActSequenceController.ReSequence() Failed to re-sequence anything! " + Time.time);
	}

	public bool IsPlaying(string nodeName)
	{
		if (m_Sequencer == null)
		{
			return false;
		}
		return m_Sequencer.IsPlaying(nodeName);
	}

	public bool IsPlaying(int nodeID)
	{
		if (m_Sequencer == null)
		{
			return false;
		}
		return m_Sequencer.IsPlaying(nodeID);
	}

	private void PlayScriptedNode()
	{
		if (Core.Str.IsEmpty(m_ScriptedNode))
		{
			return;
		}

		if (m_Sequencer != null && m_Sequencer.IsPlaying(m_ScriptedNode))
		{
			m_ScriptedNode = string.Empty;
			m_ScriptedParams = null;
			return;
		}

		ActParams newParams = m_ScriptedParams != null ? m_ScriptedParams : m_Owner.CreateParams();
		for (int i = 0; i < m_AllTrees.Count; i++)
		{
			newParams.Reset();
			ActSequencer newActSequencer = ActSequencer.TryCreateAndSequenceWithNodeName(m_AllTrees[i], newParams, m_Owner, m_ScriptedNode, m_Speed);
			if (newActSequencer != null)
			{
				if (m_Sequencer != null)
				{
					m_Sequencer.StopAndDispose();
				}
				m_Sequencer = newActSequencer;
				m_Sequencer.Play();
				m_Sequencer.Update(0.0f); // Make sure events fire
				break;
			}
		}

#if UNITY_EDITOR
		if (m_Sequencer == null || !m_Sequencer.IsPlaying(m_ScriptedNode))
		{
			Debug.LogError(GetDebugName() + " ActSequenceController.Update() Scripted node " + m_ScriptedNode + " failed " + Time.time);
		}
#endif

		m_ScriptedNode = string.Empty;
		m_ScriptedParams = null;
	}

	private void PlayScriptedNodeID()
	{
		if (m_NodeID == Act.Node.INVALID_ID)
		{
			return;
		}

		ActParams newParams = m_NodeIDParams != null ? m_NodeIDParams : m_Owner.CreateParams();
		int mainIndex = m_AllTrees.Count - 1;
		ActSequencer newActSequencer = ActSequencer.TryCreateAndSequenceWithNodeID(m_AllTrees[mainIndex], newParams, m_Owner, m_NodeID, m_Speed);
		if (newActSequencer != null)
		{
			if (m_Sequencer != null)
			{
				m_Sequencer.StopAndDispose();
			}
			m_Sequencer = newActSequencer;
			m_Sequencer.Play();
			m_Sequencer.Update(0.0f); // Make sure events fire
			//Debug.Log(this.GetType().Name + ".PlayScriptedNodeID() Node ID "
			//	+ m_NodeID + " " + m_Sequencer.ToString(), gameObject);
		}
		else
		{
			Debug.LogWarning(GetDebugName() + " ActSequenceController.PlayScriptedNodeID() Node ID "
				+ m_NodeID + " FAILED " + m_Sequencer.ToString());
		}
		m_NodeID = Act.Node.INVALID_ID;
	}

	// Update is called once per frame similar to Monobehaviour.Update()
	public void Update()
	{
		if (m_UpdatedThisFrame)
		{
			// Don't want to update twice in one frame.
			m_UpdatedThisFrame = false;
			return;
		}
		if (m_AllTrees.Count == 0)
		{
			Debug.LogError(GetDebugName() + " ActSequenceController.Update() anim state controller has no trees");
			return;
		}

		if (m_Resequence)
		{
			m_Resequence = false;
			if (m_Sequencer != null)
			{
				m_Sequencer.StopAndDispose();
				m_Sequencer = null;
			}
			ReSequence();
			return;
		}

		PlayScriptedNodeID();
		PlayScriptedNode();

		if (UpdateParams())
		{
			return;
		}

//#if UNITY_EDITOR
//		float oldTime = mActSequencer.GetLeafActSequencer().GetTimer();
//		string oldString = mActSequencer.ToString();
//#endif

		if (m_Sequencer == null)
		{
			ReSequence();
			return;
		}

		float deltaTime = m_UpdateMode == UpdateMode.Realtime ? Time.unscaledDeltaTime : Time.deltaTime;
		if (!m_Sequencer.Update(deltaTime))
		{
			ReSequence();
		}

		UpdateParams(); // Tracks might send params that we want to resolve right away

//#if UNITY_EDITOR
//		if (mActSequencer.GetLeafActSequencer().GetTimer() < oldTime)
//		{
//			GameUtil.Log(this.GetType().Name + ".Update() Sequenced " + oldString + " -> " + mActSequencer, gameObject, true);
//		}
//#endif
	}

	public void Stop()
	{
		// Make sure we get our end events before destroying
		if (m_Sequencer != null)
		{
			m_Sequencer.StopAndDispose();
		}
		m_Sequencer = null;
		ClearParams();
	}

	public void Destroy()
	{
		foreach (ActTreeRT treeRT in m_AllTrees)
		{
			treeRT.Destroy();
		}
		m_AllTrees.Clear();
	}

	public void SetSpeed(float speed)
	{
		m_Speed = speed;
		if (m_Sequencer != null)
		{
			m_Sequencer.SetTimeScale(speed);
		}
	}
}
