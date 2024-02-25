
using UnityEngine;
using System.Collections.Generic;

public class ActTreeRT
{
	private ActTree m_SourceTree = null;
	public ActTree GetSourceTree() { return m_SourceTree; }
	private string m_Name = Core.Str.EMPTY;
	private ActNodeRT m_Root = null;
	private double m_CreationTime = 0.0;
	Dictionary<int, ActNodeRT> m_Nodes = new Dictionary<int, ActNodeRT>();
	Dictionary<int, ActConditionGroup> m_Conditions = new Dictionary<int, ActConditionGroup>();
	Dictionary<int, List<ActTrack>> m_Tracks = new Dictionary<int, List<ActTrack>>();
	Dictionary<int, List<ActNodeRT>> m_Children = new Dictionary<int, List<ActNodeRT>>();
	Dictionary<int, ActNodeRT> m_Parents = new Dictionary<int, ActNodeRT>();
	Dictionary<int, List<ActSequenceRT>> m_Opportunities = new Dictionary<int, List<ActSequenceRT>>();

	public ActNodeRT GetRoot() { return m_Root; }
	public string GetName() { return m_Name; }

	public ActNodeRT GetNode(int nodeID)
	{
		ActNodeRT node = null;
		m_Nodes.TryGetValue(nodeID, out node);
		return node;
	}
	public ActNodeRT GetNode(string nodeName)
	{
		// This function only searches top level nodes, this is because we only support playing top level nodes by name
		// String comparing through a huge tree is expensive so it is deliberately disallowed
		List<ActNodeRT> children = m_Root.GetChildren();
		if (children == null)
		{
			return null;
		}
		foreach (ActNodeRT child in children)
		{
			if (string.Equals(child.GetName(), nodeName))
			{
				return child;
			}
		}
		return null;
	}
	public ActNodeRT GetParent(int nodeID)
	{
		ActNodeRT node = null;
		m_Parents.TryGetValue(nodeID, out node);
		return node;
	}
	public List<ActNodeRT> GetChildren(int nodeID)
	{
		List<ActNodeRT> nodes = null;
		m_Children.TryGetValue(nodeID, out nodes);
		return nodes;
	}

	public ActConditionGroup GetConditions(int nodeID)
	{
		ActConditionGroup conds = null;
		m_Conditions.TryGetValue(nodeID, out conds);
		return conds;
	}
	public List<ActTrack> GetTracks(int nodeID)
	{
		List<ActTrack> tracks = null;
		m_Tracks.TryGetValue(nodeID, out tracks);
		return tracks;
	}

	public List<ActSequenceRT> GetOpportunities(int nodeID)
	{
		List<ActSequenceRT> opps = null;
		m_Opportunities.TryGetValue(nodeID, out opps);
		return opps;
	}

	public ActTreeRT(ActTree tree)
	{
		m_SourceTree = tree ?? throw new System.ArgumentNullException("ActTreeRT() ActTree is null, a reference to an ActTree is probably unassigned somewhere");
		BuildTree();
	}

	public ActTreeRT(ActTree tree, ActParams defaultParams)
	{
		m_SourceTree = tree ?? throw new System.ArgumentNullException("ActTreeRT() ActTree is null, a reference to an ActTree is probably unassigned somewhere");
		BuildTree();
		Initialize(defaultParams);
	}

	private void BuildTree()
	{
		m_CreationTime = ActTreeDirtyTimestamps.Timestamp;
		m_Name = m_SourceTree.name;
		m_Root = new ActNodeRT(m_SourceTree.GetRootNode(), this);
		m_Nodes.Add(m_Root.GetID(), m_Root);

		foreach (ActCondition cond in m_SourceTree.GetConditions())
		{
			if (cond == null)
			{
				Debug.LogWarning($"ActTreeRT.BuildTree() Null condition in tree {m_SourceTree.name}.");
				continue;
			}
			ActConditionGroup conds = null;
			if (!m_Conditions.TryGetValue(cond.GetNodeID(), out conds))
			{
				conds = new ActConditionGroup();
				m_Conditions.Add(cond.GetNodeID(), conds);
			}
			conds.Add(Object.Instantiate<ActCondition>(cond));
		}

		foreach (ActTrack track in m_SourceTree.GetTracks())
		{
			if (track == null)
			{
				Debug.LogWarning($"ActTreeRT.BuildTree() Null track in tree {m_SourceTree.name}.");
				continue;
			}
			if (!track.IsActive())
			{
				continue;
			}
			List<ActTrack> tracks = null;
			if (!m_Tracks.TryGetValue(track.GetNodeID(), out tracks))
			{
				tracks = new List<ActTrack>();
				m_Tracks.Add(track.GetNodeID(), tracks);
			}
			tracks.Add(Object.Instantiate<ActTrack>(track));
		}

		foreach (Act.Node node in m_SourceTree.GetAllNodes())
		{
			if (m_Nodes.ContainsKey(node.GetID()))
			{
				Debug.LogError(Core.Str.Build(m_Name, " ActTreeRT() Duplicate node ID ", node.GetID().ToString()));
			}
			else
			{
				m_Nodes.Add(node.GetID(), new ActNodeRT(node, this));
			}
		}

		foreach (Act.NodeLink link in m_SourceTree.GetAllNodeLinks())
		{
			ActNodeRT parent = GetNode(link.GetParentID());
			if (parent == null)
			{
				Debug.LogWarning(Core.Str.Build(m_Name, " ActTreeRT() Link to invalid parent ", link.GetParentID().ToString()));
				continue;
			}
			ActNodeRT child = GetNode(link.GetChildID());
			if (child == null)
			{
				Debug.LogWarning(Core.Str.Build(m_Name, " ActTreeRT() Link to invalid child ", link.GetChildID().ToString()));
				continue;
			}

			List<ActNodeRT> children = null;
			if (!m_Children.TryGetValue(link.GetParentID(), out children))
			{
				children = new List<ActNodeRT>();
				m_Children.Add(link.GetParentID(), children);
			}
			children.Add(child);

			if (!m_Parents.ContainsKey(link.GetChildID()))
			{
				m_Parents.Add(link.GetChildID(), parent);
			}
			else
			{
				Debug.LogError(Core.Str.Build(m_Name, " ActTreeRT() Node ", link.GetChildID().ToString(), " has multiple parents"));
			}
		}

		foreach (Act.NodeSequence opp in m_SourceTree.GetAllOpportunities())
		{
			ActNodeRT node = GetNode(opp.GetToID());
			if (node == null)
			{
				// TODO: Need to actually fix orphaned opps.
				//Debug.LogWarning(Core.Str.Build(m_Name, " ActTreeRT() Opportunity to invalid node ", opp.GetToID().ToString()));
				continue;
			}
			List<ActSequenceRT> opps = null;
			if (!m_Opportunities.TryGetValue(opp.GetFromID(), out opps))
			{
				opps = new List<ActSequenceRT>();
				m_Opportunities.Add(opp.GetFromID(), opps);
			}
			opps.Add(new ActSequenceRT(opp, node));
		}
	}

	public void Initialize(ActParams defaultParams)
	{
#if UNITY_EDITOR
		m_EditorReinitializeParams = defaultParams ?? throw new System.ArgumentNullException("ActTreeRT.Initialize() Cannot initialize with null params");
#endif
		foreach (ActNodeRT node in m_Nodes.Values)
		{
			ActConditionGroup conds = null;
			if (m_Conditions.TryGetValue(node.GetID(), out conds))
			{
				conds.Initialize(this, node, defaultParams);
			}

			List<ActTrack> tracks = null;
			if (m_Tracks.TryGetValue(node.GetID(), out tracks))
			{
				for (int i = 0; i < tracks.Count; i++)
				{
					ActTrack track = tracks[i];
					if (!track.Initialize(this, node, defaultParams))
					{
						if (track.IsMaster())
						{
							Debug.LogWarning(m_SourceTree.name + " ActTreeRT.Initialize() Master track " + tracks[i].name + " in " + node +
								" failed to initialize and will be removed from the tree, removing a master track will break the intented logic of this tree");
						}
						Object.Destroy(track);
						tracks.RemoveAt(i);
						i--;
					}
				}
			}
		}
	}

	public void Destroy()
	{
		foreach (ActConditionGroup conditionGroup in m_Conditions.Values)
			while (conditionGroup.GetConditions().Count > 0)
			{
				Object.Destroy(conditionGroup.GetConditions()[0]);
				conditionGroup.GetConditions().RemoveAt(0);
			}
		m_Conditions.Clear();

		foreach (List<ActTrack> tracks in m_Tracks.Values)
			while (tracks.Count > 0)
			{
				Object.Destroy(tracks[0]);
				tracks.RemoveAt(0);
			}
		m_Tracks.Clear();
		
		m_Root = null;
		m_Nodes.Clear();
		m_Children.Clear();
		m_Parents.Clear();
		m_Opportunities.Clear();
	}

#if UNITY_EDITOR
	private ActParams m_EditorReinitializeParams = null;

	public void EditorUpdateFromSource()
	{
		if (ActTreeDirtyTimestamps.IsDirty(m_SourceTree, m_CreationTime))
		{
			m_Conditions.Clear();
			m_Tracks.Clear();
			m_Nodes.Clear();
			m_Children.Clear();
			m_Parents.Clear();
			m_Opportunities.Clear();
			BuildTree();
			if (m_EditorReinitializeParams != null)
			{
				Initialize(m_EditorReinitializeParams);
			}
		}
	}
#endif
}

public class ActSequenceRT
{
	Act.NodeSequence m_Sequence = null;
	public Act.NodeSequence GetSequence() { return m_Sequence; }
	ActNodeRT m_Node = null;
	public ActNodeRT GetNode() { return m_Node; }

	public ActSequenceRT(Act.NodeSequence sequence, ActNodeRT node)
	{
		m_Sequence = sequence;
		m_Node = node;
	}
}
