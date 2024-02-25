
using UnityEngine;
using System.Collections.Generic;

public class ActSequencer
{
	public interface IActSequenceOwner
	{
		string name { get; }
	}

	private static Core.ObjectPool<ActSequencer> s_Pool = new Core.ObjectPool<ActSequencer>();

	public static ActSequencer TryCreateAndSequence(ActTreeRT tree, ActParams actParams, IActSequenceOwner owner, float timeScale = 1.0f)
	{
		return TryCreateAndSequenceInternal(tree, actParams, owner, null, Act.Node.INVALID_ID, timeScale);
	}

	public static ActSequencer TryCreateAndSequenceWithNodeName(ActTreeRT tree, ActParams actParams, IActSequenceOwner owner, string nodeName, float timeScale = 1.0f)
	{
		return TryCreateAndSequenceInternal(tree, actParams, owner, nodeName, Act.Node.INVALID_ID, timeScale);
	}

	public static ActSequencer TryCreateAndSequenceWithNodeID(ActTreeRT tree, ActParams actParams, IActSequenceOwner owner, int nodeID, float timeScale = 1.0f)
	{
		return TryCreateAndSequenceInternal(tree, actParams, owner, null, nodeID, timeScale);
	}

	private static ActSequencer TryCreateAndSequenceInternal(
		ActTreeRT tree, 
		ActParams actParams, 
		IActSequenceOwner owner,
		string nodeName, 
		int nodeID,
		float timeScale)
	{
		ActSequencer seq = s_Pool.Request();
		seq.Initialize(null, tree, tree.GetRoot(), actParams, owner, timeScale);
		if (seq.TrySequenceInternal(nodeName, nodeID))
		{
			return seq;
		}
		seq.Dispose();
		return null;
	}

	ActTreeRT m_Tree = null;
	ActNodeRT m_Node = null;
	ActSequencer m_ChildSequencer = null;
	ActSequencer m_ParentSequencer = null;
	ActParams m_Params = null;
	float m_Timer = -1.0f;
	float m_TimeScale = 1.0f;

	IActSequenceOwner m_Owner = null;
	string m_EditorTreePath = Core.Str.EMPTY;
#if UNITY_EDITOR
	string m_DebugID = null;
	#endif

	public ActTreeRT GetTree() { return m_Tree; }
	public ActNodeRT GetNode() { return m_Node; }
	public ActSequencer GetChildSequencer() { return m_ChildSequencer; }
	public ActParams GetParams() { return m_Params; }
	public float GetTimer() { return m_Timer; }
	public ActSequencer GetLeafSequencer()
	{
		if (m_ChildSequencer == null)
		{
			return this;
		}
		return m_ChildSequencer.GetLeafSequencer();
	}
	public ActSequencer GetRootSequencer()
	{
		if (m_ParentSequencer == null)
		{
			return this;
		}
		return m_ParentSequencer.GetRootSequencer();
	}

	private void Initialize(
		ActSequencer parent,
		ActTreeRT tree,
		ActNodeRT node,
		ActParams actParams,
		IActSequenceOwner owner,
		float timeScale)
	{
		if (tree == null || node == null || actParams == null || owner == null)
		{
			throw new System.ArgumentNullException("ActSequencer.Initialize()");
		}
		m_Tree = tree;
		m_Node = node;
		m_ChildSequencer = null;
		m_ParentSequencer = parent;
		m_Params = actParams;
		m_Owner = owner;
		m_TimeScale = timeScale;
		m_Timer = -1.0f;
#if UNITY_EDITOR
		m_Tree.EditorUpdateFromSource(); // Rebuilds our tree from source if it's dirty
		m_EditorTreePath = parent != null ? Core.Str.Build(parent.m_EditorTreePath, "․", node.GetName()) : tree.GetName();
		m_DebugID = owner != null ? Core.Str.Build(owner.name, "․", tree.GetName()) : Core.Str.EMPTY;
#endif
	}

	private void Dispose()
	{
		// Release all of our references
		m_Tree = null;
		m_Node = null;
		m_ChildSequencer = null;
		m_ParentSequencer = null;
		m_Params = null;
		s_Pool.Return(this);
	}

	public ActSequencer Copy()
	{
		ActSequencer copy = (ActSequencer)MemberwiseClone();
		if (m_ChildSequencer != null)
		{
			copy.m_ChildSequencer = m_ChildSequencer.Copy();
		}
		return copy;
	}

	public override string ToString()
	{
#if UNITY_EDITOR
		ActSequencer leaf = GetLeafSequencer();
		return leaf.m_EditorTreePath;
#else
		return Core.Str.EMPTY;
#endif
	}

	public string GetDebugString()
	{
		ActSequencer seq = this;
		string s = Core.Str.EMPTY;
		while (seq != null && seq.IsPlaying())
		{
			s += seq.m_EditorTreePath + " t: " + seq.m_Timer + "\n    parms: " + seq.m_Params + "\n";
			seq = seq.m_ChildSequencer;
		}
		return s;
	}

	public bool IsPlaying()
	{
		if (m_Timer < 0.0f)
		{
			return false;
		}
		return true;
	}

	public void Play()
	{
		if (IsPlaying())
		{
			Debug.LogError(this + ".Play() " + m_Node.GetName() + " is already being played, call StopPlaying() first.");
			return;
		}

		m_Timer = 0.0f;
		m_Params.Play(m_Node.GetID());
		List<ActTrack> tracks = m_Node.GetTracks();
		if (tracks != null)
		{
			foreach (ActTrack track in tracks)
			{
				track.StateEnter(m_TimeScale, m_Params);
			}
		}
		ActConditionGroup conditions = m_Node.GetConditions();
		if (conditions != null)
		{
			conditions.StateEnter(m_Params);
		}

		if (m_ChildSequencer != null)
		{
			m_ChildSequencer.Play();
		}
#if UNITY_EDITOR
		else if (!Core.Str.IsEmpty(m_DebugID))
		{
			TreeDebuggerData.Get().NewSnapShot(m_DebugID, this, m_Params);
		}
#endif
	}

	bool SwitchChild(
		ActParams actParams,
		bool poling,
		ref float weightedChance,
		ActNodeRT child,
		string forceNode = null,
		int nodeID = Act.Node.INVALID_ID)
	{
		float conditonChance = weightedChance; // Use this weight to test conditions if we make it that far
		weightedChance -= child.GetRandomWeight(); // Remove this nodes weight from chance

		if (nodeID != Act.Node.INVALID_ID)
		{
			// We are looking for a specific node ID, ignore all conditions until we find it
			if (child.GetID() == nodeID)
			{
				nodeID = Act.Node.INVALID_ID; // Invalidate ID as we want to evaluate the rest of the tree normally
			}
			else if (child.FindNode(nodeID) == null)
			{
				return false;
			}
		}
		else
		{
			if (!Core.Str.IsEmpty(forceNode) && !Core.Str.Equals(child.GetName(), forceNode))
			{
				return false;
			}
			if (!child.EvaluateConditions(actParams, conditonChance, poling))
			{
				return false;
			}
		}
		ActSequencer childSequencer = s_Pool.Request();
		childSequencer.Initialize(
			this,
			m_Tree,
			child,
			actParams,
			m_Owner,
			m_TimeScale);
		if (!childSequencer.Sequence(null, nodeID)) // Pass through nodeID but not forceNode
		{
			childSequencer.Dispose();
			return false;
		}
		StopChild();
		m_ChildSequencer?.Dispose();
		m_ChildSequencer = childSequencer;
		return true;
	}

	void StopChild()
	{
		if (m_ChildSequencer != null)
		{
			// Note: A parent stopping a child should always be an interruption. It should be the children that dictate the lifetime of the tree
			m_ChildSequencer.StopSelf(true);
			m_ChildSequencer.Dispose();
			m_ChildSequencer = null;
		}
	}

	public bool NewParams(ActParams actParams)
	{
		if (actParams.GetState() != ActParams.State.None)
		{
			Debug.LogError(this + ".NewParams() Act params " + actParams +
				" have already been processed, they need to be reset before they can be used again");
			return false;
		}
		actParams.Process(); // Params have been processed they've had a chance to try and sequence a node

		bool oppTaken = NewParamsRecursive(actParams);
#if UNITY_EDITOR
		TreeDebuggerData.Get().MissedParams(m_DebugID, actParams);
#endif
		return oppTaken;
	}

	bool NewParamsRecursive(ActParams newParams)
	{
		if (!IsPlaying())
		{
			Debug.LogError(this + ".NewParamsRecursive() Can't update params for " + m_Node.GetName() + " since it's not playing");
			return false;
		}

//		if (!mNode.EvaluateConditions(animParams))
//		{
//			return false;
//		}

		if (m_ChildSequencer == null)
		{
			return false;
		}

		if (m_ChildSequencer.NewParamsRecursive(newParams))
		{
			return true;
		}

		if (TrySequenceNode(newParams, false, false))
		{
			return true;
		}

		return false;
	}

	public bool IsPlaying(string nodeName)
	{
		if (Core.Str.Equals(m_Node.GetName(), nodeName))
		{
			return true;
		}
		if (m_ChildSequencer == null)
		{
			return false;
		}
		return Core.Str.Equals(m_ChildSequencer.m_Node.GetName(), nodeName);
	}

	public bool IsPlaying(int nodeID)
	{
		if (m_Node.GetID() == nodeID)
		{
			return true;
		}
		if (m_ChildSequencer == null)
		{
			return false;
		}
		return m_ChildSequencer.IsPlaying(nodeID);
	}

	bool TrySequenceInternal(string forceNode = null, int nodeID = Act.Node.INVALID_ID)
	{
		if (m_Params.GetState() != ActParams.State.None)
		{
			Debug.LogError(this + ".InitializeInternal() Act params " + m_Params +
			" have already been processed, they need to be reset before they can be used again");
			return false;
		}
		m_Params.Process(); // These params have been used to create a sequencer
		if (!Sequence(forceNode, nodeID))
		{
			return false;
		}
		return true;
	}

	bool Sequence(string forceNode = null, int nodeID = Act.Node.INVALID_ID)
	{
		if (IsPlaying())
		{
			Debug.LogError(this + ".Sequence() Why are you sequencing " + m_Node.GetName() +
				" when it's already playing? You need to call StopPlaying() first");
			return false;
		}

		ActNodeRT pointer = m_Node.GetPointer();
		if (pointer != null)
		{
			float ignoreRandom = 0.0f; // Ignore random conditions for sequences, all randoms will evaluate to true when chance is 0
			if (SwitchChild(m_Params, false, ref ignoreRandom, pointer, forceNode, nodeID))
			{
				return true;
			}

			return false;
		}

		List<ActNodeRT> children = m_Node.GetChildren();
		if (children == null)
		{
			return true; // Found a leaf node to play
		}

		float randomWeight = 0.0f;
		int childCount = children.Count;
		for (int i = 0; i < childCount; i++)
		{
			ActNodeRT child = children[i];
			if (child.GetRandomWeight() > 0.0f)
			{
				randomWeight += child.GetRandomWeight();
			}
		}
		float chance = Random.Range(0.0f, randomWeight);
		for (int i = 0; i < childCount; i++)
		{
			ActNodeRT child = children[i];
			if (SwitchChild(m_Params, false, ref chance, child, forceNode, nodeID))
			{
				return true;
			}
		}
		return m_Node.IsMaster();
	}

	public void StopAndDispose()
	{
		StopSelf(true);
		Dispose();
	}

	void StopSelf(bool interrupted)
	{
		if (!IsPlaying())
		{
			// Make sure we only call end event once
			return;
		}

		m_Params.Stop(m_Node.GetID(), interrupted);
		List<ActTrack> tracks = m_Node.GetTracks();
		if (tracks != null)
		{
			foreach (ActTrack track in tracks)
			{
				track.StateExit(m_Params, interrupted);
			}
		}
		ActConditionGroup conditions = m_Node.GetConditions();
		if (conditions != null)
		{
			conditions.StateExit();
		}

		m_Timer = -1.0f;

		StopChild();

		
#if UNITY_EDITOR
		TreeDebuggerData.Get().NewEmptySnapShot(m_DebugID, this);
#endif
	}

	bool IsAlive()
	{
		List<ActTrack> tracks = m_Node.GetTracks();
		if (tracks != null)
		{
			foreach (ActTrack track in tracks)
			{
				if (track.Alive())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool Update(float deltaTime)
	{
#if UNITY_EDITOR
		m_Tree.EditorUpdateFromSource(); // Rebuilds our tree from source if it's dirty
#endif
		bool playing = UpdateRecursive(deltaTime);
		return playing;
	}

	bool UpdateRecursive(float deltaTime)
	{
		if (!IsPlaying())
		{
			Debug.LogError(this + ".UpdateRecursive() Trying to call Update() on " + m_Node.GetName() +
				" which isn't playing anything, call Play() first");
			return false;
		}

		float scaledDeltaTime = m_TimeScale * deltaTime;
		m_Timer += scaledDeltaTime;
		List<ActTrack> tracks = m_Node.GetTracks();
		if (tracks != null)
		{
			foreach (ActTrack track in tracks)
			{
				track.StateUpdate(m_TimeScale, scaledDeltaTime, m_Timer, m_Params);
			}
		}

		if (m_ChildSequencer == null)
		{
			if (IsAlive())
			{
				return true;
			}
			else
			{
				StopSelf(false);
				return false;
			}
		}

		if (TrySequenceNode(m_Params, false, true))
		{
			return true;
		}

		if (m_ChildSequencer.IsPlaying() && // Since we update our child as soon as it is sequenced (bellow), it may had died by the time we get here
			m_ChildSequencer.UpdateRecursive(deltaTime))
		{
			return true;
		}

		if (TrySequenceNode(m_Params, true, false))
		{
			m_ChildSequencer.Update(0.0f); // Give our new child an update, we want to make sure every node that is playing was updated this frame
			return true;

		}

		if (IsAlive()) // As long as we are alive, keep playing
		{
			m_ChildSequencer.Dispose();
			m_ChildSequencer = null;
			return true;
		}
		StopSelf(false);
		return false;
	}

	bool TrySequenceNode(ActParams actParams, bool onEnd, bool poling)
	{
		List<ActSequenceRT> seqs = m_ChildSequencer.GetNode().GetOpportunities();
		if (seqs == null)
		{
			return false;
		}
		int seqCount = seqs.Count;
		for (int i = 0; i < seqCount; i++)
		{
			ActSequenceRT seq = seqs[i];
			if ((onEnd && seq.GetSequence().IsAvailableOnEnd()) ||
				(!onEnd && seq.GetSequence().IsAvailable(m_ChildSequencer.m_Timer)))
			{
				float chance = 0.0f; // Ignore random conditions for sequences, all randoms will evaluate to true when chance is 0
				if (SwitchChild(actParams, poling, ref chance, seq.GetNode()))
				{
					m_ChildSequencer.Play();
					return true;
				}
			}
		}
		return false;
	}

	public void SetTimeScale(float scale)
	{
		m_TimeScale = scale;
		if (m_ChildSequencer != null)
		{
			m_ChildSequencer.SetTimeScale(scale);
		}
	}

	public void SetNodeTimeScale(float scale, int nodeID)
	{
		if (m_Node != null &&
			m_Node.GetID() == nodeID)
		{
			SetTimeScale(scale);
		}
		else if (m_ChildSequencer != null)
		{
			m_ChildSequencer.SetNodeTimeScale(scale, nodeID);
		}
	}

	public void ClearTimeScale()
	{
		SetTimeScale(1.0f);
	}
}
