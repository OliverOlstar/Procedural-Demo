
using UnityEngine;
using System.Collections.Generic;
using Act;

[ScriptoGraph.Hide]
[CreateAssetMenu(fileName = "New ActTree", menuName = "Act Tree", order = 100)]
public class ActTree : ScriptableObject
{
	[SerializeField]
	List<Node> m_Nodes = new List<Node>();
	public List<Node> GetAllNodes() { return m_Nodes; }

	[SerializeField]
	List<NodeLink> m_NodeLinks = new List<NodeLink>();
	public List<NodeLink> GetAllNodeLinks() { return m_NodeLinks; }

	[SerializeField]
	List<NodeSequence> m_Opportunities = new List<NodeSequence>();
	public List<NodeSequence> GetAllOpportunities() { return m_Opportunities; }

	[SerializeField]
	List<ActCondition> m_Conditions = new List<ActCondition>();
	public List<ActCondition> GetConditions() { return m_Conditions; }

	[SerializeField]
	List<ActTrack> m_Tracks = new List<ActTrack>();
	public List<ActTrack> GetTracks() { return m_Tracks; }

	public string[] m_VarNames = {};
	public int[] m_VarValues = {};

	[SerializeField]
	Node m_RootNode = new Node(Node.ROOT_ID);
	public Node GetRootNode() { return m_RootNode; }

	// This is used by editor code to generate unique IDs for nodes
	[SerializeField]
	int m_NextNodeID = Node.ROOT_ID + 1;
	private int DontGetNextNodeID() { return m_NextNodeID; } // This just exists to supress compiler warnings

	public virtual int GetContextMask() { return 0x7FFFFFFF; }

	public Node GetNode(int id)
	{
		if (id == m_RootNode.GetID())
		{
			return m_RootNode;
		}
		foreach (Node node in m_Nodes)
		{
			if (node.GetID() == id)
			{
				return node;
			}
		}
		return null;
	}

	public IEnumerable<Act.ITrack> GetTrackAndOpportunities()
	{
		foreach (ActTrack track in m_Tracks)
		{
			yield return track;
		}
		foreach (Act.NodeSequence seq in m_Opportunities)
		{
			yield return seq;
		}
	}
}

namespace Act
{
	[System.Serializable]
	public class NodeLink
	{
		[SerializeField]
		int m_ParentID = 0;
		public int GetParentID() { return m_ParentID; }

		[SerializeField]
		int m_ChildID = 0;
		public int GetChildID() { return m_ChildID; }

		public override string ToString()
		{
			return "NodeLink(" + m_ParentID + "," + m_ChildID + ")";
		}
	}

	[System.Serializable]
	public class NodeSequence : Act.ITrack
	{
		[SerializeField]
		int m_FromID = 0;
		public int GetFromID() { return m_FromID; }

		[SerializeField]
		int m_ToID = 0;
		public int GetToID() { return m_ToID; }

		[SerializeField]
		float m_StartTime = 0.0f;
		public float GetStartTime() { return m_StartTime; }
		public bool IsAvailableOnEnd() { return m_StartTime < 0.0f; }

		[SerializeField]
		float m_EndTime = -1.0f;
		public float GetEndTime() { return m_EndTime; }
		public bool HasNegativeEndTime() { return m_EndTime < 0.0f; }
		
		public virtual void _EditorAddSubTimes(List<float> times) { }

		public ActTrack.EndEventType GetEndEventType() { return IsAvailableOnEnd() ? ActTrack.EndEventType.NoEndEvent : ActTrack.EndEventType.EndTime; }

		public bool IsAvailable(float time)
		{
			if (m_StartTime < 0.0f) // Only available on end
			{
				return false;
			}
			if (time < m_StartTime)
			{
				return false;
			}
			if (m_EndTime < 0.0f)
			{
				return true;
			}
			return time < m_EndTime;
		}

		public override string ToString()
		{
			return "NodeSequence(" + m_FromID + "," + m_ToID + ")";
		}

		public int GetNodeID()
		{
			return m_FromID;
		}

		public int GetInstanceID()
		{
			return m_ToID;
		}

		public bool IsMaster()
		{
			return false;
		}

		public bool HasEndEvent()
		{
			return !IsAvailableOnEnd();
		}

		public float _EditorDisplayEndTime()
		{
			return m_EndTime;
		}

		public bool IsActive()
		{
			return true;
		}

		public Color _EditorGetColor()
		{
			return new Color(0.3f, 0.3f, 0.3f, 1.0f);
		}
	}
}
