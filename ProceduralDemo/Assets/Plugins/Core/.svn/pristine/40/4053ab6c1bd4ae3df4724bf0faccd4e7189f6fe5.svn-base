
using UnityEngine;
using System.Collections.Generic;

public class ActNodeRT
{
	Act.Node m_Node = null;
	public Act.Node GetNode() { return m_Node; }
	public string GetName() { return m_Node.GetName(); }
	public int GetID() { return m_Node.GetID(); }
	ActTreeRT m_Tree = null;
	ActConditionGroup m_Conditions = null;
	List<ActTrack> m_Tracks = null;
	float m_RandomWeight = -1.0f;
	public float GetRandomWeight() { return m_RandomWeight; }
	bool m_Master = false;
	public bool IsMaster() { return m_Master; }

	public ActNodeRT(Act.Node node, ActTreeRT tree)
	{
		Debug.Assert(node != null, "ActNodeRT() Node cannot be null");
		m_Node = node;
		Debug.Assert(tree != null, "ActNodeRT() Tree cannot be null");
		m_Tree = tree;
		m_Conditions = m_Tree.GetConditions(m_Node.GetID());
		if (m_Conditions != null)
		{
			foreach (ActCondition condition in m_Conditions.GetConditions())
			{
				RandomCondition random = condition as RandomCondition;
				if (random != null)
				{
					m_RandomWeight = random.GetWeight();
					break;
				}
			}
		}
		m_Tracks = m_Tree.GetTracks(m_Node.GetID());
		if (m_Tracks != null)
		{
			foreach (ActTrack track in m_Tracks)
			{
				if (track.IsMaster())
				{
					m_Master = true;
					break;
				}
			}
		}
	}

	public ActNodeRT GetParent()
	{
		return m_Tree.GetParent(m_Node.GetID());
	}

	public List<ActNodeRT> GetChildren()
	{
		return m_Tree.GetChildren(m_Node.GetID());
	}

	public bool IsPointer()
	{
		return m_Tree.GetNode(m_Node.GetPointerID()) != null;
	}

	public ActNodeRT GetPointer()
	{
		return m_Tree.GetNode(m_Node.GetPointerID());
	}

	public int GetChildCount()
	{
		List<ActNodeRT> children = m_Tree.GetChildren(m_Node.GetID());
		return children != null ? children.Count : 0;
	}

	public ActConditionGroup GetConditions()
	{
		return m_Conditions;
	}

	public List<ActTrack> GetTracks()
	{
		return m_Tracks;
	}

	public List<ActSequenceRT> GetOpportunities()
	{
		return m_Tree.GetOpportunities(m_Node.GetID());
	}

	public bool EvaluateConditions(ActParams param, float weightedChance, bool poling)
	{
		if (m_Conditions == null)
		{
			return true;
		}
		if (poling)
		{
			return m_Conditions.PoleConditions(param);
		}
		else
		{
			return m_Conditions.EvaluateConditions(param, weightedChance);
		}
	}

	public override string ToString()
	{
		return m_Node != null ? m_Node.ToString() : "INVALID";
	}

	public ActNodeRT FindNode(int id)
	{
		if (m_Node.GetID() == id)
		{
			return this;
		}

		List<ActNodeRT> children = GetChildren();
		if (children == null)
		{
			return null;
		}

		foreach (ActNodeRT node in children)
		{
			ActNodeRT foundNode = node.FindNode(id);
			if (foundNode != null)
			{
				return foundNode;
			}
		}

		return null;
	}
}
