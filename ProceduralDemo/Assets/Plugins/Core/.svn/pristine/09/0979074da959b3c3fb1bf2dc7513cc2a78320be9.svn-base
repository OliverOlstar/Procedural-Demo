
using UnityEngine;
using System.Collections.Generic;

namespace Act2
{
	public interface ITreeOwner
	{
		ITreeContext GetContext();
		string name { get; }
	}

	public class TreeRT
	{
		private ITreeOwner m_Owner = null;
		public ITreeOwner Owner => m_Owner;

		private ActTree2 m_SourceTree = null;
		public ActTree2 SourceTree => m_SourceTree;

		private ActTree2 m_Instance = null;
		public ActTree2 Instance => m_Instance;

		private string m_Name = Core.Str.EMPTY;
		public string Name => m_Name;

		private NodeRT m_Root = null;
		public NodeRT RootNode => m_Root;

		private double m_CreationTime = 0.0;
		private Dictionary<int, NodeRT> m_Nodes = new Dictionary<int, NodeRT>();
		private Dictionary<int, List<NodeRT>> m_Children = new Dictionary<int, List<NodeRT>>();
		private Dictionary<int, NodeRT> m_Parents = new Dictionary<int, NodeRT>();
		private Dictionary<int, List<TransitionRT>> m_Transitions = new Dictionary<int, List<TransitionRT>>();

		private Dictionary<string, int> m_NodesByName = new Dictionary<string, int>();

		public bool TryGetNode(int nodeID, out NodeRT node) => m_Nodes.TryGetValue(nodeID, out node);
		public bool HasNode(string nodeName) => m_NodesByName.ContainsKey(nodeName);
		public bool TryGetNodeIDFromName(string nodeName, out int nodeID)
		{
			if (m_NodesByName.TryGetValue(nodeName, out nodeID))
			{
				return true;
			}
			nodeID = Node.INVALID_ID;
			return false;
		}

		public NodeRT GetParent(int nodeID)
		{
			m_Parents.TryGetValue(nodeID, out NodeRT node);
			return node;
		}
		public bool TryGetChildren(int nodeID, out IReadOnlyList<NodeRT> nodes)
		{
			nodes = m_Children.TryGetValue(nodeID, out List<NodeRT> list) ? list : null;
			return nodes != null;
		}

		public bool TryGetTransitions(int nodeID, out IReadOnlyList<TransitionRT> transitions)
		{
			transitions = m_Transitions.TryGetValue(nodeID, out List<TransitionRT> list) ? list : null;
			return transitions != null;
		}

		public static TreeRT CreateAndDeferInitialize(ActTree2 tree, ITreeOwner owner)
		{
			TreeRT rt = new TreeRT(tree, owner);
			return rt;
		}
		public static TreeRT CreateAndInitialize(ActTree2 tree, ITreeOwner owner)
		{
			TreeRT rt = new TreeRT(tree, owner);
			rt.Initialize();
			return rt;
		}

		private TreeRT(ActTree2 tree, ITreeOwner owner)
		{
			m_Owner = owner ?? throw new System.ArgumentNullException("ActTreeRT() Owner cannot be null");
			m_SourceTree = tree ?? throw new System.ArgumentNullException("ActTreeRT() ActTree is null, a reference to an ActTree is probably unassigned somewhere");
			BuildTree();
		}

		private void BuildTree()
		{
			m_Instance = ActTree2.Instantiate(m_SourceTree);
			m_CreationTime = TreeDirtyTimestamps.Timestamp;
			m_Name = m_SourceTree.name;
			m_Root = new NodeRT(m_Instance.RootNode, this);
			m_Nodes.Add(m_Root.ID, m_Root);

			foreach (Node node in m_Instance.Nodes)
			{
				if (m_Nodes.ContainsKey(node.GetID()))
				{
					Debug.LogError(Core.Str.Build(m_Name, " ActTreeRT() Duplicate node ID ", node.GetID().ToString()));
					continue;
				}

				NodeRT nodeRT = new NodeRT(node, this);
				m_Nodes.Add(node.GetID(), nodeRT);
				if (!m_NodesByName.ContainsKey(node.Name))
				{
					m_NodesByName.Add(node.Name, node.GetID());
				}

				if (!m_Children.TryGetValue(node.GetParentID(), out List<NodeRT> children))
				{
					children = new List<NodeRT>();
					m_Children.Add(node.GetParentID(), children);
				}
				children.Add(nodeRT);
			}

			foreach (KeyValuePair<int, List<NodeRT>> pair in m_Children)
			{
				foreach (NodeRT node in pair.Value)
				{
					if (!m_Nodes.TryGetValue(pair.Key, out NodeRT parent))
					{
						Debug.LogWarning(Core.Str.Build(m_Name, " ActTreeRT() Parent ", pair.Key.ToString(), " for node ", node.ToString(), "doesn't exist"));
						continue;
					}
					m_Parents.Add(node.ID, parent);
				}
			}

			// Need to do this after creating nodes so they all exist
			// We store transitions at this level to support tree's refreshing while playing at runtime
			// Nodes should not hold references to other runtime nodes, otherwise stale sequencers are very difficult to purge
			foreach (NodeRT node in m_Nodes.Values)
			{
				Node sourceNode = node.SourceNode;
				if (sourceNode.Transitions.Count == 0)
				{
					continue;
				}
				List<TransitionRT> transitions = new List<TransitionRT>(sourceNode.Transitions.Count);
				foreach (NodeTransition trans in sourceNode.Transitions)
				{
					if (!m_Nodes.TryGetValue(trans.GetToID(), out NodeRT toNode))
					{
						Debug.LogWarning(Core.Str.Build(m_Name, " ActTreeRT() Transition to invalid node ", trans.GetToID().ToString()));
						continue;
					}
					transitions.Add(new TransitionRT(trans, node, toNode));
				}
				m_Transitions.Add(node.ID, transitions);
			}
		}

		public void Initialize()
		{
			ITreeContext context = m_Owner.GetContext();
			if (!m_SourceTree.GetContextType().IsAssignableFrom(context.GetType()))
			{
				Core.DebugUtil.DevException($"ActTreeRT.Initialize() Tree {m_SourceTree.name} cannot be initialized with context of type " +
					$"{context.GetType().Name} provided by owner {m_Owner.name}, it requires context of type {m_SourceTree.GetContextType().Name}");
			}
			foreach (NodeRT node in m_Nodes.Values)
			{
				node.Initialize(this, context);
			}
		}

#if UNITY_EDITOR
		public void _EditorRebuildIfDirty()
		{
			if (TreeDirtyTimestamps.IsDirty(m_SourceTree, m_CreationTime))
			{
				m_Instance = null;
				m_Nodes.Clear();
				m_Children.Clear();
				m_Parents.Clear();
				m_Transitions.Clear();
				BuildTree();
				Initialize();
			}
		}
#endif
	}
}
