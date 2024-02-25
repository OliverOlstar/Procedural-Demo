
using UnityEngine;
using System.Collections.Generic;

namespace Act2
{
	public class Sequencer
	{
		public enum ActionType
		{
			Resequence,
			TransitionPoling,
			TransitionOnEnd,
			EventTransition,
			EventSecondaryTree,
			ForcedPlayNode,
			ForcedResequence,
			ForcedStop,
		}

		public struct PlayNode
		{
			/// <param name="ignoreConditions">Ignore all conditions until requested node is found. Note: Conditions on children of requested node will still be evaluated</param>
			public static PlayNode FromID(int nodeID, bool ignoreConditions = false)
			{
				PlayNode output = new PlayNode(string.Empty, nodeID, ignoreConditions);
				return output;
			}

			/// <param name="ignoreConditions">Ignore all conditions until requested node is found. Note: Conditions on children of requested node will still be evaluated</param>
			public static PlayNode FromName(TreeRT tree, string nodeName, bool ignoreConditions = false)
			{
				tree.TryGetNodeIDFromName(nodeName, out int nodeID);
				PlayNode output = new PlayNode(nodeName, nodeID, ignoreConditions);
				return output;
			}

			public string NodeName;
			public int NodeID;
			public bool IgnoreConditions;

			private PlayNode(string nodeName, int nodeID, bool ignoreConditions)
			{
				NodeName = nodeName;
				NodeID = nodeID;
				IgnoreConditions = ignoreConditions;
			}

			public bool IsValid() => NodeID != Act2.Node.INVALID_ID;

			public override string ToString()
			{
				string nodeID = IsValid() ? NodeID.ToString() : "INVALID";
				return !string.IsNullOrEmpty(NodeName) ? $"RequestNode({NodeName}:{nodeID}" : $"RequestNode({nodeID})";
			}
		}

		private static Core.ObjectPool<Sequencer> s_Pool = new Core.ObjectPool<Sequencer>();

		public static Sequencer TryCreateAndSequence(ActionType action, TreeRT tree, Params actParams = null, float timeScale = 1.0f)
		{
			return TryCreateAndSequenceInternal(action, tree, actParams, null, timeScale);
		}

		public static Sequencer TryCreateAndSequenceNode(
			ActionType action,
			TreeRT tree,
			PlayNode node,
			Params actParams,
			float timeScale = 1.0f)
		{
			return TryCreateAndSequenceInternal(action, tree, actParams, node, timeScale);
		}

		private static Sequencer TryCreateAndSequenceInternal(
			ActionType action,
			TreeRT tree,
			Params actParams = null,
			PlayNode? playNode = null,
			float timeScale = 1.0f)
		{
			if (playNode.HasValue && !playNode.Value.IsValid())
			{
				return null; // Early out instead of trying to sequence a node we know isn't valid
			}
			Sequencer seq = s_Pool.Request();
			seq.Initialize(null, tree, tree.RootNode, actParams, timeScale);
			if (seq.TrySequenceInternal(action, playNode))
			{
				return seq;
			}
			seq.Dispose();
			return null;
		}

		private TreeRT m_Tree = null;
		private NodeRT m_Node = null;
		private Sequencer m_ChildSequencer = null;
		private Sequencer m_ParentSequencer = null;
		private Params m_Params = null;
		private ITreeEvent m_Event = null;
		private float m_Timer = -1.0f;
		private float m_TimeScale = 1.0f;
		private TrackPlayer m_TrackPlayer = new();

		private string m_EditorTreePath = Core.Str.EMPTY;

		public TreeRT Tree => m_Tree;
		public NodeRT Node => m_Node;
		public Sequencer ChildSequencer => m_ChildSequencer;
		public bool IsLeaf() => m_ChildSequencer == null;
		public Sequencer ParentSequencer => m_ParentSequencer;
		public Params Params => m_Params;
		public float Timer => m_Timer;
		public bool IsPlaying() => !(m_Timer < 0.0f);
		public string _EditorTreePath => m_EditorTreePath;

		public Sequencer GetLeafSequencer()
		{
			if (m_ChildSequencer == null)
			{
				return this;
			}
			return m_ChildSequencer.GetLeafSequencer();
		}
		public Sequencer GetRootSequencer()
		{
			if (m_ParentSequencer == null)
			{
				return this;
			}
			return m_ParentSequencer.GetRootSequencer();
		}

		private void Initialize(
			Sequencer parent,
			TreeRT tree,
			NodeRT node,
			Params actParams,
			float timeScale)
		{
			if (tree == null || node == null)
			{
				throw new System.ArgumentNullException("ActSequencer.Initialize()");
			}
			m_Tree = tree;
			m_Node = node;
			m_ChildSequencer = null;
			m_ParentSequencer = parent;
			m_Params = actParams;
			m_Event = m_Params != null ? m_Params.Event : null;
			m_TimeScale = timeScale;
			m_Timer = -1.0f;
#if UNITY_EDITOR
			m_Tree._EditorRebuildIfDirty();
			m_EditorTreePath = parent != null ? Core.Str.Build(parent.m_EditorTreePath, ".", node.Name) : tree.Name;
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
			m_Event = null;
			s_Pool.Return(this);
		}

		public Sequencer Copy()
		{
			Sequencer copy = (Sequencer)MemberwiseClone();
			if (m_ChildSequencer != null)
			{
				copy.m_ChildSequencer = m_ChildSequencer.Copy();
			}
			return copy;
		}

#if UNITY_EDITOR
		public override string ToString()
		{
			Sequencer leaf = GetLeafSequencer();
			string path = leaf.m_EditorTreePath;
			path = path.Insert(m_EditorTreePath.Length, "*");
			return $"[{m_Tree.Owner.name},{m_Tree.Name}].{path}";
		}
#endif

		public string GetDebugString()
		{
			Sequencer seq = this;
			string s = Core.Str.EMPTY;
			while (seq != null && seq.IsPlaying())
			{
				s += seq.m_EditorTreePath + " t: " + seq.m_Timer + "\n    event: " + 
					(seq.m_Event == null ? "none" : seq.m_Event) + "\n";
				seq = seq.m_ChildSequencer;
			}
			return s;
		}

		public void Play(ActionType action, TransitionRT transitionTaken = null)
		{
			if (IsPlaying())
			{
				Debug.LogError(this + ".Play() " + m_Node.Name + " is already being played, call StopPlaying() first.");
				return;
			}

			m_Timer = 0.0f;
			m_Params?.Play(m_Node.ID);
			// I think order is important here, makes sense if conditions have to do anything on start they go before tracks
			if (m_Node.TryGetConditions(out ConditionSet conditions))
			{
				conditions.StateEnter(m_Event);
			}
			m_TrackPlayer.StateEnter(m_Node.Tracks, m_TimeScale, m_Event);

			if (m_ChildSequencer != null)
			{
				m_ChildSequencer.Play(action, transitionTaken);
			}
#if UNITY_EDITOR
			TreeDebuggerData.Get().OnPlay(this, action, m_Event, transitionTaken);
#endif
		}

		private Sequencer TrySequenceChild(
			Params parentParams,
			ActionType action,
			ref float weightedChance,
			NodeRT child,
			PlayNode? playNode = null,
			bool timeSliced = false)
		{
			float conditonChance = weightedChance; // Use this weight to test conditions if we make it that far
			weightedChance -= child.GetRandomWeight(); // Remove this nodes weight from chance

			bool evaluateConditions = true;
			if (playNode.HasValue)
			{
				evaluateConditions = !playNode.Value.IgnoreConditions; // Sometimes we want to ignore all conditions until we find the node we want to play
				if (child.ID == playNode.Value.NodeID)
				{
					playNode = null; // Invalidate ID as we want to evaluate the rest of the tree normally
				}
				else if (child.FindNode(playNode.Value.NodeID) == null)
				{
					return null;
				}
			}

			Params childParams = parentParams;
			if (evaluateConditions)
			{
				if (!child.IsAvailable(parentParams, action, out childParams))
				{
					return null;
				}
				if (!child.EvaluateConditions(childParams, conditonChance, timeSliced))
				{
					return null;
				}
			}

			Sequencer childSequencer = s_Pool.Request();
			childSequencer.Initialize(
				this,
				m_Tree,
				child,
				childParams,
				m_TimeScale);
			if (!childSequencer.Sequence(action, playNode))
			{
				childSequencer.Dispose();
				return null;
			}
			return childSequencer;
		}

		public bool NewParams(Params actParams)
		{
			if (actParams.GetState() != Act2.Params.State.None)
			{
				Debug.LogError(this + ".NewParams() Act params " + actParams +
					" have already been processed, they need to be reset before they can be used again");
				return false;
			}
			bool transTaken = NewParamsRecursive(actParams);
			actParams.Process(); // Params have been processed they've had a chance to try and sequence a node
#if UNITY_EDITOR
			if (!transTaken) TreeDebuggerData.Get().MissedParams(this, actParams);
#endif
			return transTaken;
		}

		private bool NewParamsRecursive(Params newParams)
		{
			if (!IsPlaying())
			{
				Debug.LogError(this + ".NewParamsRecursive() Can't update params for " + m_Node.Name + " since it's not playing");
				return false;
			}
			if (m_ChildSequencer == null)
			{
				return m_Node.TryHandleEvent(newParams);
			}
			if (m_ChildSequencer.NewParamsRecursive(newParams))
			{
				return true;
			}
			if (m_Node.TryHandleEvent(newParams))
			{
				return true;
			}
			if (TryTransitionChildSequencer(ActionType.EventTransition, newParams))
			{
				return true;
			}
			return false;
		}

		public bool IsPlaying(string nodeName)
		{
			if (Core.Str.Equals(m_Node.Name, nodeName))
			{
				return true;
			}
			if (m_ChildSequencer == null)
			{
				return false;
			}
			return Core.Str.Equals(m_ChildSequencer.m_Node.Name, nodeName);
		}

		public bool IsPlaying(int nodeID)
		{
			if (m_Node.ID == nodeID)
			{
				return true;
			}
			if (m_ChildSequencer == null)
			{
				return false;
			}
			return m_ChildSequencer.IsPlaying(nodeID);
		}

		private bool TrySequenceInternal(ActionType action, PlayNode? playNode = null)
		{
			if (m_Params != null)
			{
				if (m_Params.GetState() != Act2.Params.State.None)
				{
					Debug.LogError(this + ".InitializeInternal() Act params " + m_Params +
					" have already been processed, they need to be reset before they can be used again");
					return false;
				}
				m_Params.Process(); // These params have been used to create a sequencer
			}
			if (!Sequence(action, playNode))
			{
				return false;
			}
			return true;
		}

		private bool Sequence(ActionType action, PlayNode? playNode = null)
		{
			if (IsPlaying())
			{
				Debug.LogError(this + ".Sequence() Why are you sequencing " + m_Node.Name +
					" when it's already playing? You need to call StopPlaying() first");
				return false;
			}

			if (m_Node.TryGetPointer(out NodeRT pointer))
			{
				float ignoreRandom = 0.0f; // Ignore random conditions for sequences, all randoms will evaluate to true when chance is 0
				Sequencer pointerSequencer = TrySequenceChild(m_Params, action, ref ignoreRandom, pointer, playNode);
				if (pointerSequencer != null)
				{
					m_ChildSequencer = pointerSequencer;
					return true;
				}
				return m_Node.HasMajorTrack();
			}

			if (!m_Node.TryGetChildren(out IReadOnlyList<NodeRT> children))
			{
				return true; // Found a leaf node to play
			}

			float randomWeight = 0.0f;
			int childCount = children.Count;
			for (int i = 0; i < childCount; i++)
			{
				NodeRT child = children[i];
				if (child.IsAvailableOnEntry() && child.GetRandomWeight() > 0.0f)
				{
					randomWeight += child.GetRandomWeight();
				}
			}
			float chance = Random.Range(0.0f, randomWeight);
			for (int i = 0; i < childCount; i++)
			{
				NodeRT child = children[i];
				if (!child.IsAvailableOnEntry())
				{
					continue;
				}
				Sequencer childSequencer = TrySequenceChild(m_Params, action, ref chance, child, playNode);
				if (childSequencer != null)
				{
					m_ChildSequencer = childSequencer;
					return true;
				}
			}
			return m_Node.HasMajorTrack();
		}

		public void StopAndDispose(ActionType action)
		{
			StopInternal(action);
			Dispose();
		}

		private void StopInternal(ActionType action)
		{
			if (!IsPlaying())
			{
				// Make sure we only call end event once
				return;
			}

			bool interrupted = action != ActionType.Resequence;
			m_Params?.Stop(m_Node.ID, interrupted);
			m_TrackPlayer.StateExit(interrupted);
			if (m_Node.TryGetConditions(out ConditionSet conditions))
			{
				conditions.StateExit();
			}

#if UNITY_EDITOR
			TreeDebuggerData.Get().OnStop(this, action);
#endif

			m_Timer = -1.0f;

			if (m_ChildSequencer != null)
			{
				m_ChildSequencer.StopInternal(action);
				m_ChildSequencer.Dispose();
				m_ChildSequencer = null;
			}
		}

		public bool Update(float deltaTime)
		{
#if UNITY_EDITOR
			m_Tree._EditorRebuildIfDirty(); // Rebuilds our tree from source if it's dirty
#endif
			bool playing = UpdateRecursive(deltaTime);
			return playing;
		}

		private bool UpdateRecursive(float deltaTime)
		{
			if (!IsPlaying())
			{
				Debug.LogError(this + ".UpdateRecursive() Trying to call Update() on " + m_Node.Name +
					" which isn't playing anything, call Play() first");
				return false;
			}

			float scaledDeltaTime = m_TimeScale * deltaTime;
			m_Timer += scaledDeltaTime;
			for (int i = 0; i < m_TrackPlayer.TrackCount; i++)
			{
				m_TrackPlayer.UpdateIndividual(i, m_Timer, scaledDeltaTime, m_TimeScale);
				if (!IsPlaying())
				{
					return false; // It's possible a track could stop this sequencer, for example by disabling the GameObject it's attached to
				}
			}
			bool alive = m_TrackPlayer.KeepAlive();
			if (m_ChildSequencer == null)
			{
				if (alive)
				{
					return true;
				}
				else
				{
					StopInternal(ActionType.Resequence);
					return false;
				}
			}

			// Minor nodes cannot keep a sequencer alive, but as long as we're alive we'll let a minor child keep playing
			if (!alive && m_ChildSequencer.m_Node.SourceNode.NodeType == NodeType.Minor)
			{
				return false;
			}

			if (TryTransitionChildSequencer(ActionType.TransitionPoling, null, true))
			{
				return true;
			}

			if (m_ChildSequencer.IsPlaying()) // Since we update with 0.0 delta when transition to a new child, it may had died by the time we get here
			{
				if (m_ChildSequencer.UpdateRecursive(deltaTime))
				{
					return true;
				}
				if (!IsPlaying())
				{
					return false; // It's possible our child could stop this sequencer, for example by disabling the GameObject it's attached to
				}
			}

			// Content has stopped playing try to find an "On End" transition to take
			if (TryTransitionChildSequencer(ActionType.TransitionOnEnd))
			{
				m_ChildSequencer.Update(0.0f); // Give our new child an update, we want to make sure every node that is playing was updated this frame
				return true;
			}

			if (alive) // As long as we are alive, keep playing
			{
				m_ChildSequencer.Dispose();
				m_ChildSequencer = null;
				return true;
			}
			StopInternal(ActionType.Resequence);
			return false;
		}

		private bool TryTransitionChildSequencer(ActionType action, Params newParams = null, bool timeSliced = false)
		{
			Params actParams = newParams != null ? newParams : m_ChildSequencer.m_Params;
			if (!m_ChildSequencer.Node.TryGetTransitions(out IReadOnlyList<TransitionRT> transitions))
			{
				return false;
			}
			int transCount = transitions.Count;
			for (int i = 0; i < transCount; i++)
			{
				float time = m_ChildSequencer.m_Timer;
				TransitionRT trans = transitions[i];
				if (!trans.IsAvailable(action, time))
				{
					continue;
				}
				float chance = 0.0f; // Ignore random conditions for sequences, all randoms will evaluate to true when chance is 0
				Sequencer transitionSequencer = TrySequenceChild(actParams, action, ref chance, trans.ToNode, null, timeSliced);
				if (transitionSequencer == null)
				{
					continue;
				}
				m_ChildSequencer.StopInternal(action); // Note: In the case we're trying to transition "On End" child sequencer will already be stopped
				m_ChildSequencer.Dispose();
				m_ChildSequencer = transitionSequencer;
				m_ChildSequencer.Play(action, trans);
				return true;
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
			if (m_Node != null && m_Node.ID == nodeID)
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
}
