
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public interface ISequenceControllerOwner : ITreeOwner
	{
		bool isActiveAndEnabled { get; }
	}

	public interface IParamsFilter
	{
		bool AllowParams(Params actParams);
	}

	public class SequenceController
	{
		private static readonly int PARAMS_LIST_SIZE = 10;
		private static readonly int LOWEST_SCRIPTED_PRIORITY = int.MinValue;

		public enum UpdateMode
		{
			GameTime = 0,
			Realtime
		}

		protected Sequencer m_Sequencer = null;
		public Sequencer GetSequencer() { return m_Sequencer; }
		public string SequencerToString() { return m_Sequencer == null ? Core.Str.EMPTY : m_Sequencer.ToString(); }
		public string SequencerDebugString() { return m_Sequencer == null ? Core.Str.EMPTY : m_Sequencer.GetDebugString(); }

		protected List<Params> m_NewParamsList = new List<Params>(PARAMS_LIST_SIZE);
		public IEnumerable<Params> NewParams => m_NewParamsList;

		private List<TreeRT> m_AllTrees = new List<TreeRT>();
		public IEnumerable<TreeRT> GetAllRuntimeTrees() { return m_AllTrees; }
		public TreeRT GetMainRuntimTree() { return m_AllTrees.Count > 0 ? m_AllTrees[m_AllTrees.Count - 1] : null; }

		private string m_ScriptedNodeName = string.Empty;
		private int m_ScripedNodeID = Node.INVALID_ID;
		private Params m_ScriptedParams = null;
		private int m_ScriptedNodePriority = LOWEST_SCRIPTED_PRIORITY;
		private bool m_ScriptedNodeResequence = true;
		private bool m_ScriptedNodeIgnoreConditons = false;

		private string m_StartNode = Core.Str.EMPTY;
		private int m_StartNodeID = Act2.Node.INVALID_ID;
		private Params m_StartNodeParams = null;

		private ISequenceControllerOwner m_Owner = null;
		private string GetDebugName() => Core.Str.Build(
					m_Owner != null ? m_Owner.name : Core.Str.EMPTY, ".",
					m_AllTrees.Count > 0 ? m_AllTrees[m_AllTrees.Count - 1].Name : Core.Str.EMPTY);

		private bool m_Resequence = false;

		private bool m_UpdatedThisFrame = false;

		private UpdateMode m_UpdateMode = UpdateMode.GameTime;
		public void SetUpdateMode(UpdateMode mode) { m_UpdateMode = mode; }

		private float m_Speed = 1.0f;
		public float Speed => m_Speed;

		public SequenceController(
			ActTree2 mainTree,
			IEnumerable<ActTree2> secondaryTrees,
			ISequenceControllerOwner owner,
			UpdateMode updateMode = UpdateMode.GameTime)
		{
			m_Owner = owner ?? throw new System.ArgumentNullException("SequenceController() owner cannot be null");
			foreach (ActTree2 tree in secondaryTrees)
			{
				TreeRT rtTree = TreeRT.CreateAndDeferInitialize(tree, m_Owner);
				m_AllTrees.Add(rtTree);
			}
			TreeRT mainTreeRT = mainTree != null ? TreeRT.CreateAndDeferInitialize(mainTree, m_Owner) : throw new System.ArgumentNullException("SequenceController() mainTree cannot be null");
			m_AllTrees.Add(mainTreeRT);
			m_UpdateMode = updateMode;
		}

		public void SetStartNode(int id, Params scriptedParams = null)
		{
			m_StartNodeID = id;
			m_StartNodeParams = scriptedParams;
		}
		public void SetStartNodeName(string name, Params scriptedParams = null)
		{
			m_StartNode = name;
			m_StartNodeParams = scriptedParams;
		}

		/// <param name="priority">higher numbers are first priority</param>
		public void PlayNodeID(
			int nodeID,
			Params scriptedParams = null,
			bool resequenceIfAlreadyPlaying = true,
			bool ignoreConditions = false,
			int priority = 0)
		{
			PlayNodeInternal(string.Empty, nodeID, scriptedParams, resequenceIfAlreadyPlaying, ignoreConditions, priority);
		}

		/// <param name="priority">higher numbers are first priority</param>
		public void PlayNodeName(
			string nodeName,
			Params scriptedParams = null,
			bool resequenceIfAlreadyPlaying = true,
			bool ignoreConditions = false,
			int priority = 0)
		{
			if (!Core.Util.IsRelease())
			{
				bool found = false;
				foreach (TreeRT tree in m_AllTrees)
				{
					if (tree.HasNode(nodeName))
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					Debug.LogWarning($"{GetDebugName()} ActSequenceController.PlayNodeName() '{m_ScriptedNodeName}' will fail, can't find this node in any trees t:{Time.time}");
				}
			}
			PlayNodeInternal(nodeName, Node.INVALID_ID, scriptedParams, resequenceIfAlreadyPlaying, ignoreConditions, priority);
		}

		private void PlayNodeInternal(
			string nodeName,
			int nodeID,
			Params scriptedParams = null,
			bool resequenceIfAlreadyPlaying = true,
			bool ignoreConditions = false,
			int priority = 0)
		{
			if (priority < m_ScriptedNodePriority)
			{
				return;
			}
			m_ScriptedNodeName = nodeName;
			m_ScripedNodeID = nodeID;
			m_ScriptedParams = scriptedParams;
			m_ScriptedNodeResequence = resequenceIfAlreadyPlaying;
			m_ScriptedNodeIgnoreConditons = ignoreConditions;
			m_ScriptedNodePriority = priority;
			ClearParams(); // Clear the params here, we should handle params that come after
		}

		public void AddTree(ActTree2 tree)
		{
			// Want to insert new tree right one spot before the main tree
			m_AllTrees.Insert(m_AllTrees.Count - 1, TreeRT.CreateAndInitialize(tree, m_Owner));
		}

		public void Reset()
		{
			// Clear the params here, we should handle params that come after the resequence
			ClearParams();
			if (m_StartNodeID != Act2.Node.INVALID_ID)
			{
				PlayNodeID(m_StartNodeID, m_StartNodeParams);
			}
			else if (!Core.Str.IsEmpty(m_StartNode))
			{
				PlayNodeName(m_StartNode, m_StartNodeParams);
			}
			else
			{
				m_Resequence = true;
			}
		}

		protected void ClearParams()
		{
			foreach (Params actParams in m_NewParamsList)
			{
				if (actParams != null)
				{
					actParams.Process();
				}
			}
			m_NewParamsList.Clear();
		}

		public void SendParams(Params stateParams)
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
					foreach (Params param in m_NewParamsList)
					{
						Core.Str.AddNewLine(param.GetType().Name);
					}
				}
				Debug.LogWarning(Core.Str.Finish());
			}

#if UNITY_EDITOR
			if (DebugOptions.LogSequencing.IsSet())
			{
				Debug.Log(m_Owner.name + " " + GetType().Name + ".SendParams() " + 
					stateParams.ToString() + " count: " + m_NewParamsList.Count + " " + Time.time);
			}
#endif
			m_NewParamsList.Add(stateParams);
		}

		public void FilterParams(IParamsFilter filter)
		{
			int count = m_NewParamsList.Count;
			for (int i = 0; i < count; i++)
			{
				Params newParams = m_NewParamsList[i];
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

			bool transTaken = false;
			for (int i = 0; i < m_NewParamsList.Count; i++)
			{
				// This can happen because we need to remove button params from the list sometimes
				Params newParams = m_NewParamsList[i];
				if (newParams == null)
				{
					continue;
				}
#if UNITY_EDITOR
				bool logging = DebugOptions.LogSequencing.IsSet();
				string oldPath = logging ? m_Sequencer.ToString() : null;
#endif
				m_NewParamsList[i] = null; // Remove params from list before new condtions and tracks fire
				transTaken = m_Sequencer.NewParams(newParams);
				if (!transTaken)
				{
					for (int j = 0; j < m_AllTrees.Count; j++)
					{
						TreeRT tree = m_AllTrees[j];
						if (tree.SourceTree.GetInstanceID() == m_Sequencer.Tree.SourceTree.GetInstanceID())
						{
							continue; // If this is our current sequencer's source tree it has already got its opportunity to respond to new params
						}
						newParams.Reset();
						Sequencer newActSequencer = Sequencer.TryCreateAndSequence(Sequencer.ActionType.EventSecondaryTree, tree, newParams);
						if (newActSequencer != null)
						{
							m_Sequencer.StopAndDispose(Sequencer.ActionType.EventSecondaryTree);
							m_Sequencer = newActSequencer;
							m_Sequencer.Play(Sequencer.ActionType.EventSecondaryTree);
							break;
						}
					}
				}
#if UNITY_EDITOR
				if (logging)
				{
					Debug.Log(m_Owner.name + " " + GetType().Name + ".UpdateParams() " + newParams + " \n" +
						(transTaken ? oldPath + " -> " + m_Sequencer.ToString() : "opp NOT taken " + m_Sequencer.ToString()));
				}
#endif
			}

			ClearParams();

			return transTaken;
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
			if (m_ScripedNodeID == Act2.Node.INVALID_ID && string.IsNullOrEmpty(m_ScriptedNodeName))
			{
				if (m_StartNodeID != Act2.Node.INVALID_ID)
				{
					PlayNodeID(m_StartNodeID, m_StartNodeParams);
				}
				else if (!string.IsNullOrEmpty(m_StartNode))
				{
					PlayNodeName(m_StartNode, m_StartNodeParams);
				}
			}

			for (int i = 0; i < m_AllTrees.Count; i++)
			{
				m_AllTrees[i].Initialize();
			}

			// Typically we want to force an update in Start() depending on when this behaviour gets instatiated sometimes Monobehaviour can have
			// Start() called but not Update() so this guarentees the tree will update on the frame we're instatiated
			// In some cases we might have to disable this behaviour, if we want to initialize an ActSequenceController but not have it start playing anything yet
			if (update)
			{
				ForceUpdate();
			}
		}

		private void ReSequence(Sequencer.ActionType sequencerAction = Sequencer.ActionType.Resequence)
		{
			if (m_Sequencer != null)
			{
				m_Sequencer.StopAndDispose(sequencerAction);
			}
			m_Sequencer = null;

			PlayScriptedNode();
			if (m_Sequencer != null)
			{
				return;
			}
			
			for (int i = 0; i < m_AllTrees.Count; i++)
			{
				Sequencer temp = Sequencer.TryCreateAndSequence(sequencerAction, m_AllTrees[i], null, m_Speed);
				if (temp != null)
				{
					m_Sequencer = temp;
					m_Sequencer.Play(sequencerAction);
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
			if (m_ScripedNodeID == Node.INVALID_ID && string.IsNullOrEmpty(m_ScriptedNodeName))
			{
				return;
			}
			if (!m_ScriptedNodeResequence && m_Sequencer != null)
			{
				bool alreadyPlaying =
					(m_ScripedNodeID != Node.INVALID_ID && m_Sequencer.IsPlaying(m_ScripedNodeID)) ||
					(!string.IsNullOrEmpty(m_ScriptedNodeName) && m_Sequencer.IsPlaying(m_ScriptedNodeName));
				if (alreadyPlaying)
				{
					ClearScriptedNode();
					return;
				}
			}
			for (int i = 0; i < m_AllTrees.Count; i++)
			{
				if (m_ScriptedParams != null)
				{
					m_ScriptedParams.Reset();
				}
				TreeRT tree = m_AllTrees[i];
				Sequencer.PlayNode req = m_ScripedNodeID != Node.INVALID_ID ?
					Sequencer.PlayNode.FromID(m_ScripedNodeID, m_ScriptedNodeIgnoreConditons) :
					Sequencer.PlayNode.FromName(tree, m_ScriptedNodeName, m_ScriptedNodeIgnoreConditons);
				Sequencer newActSequencer = Sequencer.TryCreateAndSequenceNode(
					Sequencer.ActionType.ForcedPlayNode,
					tree,
					req,
					m_ScriptedParams,
					m_Speed);
				if (newActSequencer == null)
				{
					continue;
				}
				if (m_Sequencer != null)
				{
					m_Sequencer.StopAndDispose(Sequencer.ActionType.ForcedPlayNode);
				}
				m_Sequencer = newActSequencer;
				m_Sequencer.Play(Sequencer.ActionType.ForcedPlayNode);
				m_Sequencer.Update(0.0f); // Make sure events fire
				break;
			}
			ClearScriptedNode();
		}

		private void ClearScriptedNode()
		{
			m_ScriptedNodeName = string.Empty;
			m_ScripedNodeID = Node.INVALID_ID;
			m_ScriptedParams = null;
			m_ScriptedNodePriority = LOWEST_SCRIPTED_PRIORITY;
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
				ReSequence(Sequencer.ActionType.ForcedResequence);
				return;
			}

			PlayScriptedNode();

			if (UpdateParams())
			{
				return;
			}

			if (m_Sequencer == null)
			{
				ReSequence();
				return;
			}

			float deltaTime = m_UpdateMode == UpdateMode.Realtime ? Time.unscaledDeltaTime : Time.deltaTime;
			bool sequencerAlive = m_Sequencer.Update(deltaTime);

			// Updating our sequencer could disable our owner, in this case don't try to resequence because we won't be getting another update
			if (!m_Owner.isActiveAndEnabled)
			{
				return;
			}

			if (!sequencerAlive)
			{
				ReSequence();
			}
			UpdateParams(); // Tracks might send params that we want to resolve right away
		}

		public void Stop()
		{
			// Make sure we get our end events before destroying
			if (m_Sequencer != null)
			{
				m_Sequencer.StopAndDispose(Sequencer.ActionType.ForcedStop);
			}
			m_Sequencer = null;
			ClearParams();
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
}
