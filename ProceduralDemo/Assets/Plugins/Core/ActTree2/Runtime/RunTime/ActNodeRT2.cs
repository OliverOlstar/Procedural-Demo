
using UnityEngine;
using System.Collections.Generic;

namespace Act2
{
	public class NodeRT : IActNodeRuntime
	{
		private Node m_SourceNode = null;
		public Node SourceNode => m_SourceNode;
		public string Name => m_SourceNode.Name;
		public int ID => m_SourceNode.GetID();
		public bool IsEventRequired(out System.Type requiredEvent) => m_SourceNode.IsEventRequired(out requiredEvent);

		private TreeRT m_Tree = null;

		private ConditionSet m_Conditions = null;
		public float GetRandomWeight() => m_Conditions != null ? m_Conditions.GetRandomWeight() : -1.0f;

		private TrackList m_Tracks = null;
		public TrackList Tracks => m_Tracks;
		public bool HasMajorTrack() => m_Tracks.HasMajorTrack;
		public bool TryHandleEvent(Params newParams) => m_Tracks.TryHandleEvent(newParams);

		public NodeRT(Node node, TreeRT tree)
		{
			Debug.Assert(node != null, "ActNodeRT() Node cannot be null");
			m_SourceNode = node;
			Debug.Assert(tree != null, "ActNodeRT() Tree cannot be null");
			m_Tree = tree;

			if (node.Conditions.Count > 0)
			{
				m_Conditions = new ConditionSet(m_SourceNode);
			}
			m_Tracks = new TrackList(m_SourceNode);
		}

		public void Initialize(TreeRT tree, ITreeContext context)
		{
			if (m_Conditions != null)
			{
				m_Conditions.Initialize(tree.SourceTree, this, context);
			}
			m_Tracks.Initialize(tree.SourceTree, this, context);
		}

		public NodeRT GetParent() => m_Tree.GetParent(m_SourceNode.GetID());
		public bool TryGetChildren(out IReadOnlyList<NodeRT> nodes) => m_Tree.TryGetChildren(m_SourceNode.GetID(), out nodes);
		public int GetChildCount() => m_Tree.TryGetChildren(m_SourceNode.GetID(), out IReadOnlyList<NodeRT> children) ? children.Count : 0;

		public bool IsPointer() => m_Tree.TryGetNode(m_SourceNode.GetPointerID(), out _);
		public bool TryGetPointer(out NodeRT pointerNode) => m_Tree.TryGetNode(m_SourceNode.GetPointerID(), out pointerNode);
		
		public bool HasConditions() => m_Conditions != null;
		public bool TryGetConditions(out ConditionSet conditions)
		{
			conditions = m_Conditions;
			return conditions != null;
		}

		public bool TryGetTransitions(out IReadOnlyList<TransitionRT> transitions) => m_Tree.TryGetTransitions(m_SourceNode.GetID(), out transitions);

		public bool EvaluateConditions(Params param, float weightedChance, bool timeSliced)
		{
			if (m_Conditions == null)
			{
				return true;
			}
			return m_Conditions.EvaluateConditions(param != null ? param.Event : null, weightedChance, timeSliced);
		}
		public bool EvaluateConditions(ITreeEvent treeEvent, float weightedChance = 0.0f, bool timeSliced = false)
		{
			if (m_Conditions == null)
			{
				return true;
			}
			return m_Conditions.EvaluateConditions(treeEvent, weightedChance, timeSliced);
		}

		public bool IsAvailableOnEntry() => m_SourceNode.Availability == Node.Available.Entry;
		public bool IsAvailableOnlyOnEventTransition() => m_SourceNode.Availability == Node.Available.EventTransitionsOnly;

		public bool IsAvailable(Params sequencerParams, Sequencer.ActionType sequencerAction, out Params nodeParams)
		{
			if (!m_SourceNode.IsEventRequired(out System.Type requiredEventType))
			{
				nodeParams = null; // If we're passed an event but don't require one then don't propagate it
				return true;
			}
			// If we require a certain event then make sure this event is a match
			// Poling may have a propagated event, or event may be null
			// This is basically a chance to early out as we know these conditions will fail without the correct event
			nodeParams = sequencerParams;
			return sequencerParams != null && requiredEventType.IsAssignableFrom(sequencerParams.Event.GetType());
		}

		public NodeRT FindNode(int id)
		{
			if (m_SourceNode.GetID() == id)
			{
				return this;
			}
			if (!TryGetChildren(out IReadOnlyList<NodeRT> children))
			{
				return null;
			}
			foreach (NodeRT node in children)
			{
				NodeRT foundNode = node.FindNode(id);
				if (foundNode != null)
				{
					return foundNode;
				}
			}
			return null;
		}

		public override string ToString()
		{
			return m_SourceNode != null ? m_SourceNode.ToString() : "INVALID";
		}
	}
}
