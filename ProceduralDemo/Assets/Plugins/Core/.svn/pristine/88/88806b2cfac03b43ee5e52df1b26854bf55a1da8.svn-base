
using UnityEngine;
using System.Collections.Generic;

namespace Act2
{
	[UberPicker.NoFoldoutInspector]
	[ScriptoGraph.Hide]
	[CreateAssetMenu(fileName = "New ActTree2", menuName = "Act Tree/Act Tree 2", order = 100)]
	public class ActTree2 : ScriptableObject, IActObject
	{
		[SerializeReference]
		private List<Node> m_Nodes = new List<Node>();
		public IReadOnlyList<Node> Nodes => m_Nodes;

		[SerializeField]
		private Node m_RootNode = new Node(Node.ROOT_ID, "Root");
		public Node RootNode => m_RootNode;

#pragma warning disable CS0414
		// This is used by editor code to generate unique IDs for nodes
		[SerializeField, Core.ReadOnly]
		private int m_NextNodeID = Node.ROOT_ID + 1;
#pragma warning restore

		public virtual System.Type GetContextType() => typeof(IGOContext);

		private void OnValidate()
		{
			// This is called every time changes to the tree are made no mater at what level those changes have been applied.
			// Seems like the best place to report the tree is dirty so it will be reloaded at runtime
			TreeDirtyTimestamps.SetDirty(this);
		}

		public Node GetNode(int id)
		{
			if (id == Node.INVALID_ID)
			{
				return null;
			}
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
		public bool TryGetNode(int id, out Node node)
		{
			node = GetNode(id);
			return node != null;
		}
		bool IActObject.TryGetNode(int id, out IActNode node)
		{
			node = GetNode(id);
			return node != null;
		}

		public int GetNodeIndex(int id)
		{
			for (int i = 0; i < m_Nodes.Count; i++)
			{
				if (m_Nodes[i].GetID() == id)
				{
					return i;
				}
			}
			return -1;
		}

		public int GetParentID(int nodeID)
		{
			Node node = GetNode(nodeID);
			return node != null ? node.GetParentID() : Node.INVALID_ID;
		}

		public List<Node> GetSiblings(int nodeID) => GetChildren(GetParentID(nodeID));
		public List<Node> GetSiblings(Node node) => GetChildren(node.GetParentID());
		public List<Node> GetChildren(int nodeID)
		{
			List<Node> children = new List<Node>();
			foreach (Node node in m_Nodes)
			{
				if (node.GetParentID() == nodeID)
				{
					children.Add(node);
				}
			}
			return children;
		}
		public bool HasChildren(int nodeID)
		{
			foreach (Node node in m_Nodes)
			{
				if (node.GetParentID() == nodeID)
				{
					return true;
				}
			}
			return false;
		}
		public int GetChildCount(int nodeID)
		{
			int count = 0;
			foreach (Node node in m_Nodes)
			{
				if (node.GetParentID() == nodeID)
				{
					count++;
				}
			}
			return count;
		}
	}
}
