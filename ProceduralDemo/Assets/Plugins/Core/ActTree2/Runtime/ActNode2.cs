
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;

namespace Act2
{
	public enum NodeType
	{
		Major = 0,
		Minor,
	}

	[System.Serializable]
	public abstract class NodeEventType
	{
		public static string SuffixToRemove => typeof(NodeEventType).Name;
		public abstract System.Type GetEventType();
		public string GetName()
		{
			string name = GetType().Name;
			name = name.Substring(0, name.Length - SuffixToRemove.Length);
			return name;
		}
	}

	public abstract class NodeEventTypeGeneric<TEvent> : NodeEventType where TEvent : ITreeEvent
	{
		public override System.Type GetEventType() => typeof(TEvent);
	}

	[MovedFrom(true, sourceAssembly:"Core")]
	[System.Serializable]
	public class Node : IActNode
	{
		public const int INVALID_ID = -1;
		public const int ROOT_ID = 0;

		public enum Available
		{
			Entry,
			TransitionsOnly,
			EventTransitionsOnly,
		}

		[SerializeField]
		private string m_Name = Core.Str.EMPTY;
		public string Name => m_Name;

		[SerializeField]
		private int m_ID = INVALID_ID;
		public int ID => m_ID;
		public int GetID() => m_ID;

		[SerializeField]
		private int m_ParentID = INVALID_ID;
		public int GetParentID() => m_ParentID;

		[SerializeField]
		private int m_PointerID = INVALID_ID;
		public int GetPointerID() => m_PointerID;

		[SerializeReference]
		private Available m_Availability = Available.Entry;
		public Available Availability => m_Availability;

		[SerializeReference]
		private List<Condition> m_Conditions = new List<Condition>();
		public List<Condition> Conditions => m_Conditions;

		[SerializeField]
		private List<NodeTransition> m_Transitions = new List<NodeTransition>();
		public List<NodeTransition> Transitions => m_Transitions;

		[SerializeReference]
		private List<Track> m_Tracks = new List<Track>();
		public List<Track> Tracks => m_Tracks;

		[SerializeReference]
		private NodeEventType m_EventType = null;
		public string EventTypeName => m_EventType == null ? "None" : m_EventType.GetName();

		[SerializeField]
		private NodeType m_NodeType = NodeType.Major;
		public NodeType NodeType => m_NodeType;

		[SerializeField]
		private Core.InspectorNotes m_Notes = new Core.InspectorNotes();

		[SerializeField]
		private Vector2 m_GraphPositionParent = Vector2.zero;
		public Vector2 _GraphPositionParent => m_GraphPositionParent;

		[SerializeField]
		private Vector2 m_GraphPositionChild = Vector2.zero;
		public Vector2 _GraphPositionChild => m_GraphPositionChild;

		public Node(int id, string name)
		{
			m_ID = id;
			m_Name = name;
		}

		public bool IsEventRequired(out System.Type eventType)
		{
			eventType = m_EventType != null ? m_EventType.GetEventType() : null;
			return eventType != null;
		}

		public IEnumerable<ITimedItem> GetAllTimedItems()
		{
			foreach (Track track in m_Tracks)
			{
				yield return track;
			}
			foreach (NodeTransition trans in m_Transitions)
			{
				yield return trans;
			}
		}

		public bool CanBeDefaultEntryNode(ActTree2 tree)
		{
			if (m_Conditions.Count > 0)
			{
				return false;
			}
			if (IsEventRequired(out System.Type eventType))
			{
				Node parent = tree.GetNode(m_ParentID);
				if (!parent.IsEventRequired(out System.Type parentEventType) || parentEventType != eventType)
				{
					return false;
				}
			}
			List<Node> children = tree.GetChildren(m_ID);
			if (children.Count == 0)
			{
				return true;
			}
			foreach (Node child in children)
			{
				if (child.CanBeDefaultEntryNode(tree))
				{
					return true;
				}
			}
			return false;
		}

		public struct Properties
		{
			public BaseNodeProperties BaseProperites;
			public string Name => BaseProperites.Name;
			public int ID => BaseProperites.ID;
			public bool HasTrack => BaseProperites.HasTrack;
			public bool HasMajorTrack => BaseProperites.HasMajorTrack;
			public bool HasConditions => BaseProperites.HasConditions;
			public bool HasFalseCondtion => BaseProperites.HasFalseCondtion;
			public bool IsEvent => BaseProperites.IsEvent;
			public System.Type RequiredEventType => BaseProperites.RequiredEventType;

			public string TreeName;
			public Node ReferencedNode;
			public bool IsRoot;
			public bool IsLeaf;
			public Available Availibility;
			public NodeType NodeType;
		}

		public static Properties GetProperties(ActTree2 tree, Node node)
		{
			BaseNodeProperties baseProperites = BaseNodeProperties.GetProperties(node);
			Node referencedNode = null;
			if (node.GetPointerID() != INVALID_ID)
			{
				referencedNode = tree.GetNode(node.GetPointerID());
			}
			bool isLeaf = referencedNode == null && !tree.HasChildren(node.GetID()); // Technically not a leaf as referenced node becomes a child
			Properties properties = new Properties
			{
				BaseProperites = baseProperites,
				Availibility = node.Availability,
				IsRoot = node.GetID() == ROOT_ID,
				IsLeaf = isLeaf,
				ReferencedNode = referencedNode,
				NodeType = node.NodeType,
				TreeName = tree.name,
			};
			return properties;
		}
		
		public static void GetEntryNodes(ActTree2 tree, IEnumerable<Node> siblings, out Dictionary<Node, int> entryNodeOrder, out int defaultEntryNodeID)
		{
			entryNodeOrder = new();
			defaultEntryNodeID = Node.INVALID_ID;
			int number = 1;
			foreach (Node node in siblings)
			{
				if (node.Availability != Node.Available.Entry)
				{
					continue;
				}
				bool isDefault = node.CanBeDefaultEntryNode(tree);
				if (isDefault)
				{
					defaultEntryNodeID = node.ID;
					entryNodeOrder.Add(node, -1);
					break;
				}
				entryNodeOrder.Add(node, number);
				number++;
			}
		}

		public override string ToString()
		{
			return "Node(" + m_Name + " : " + m_ID + ")";
		}
	}
}
