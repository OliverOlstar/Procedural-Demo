
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace Act2
{
	public class ActTreeEditorWindow2 : EditorWindow
	{
		private const string SELECTED_TREE_PREFS_KEY = "ActTreePath2";

		[UnityEditor.Callbacks.OnOpenAsset(1)]
		public static bool OnOpenAsset(int instanceID, int line)
		{
			if (Selection.activeObject is ActTree2 actTree)
			{
				Get().SetActTree(actTree);

				return true; //catch open file
			}

			return false; // let unity open the file
		}

		[MenuItem("Window/Act Tree 2/Tree Editor")]
		public static ActTreeEditorWindow2 Get()
		{
			ActTreeEditorWindow2 window = EditorWindow.GetWindow<ActTreeEditorWindow2>();
			window.titleContent = new GUIContent("Act Tree 2", EditorUtil.ActTreeIcon);
			window.Show();
			return window;
		}
		public static void OpenTree(ActTree2 tree, int nodeID)
		{
			ActTreeEditorWindow2 treeWindow = Get();
			treeWindow.SetActTree(tree);
			treeWindow.SetSelectedNode(nodeID);
			treeWindow.Focus();
		}
		public static void OpenTree(string treePath, int nodeID)
		{
			ActTree2 tree = AssetDatabase.LoadAssetAtPath<ActTree2>(treePath);
			if (tree != null)
			{
				OpenTree(tree, nodeID);
			}
		}

		private enum View
		{
			Tree,
			Graph
		}

		[SerializeField, UberPicker.AssetNonNull]
		private ActTree2 m_SelectedTree = null;

		private View m_View = View.Tree;

		private TreeRecentDrawer m_RecentTreesDrawer = new TreeRecentDrawer();
		private ActTree2 m_Tree = null;
		public ActTree2 Tree => m_Tree;
		private SerializedObject m_STree = null;

		private int m_SelectedNodeID = Node.INVALID_ID;
		public int SelectedNodeID => m_SelectedNodeID;

		private int m_RenameNodeID = Node.INVALID_ID;
		public int RenameNodeID => m_RenameNodeID;

		private string m_CurrentTreePath = "";
		private Dictionary<string, int> m_NodeHistory;

		private TreeDrawer m_TreeDrawer = new TreeDrawer();
		private GraphDrawer m_GraphDrawer = new GraphDrawer();

		private SerializedObject m_SerializedWindow = null;

		private void OnEnable()
		{
			Undo.undoRedoPerformed += OnUndo;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			m_NodeHistory = new Dictionary<string, int>();
			m_RecentTreesDrawer.InitHistory();
		}

		private void OnDisable()
		{
			Undo.undoRedoPerformed -= OnUndo;
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		}

		private void OnUndo()
		{
			if (m_Tree == null)
			{
				m_STree = null;
				return;
			}
			m_Tree = AssetDatabase.LoadAssetAtPath<ActTree2>(AssetDatabase.GetAssetPath(m_Tree)); // Undo could have made changes to tree
			m_STree = new SerializedObject(m_Tree);
			if (m_Tree.GetNode(m_SelectedNodeID) == null)
			{
				m_SelectedNodeID = Node.INVALID_ID;
			}
			Repaint();
		}

		public void SetActTree(ActTree2 actTree)
		{
			m_Tree = actTree;
			m_SelectedTree = m_Tree;
			m_STree = new SerializedObject(m_Tree);
			m_CurrentTreePath = AssetDatabase.GetAssetPath(m_Tree);
			EditorPrefs.SetString(SELECTED_TREE_PREFS_KEY, m_CurrentTreePath);
			m_RecentTreesDrawer.AddToHistory(m_Tree);
			if (m_NodeHistory.TryGetValue(m_CurrentTreePath, out int selectedNodeID))
			{
				SetSelectedNode(selectedNodeID);
			}
			else
			{
				ClearSelection();
			}
		}

		private void OnSelectionChange()
		{
			Repaint(); // This is to update debugging state in TreeDrawer
		}
		private void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			Repaint(); // This is to update debugging state in TreeDrawer
		}
		private void OnInspectorUpdate()
		{
			if (m_TreeDrawer.IsDebugging || m_GraphDrawer.RequestRepaint())
			{
				Repaint();
			}
		}

		private void OnGUI()
		{
			// Note: I tried moving this code to OnEnable but it keeps opening corrupted ActNodeEditorWindows, I guess it's fine here?
			// Also Note: Mostly this code is to re-load trees when window re-initializes after recompiling and it's interesting...
			// when code re-compiles m_Tree will still hold onto it's reference, however m_STree will be null
			if ((m_Tree == null || m_STree == null) && EditorPrefs.HasKey(SELECTED_TREE_PREFS_KEY))
			{
				ActTree2 cachedTree = AssetDatabase.LoadMainAssetAtPath(EditorPrefs.GetString(SELECTED_TREE_PREFS_KEY)) as ActTree2;
				if (cachedTree != null)
				{
					SetActTree(cachedTree);
				}
			}

			if (m_SerializedWindow == null)
			{
				m_SerializedWindow = new SerializedObject(this);
			}
			else
			{
				m_SerializedWindow.Update();
			}
			EditorGUILayout.PropertyField(m_SerializedWindow.FindProperty(nameof(m_SelectedTree)), GUIContent.none);
			if (m_SelectedTree != m_Tree)
			{
				SetActTree(m_SelectedTree);
			}

			if (m_RecentTreesDrawer.HistoryGUI(m_Tree, out ActTree2 selectedTree))
			{
				SetActTree(selectedTree);
			}

			if (m_Tree == null || m_STree == null)
			{
				return;
			}

			m_View = (View)EditorGUILayout.EnumPopup(m_View);

			//EditorGUILayout.LabelField(m_Tree.name);
			switch (m_View)
			{
				case View.Tree:
					m_TreeDrawer.OnGUI(this);
					break;
				case View.Graph:
					m_GraphDrawer.OnGUI(m_Tree, m_STree, this, position, m_SelectedNodeID);
					break;
			}

			CheckInput();
		}

		private void ApplyChanges()
		{
			m_STree.ApplyModifiedProperties();
		}

		private void DeleteNode(Node nodeToDelete)
		{
			// Delete siblings transitions that point to deleted node
			SerializedProperty sNodes = m_STree.FindProperty("m_Nodes");
			IReadOnlyList<Node> allNodes = m_Tree.Nodes;
			int deleteIndex = -1;
			for (int i = 0; i < allNodes.Count; i++)
			{
				Node node = allNodes[i];
				if (deleteIndex < 0 && node.GetID() == nodeToDelete.GetID())
				{
					deleteIndex = i;
				}
				if (node.GetParentID() != nodeToDelete.GetParentID()) // Check if sibling
				{
					continue;
				}
				SerializedProperty sSiblingNode = sNodes.GetArrayElementAtIndex(i);
				SerializedProperty sTransitions = sSiblingNode.FindPropertyRelative("m_Transitions");
				for (int k = node.Transitions.Count - 1; k >= 0; k--)
				{
					if (node.Transitions[k].GetToID() == nodeToDelete.GetID())
					{
						sTransitions.DeleteArrayElementAtIndex(k);
					}
				}
			}

			// Delete node, need to do this last so we don't change indexes of nodes
			if (deleteIndex >= 0)
			{
				sNodes.DeleteArrayElementAtIndex(deleteIndex);
			}
			List<Node> childNodeIDs = m_Tree.GetChildren(nodeToDelete.GetID()); // Get children before applying deletion
			ApplyChanges(); // Need to apply the changes before we recurse

			// Need to clear node if node window is displaying it
			if (nodeToDelete.GetID() == m_SelectedNodeID)
			{
				m_SelectedNodeID = Node.INVALID_ID;
			}
			ActNodeEditorWindow2 nodeEditorWindow = ActNodeEditorWindow2.Get();
			if (nodeEditorWindow.GetNode() != null && nodeEditorWindow.GetNode().GetID() == nodeToDelete.GetID())
			{
				nodeEditorWindow.ClearNode();
			}
			// Need to clear the nodes editor pref key
			m_TreeDrawer.OnDeleted(m_Tree, nodeToDelete);

			foreach (Node child in childNodeIDs)
			{
				DeleteNode(child);
			}
		}

		private void AddChild(Node parentNode, Vector2? graphPosition = null)
		{
			SerializedProperty serNodes = m_STree.FindProperty("m_Nodes");
			SerializedProperty serNodeID = m_STree.FindProperty("m_NextNodeID");
			int newID = serNodeID.intValue;
			serNodeID.intValue++;
			serNodes.arraySize++;
			SerializedProperty newNode = serNodes.GetArrayElementAtIndex(serNodes.arraySize - 1);
			string name = "Node01";
			NodeClipboard.ValidateName(m_Tree, parentNode.GetID(), Node.INVALID_ID, name, out name);
			newNode.managedReferenceValue = new Node(newID, name);
			newNode.FindPropertyRelative("m_ParentID").intValue = parentNode.GetID();
			if (graphPosition.HasValue)
			{
				newNode.FindPropertyRelative("m_GraphPositionChild").vector2Value = graphPosition.Value;
			}
			ApplyChanges();
		}

		public void StartRenamingNode(int renameNodeID) => m_RenameNodeID = renameNodeID;
		public void FinishRenamingNode()
		{
			if (m_RenameNodeID == Node.INVALID_ID)
			{
				return;
			}
			// When finishing renaming validate that a good name was input
			if (m_Tree.TryGetNode(m_RenameNodeID, out Node node) &&
				!NodeClipboard.ValidateName(m_Tree, node.GetParentID(), node.ID, node.Name, out string validName))
			{
				RenameNode(node, validName);
			}
			m_RenameNodeID = Node.INVALID_ID;
		}
		public void RenameNode(Node node, string newName)
		{
			if (node.Name == newName)
			{
				return;
			}
			int nodeIndex = m_Tree.GetNodeIndex(node.GetID());
			if (nodeIndex < 0)
			{
				return;
			}
			SerializedProperty sNodes = m_STree.FindProperty("m_Nodes");
			SerializedProperty sNode = sNodes.GetArrayElementAtIndex(nodeIndex);
			SerializedProperty sName = sNode.FindPropertyRelative("m_Name");
			sName.stringValue = newName;
			ApplyChanges();
		}
		public void RenameNode(int nodeID, string newName)
		{
			if (m_Tree.TryGetNode(nodeID, out Node node))
			{
				RenameNode(node, newName);
			}
		}

		public void AddChildContext(object obj)
		{
			var context = ((Node node, Vector2 pos))obj;
			Vector2? graphPosition = m_View == View.Graph ? context.pos - m_GraphDrawer.Graph.Offset : null;
			AddChild(context.node, graphPosition);
		}

		public void DeleteNodeContext(object obj)
		{
			Node node = (Node)obj;
			DeleteNode(node);
		}

		public void MoveUpContext(object obj)
		{
			Node node = (Node)obj;
			MoveUp(node);
		}

		public void MoveDownContext(object obj)
		{
			Node node = (Node)obj;
			MoveDown(node);
		}

		public void DuplicateContext(object obj)
		{
			var context = ((Node node, Vector2 pos))obj;
			Vector2? graphPosition = m_View == View.Graph ? context.pos - m_GraphDrawer.Graph.Offset : null;
			NodeClipboard.Copy(context.node, m_Tree);
			Node parent = m_Tree.GetNode(m_Tree.GetParentID(context.node.GetID()));
			NodeClipboard.Paste(parent, m_Tree, ref m_STree, graphPosition);
			NodeClipboard.Clear();
		}

		public void CopyContext(object obj)
		{
			Node node = (Node)obj;
			NodeClipboard.Copy(node, m_Tree);
		}

		public void CopyClearContext()
		{
			NodeClipboard.Clear();
		}

		public void PasteContext(object obj)
		{
			var context = ((Node node, Vector2 pos))obj;
			Vector2? graphPosition = m_View == View.Graph ? context.pos - m_GraphDrawer.Graph.Offset : null;
			NodeClipboard.Paste(context.node, m_Tree, ref m_STree, graphPosition);
		}

		private void CleanupAll()
		{
			AssetDatabase.SaveAssets();
			string[] guids = AssetDatabase.FindAssets("t:ActTree2");
			for (int i = 0; i < guids.Length; i++)
			{
				EditorUtility.DisplayProgressBar("Cleanup All", (i + 1) + "/" + guids.Length, (float)i / guids.Length);
				string treePath = AssetDatabase.GUIDToAssetPath(guids[i]);
				Cleanup(treePath);
			}
			AssetDatabase.SaveAssets();
			EditorUtility.ClearProgressBar();
		}

		private void CleanupThis()
		{
			string treePath = AssetDatabase.GetAssetPath(m_Tree);
			AssetDatabase.SaveAssets();
			Cleanup(treePath);
			AssetDatabase.SaveAssets();
			m_Tree = AssetDatabase.LoadAssetAtPath<ActTree2>(treePath);
			m_STree = new SerializedObject(m_Tree);
			Repaint();
		}

		private static void Cleanup(string treePath)
		{
			// Doing all the things I can think of to make sure tree is in sync
			AssetDatabase.ImportAsset(treePath);
			ActTree2 tree = AssetDatabase.LoadAssetAtPath<ActTree2>(treePath);
			if (tree == null)
			{
				Debug.LogError($"ActTreeEditorWindow.Cleanup({treePath}) Is not an ActTree");
				return;
			}
			SerializedObject sTree = new SerializedObject(tree);
			CleanupRecursive(tree, sTree, 0);
		}

		private static void CleanupRecursive(ActTree2 tree, SerializedObject sTree, int recursionCount)
		{
			if (recursionCount > 50)
			{
				Debug.LogError($"ActTreeEditorWindow.Cleanup({tree.name}) Recursed over 50 times something went wrong");
				return;
			}

			int nodeOrphansKilled = 0;
			IReadOnlyList<Node> nodes = tree.Nodes;
			SerializedProperty serNodes = sTree.FindProperty("m_Nodes");
			if (nodes.Count != serNodes.arraySize)
			{
				Debug.LogError($"ActTreeEditorWindow.Cleanup({tree.name}) Nodes are out of sync");
				return;
			}
			for (int i = nodes.Count - 1; i >= 0;  i--)
			{
				Node node = nodes[i];
				if (node.GetParentID() == Node.ROOT_ID)
				{
					continue;
				}
				bool isOrphan = true;
				foreach (Node parent in tree.Nodes)
				{
					if (parent.GetID() == node.GetParentID())
					{
						isOrphan = false;
						break;
					}
				}
				if (isOrphan) // if we dont have a parent we should die
				{
					serNodes.DeleteArrayElementAtIndex(i);
					nodeOrphansKilled++;
				}
			}
			if (nodeOrphansKilled > 0)
			{
				sTree.ApplyModifiedProperties();
			}
			if (nodes.Count != serNodes.arraySize)
			{
				Debug.LogError($"ActTreeEditorWindow.Cleanup({tree.name}) Nodes are out of sync");
				return;
			}
			int transitionOrphansKilled = 0;
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				Node node = nodes[i];
				SerializedProperty sernNode = serNodes.GetArrayElementAtIndex(i);
				SerializedProperty serTransitions = sernNode.FindPropertyRelative("m_Transitions");
				if (node.Transitions.Count != serTransitions.arraySize)
				{
					Debug.LogError($"ActTreeEditorWindow.Cleanup({tree.name}) Node {node} transitions are out of sync");
					return;
				}
				for (int j = node.Transitions.Count - 1; j >= 0; j--)
				{
					NodeTransition trans = node.Transitions[j];
					Node toNode = tree.GetNode(trans.GetToID());
					if (toNode == null ||
						toNode.GetParentID() != node.GetParentID()) // Transitions should only point to sibilings
					{
						serTransitions.DeleteArrayElementAtIndex(j);
						transitionOrphansKilled++;
					}
				}
			}
			if (transitionOrphansKilled > 0)
			{
				sTree.ApplyModifiedProperties();
			}

			Core.Str.AddLine($"ActTreeEditorWindow.Cleanup({tree.name}) Recursion {recursionCount}");
			if (nodeOrphansKilled > 0) Core.Str.AddLine($"Killed {nodeOrphansKilled} node orphans");
			if (transitionOrphansKilled > 0) Core.Str.AddLine($"Killed {transitionOrphansKilled} transition orphans");
			if (nodeOrphansKilled > 0 || transitionOrphansKilled > 0)
			{
				CleanupRecursive(tree, sTree, ++recursionCount);
			}
			else
			{
				Core.Str.AddLine($"Clean up complete");
				Debug.Log(Core.Str.Finish());
			}
		}

		public void OpenContext(int nodeID)
		{
			Node node = m_Tree.GetNode(nodeID);
			GenericMenu menu = new GenericMenu();
			bool isRoot = nodeID == Node.ROOT_ID;

			menu.AddItem(new GUIContent("Add Child    (ctrl+n)"), false, AddChildContext, (node, Event.current.mousePosition));
			if (!isRoot)
			{
				menu.AddItem(new GUIContent("Delete    (delete)"), false, DeleteNodeContext, node);
				menu.AddSeparator("");
				menu.AddItem(new GUIContent("Move Up    (ctrl+up)"), false, MoveUpContext, node);
				menu.AddItem(new GUIContent("Move Down    (ctrl+down)"), false, MoveDownContext, node);
				menu.AddSeparator("");
				menu.AddItem(new GUIContent("Duplicate    (ctrl+d)"), false, DuplicateContext, (node, Event.current.mousePosition));
			}
			if (NodeClipboard.IsCopied(node, m_Tree))
			{
				menu.AddItem(new GUIContent("Clear Copied    (esc)"), false, CopyClearContext);
			}
			else
			{
				menu.AddItem(new GUIContent("Copy    (ctrl+c)"), false, CopyContext, node);
			}
			if (NodeClipboard.CanPaste())
			{
				menu.AddItem(new GUIContent("Paste Child    (ctrl+v)"), false, PasteContext, (node, Event.current.mousePosition));
			}
			else
			{
				menu.AddDisabledItem(new GUIContent("Paste Child    (ctrl+v)"));
			}
			if (isRoot)
			{
				menu.AddItem(new GUIContent("Cleanup"), false, CleanupThis);
				menu.AddItem(new GUIContent("Cleanup All"), false, CleanupAll);
			}
			menu.ShowAsContext();
		}

		public void MoveUp(Node node)
		{
			int parentID = node.GetParentID();
			int swapIndex = -1; // We are going to look for the closest sibling above the node to swap with
			IReadOnlyList<Node> nodes = m_Tree.Nodes;
			for (int i = 0; i < nodes.Count; i++)
			{
				Node sib = nodes[i];
				if (sib.GetParentID() != parentID) // Check if this link is a sibiling
				{
					continue;
				}
				if (sib.GetID() == node.GetID()) // Found node id in the list, see if we have a valid index to swap with
				{
					if (swapIndex >= 0)
					{
						m_STree.FindProperty("m_Nodes").MoveArrayElement(i, swapIndex);
						ApplyChanges();
					}
					break;
				}
				swapIndex = i; // This index is a swap candidate
			}
		}

		public void MoveDown(Node node)
		{
			int parentID = node.GetParentID();
			int swapIndex = -1; // We are going to look for first node after this one to swap with
			IReadOnlyList<Node> nodes = m_Tree.Nodes;
			for (int i = 0; i < nodes.Count; i++)
			{
				Node sib = nodes[i];
				if (sib.GetParentID() != parentID) // Check if this link is a sibiling
				{
					continue;
				}
				if (swapIndex >= 0) // If swap index is set then swap the next time we find a sibiling
				{
					m_STree.FindProperty("m_Nodes").MoveArrayElement(i, swapIndex);
					ApplyChanges();
					break;
				}
				if (sib.GetID() == node.GetID()) // Found node id in the list, see if we have a valid index to swap with
				{
					swapIndex = i;
				}
			}
		}

		public void SetSelectedNode(Node node) => SetSelectedNode(node.GetID());
		public void SetSelectedNode(int nodeID)
		{
			int selectedIndex = m_Tree.GetNodeIndex(nodeID);
			if (selectedIndex < 0)
			{
				ClearSelection();
				return;
			}

			FinishRenamingNode();

			Node node = m_Tree.Nodes[selectedIndex];
			TreeDrawer.UncollapseParents(m_Tree, node); // Uncollapse parent nodes, it's weird to select a node that's hidden because it's parent is collapsed

			m_SelectedNodeID = nodeID;
			m_NodeHistory[m_CurrentTreePath] = nodeID;
			SerializedProperty serNodeList = m_STree.FindProperty("m_Nodes");
			ActNodeEditorWindow2.Get().SetNode(m_Tree, m_STree, m_Tree.Nodes[selectedIndex], serNodeList.GetArrayElementAtIndex(selectedIndex));
			Focus();
		}

		public void ClearSelection()
		{
			FinishRenamingNode();
			m_NodeHistory[m_CurrentTreePath] = Node.INVALID_ID;
			m_SelectedNodeID = Node.INVALID_ID;
			ActNodeEditorWindow2.Get().ClearNode();
			Focus();
		}

		public void CheckInput()
		{
			Event inputEvent = Event.current; 
			if (inputEvent.type != EventType.KeyDown)
			{
				return;
			}
			switch (inputEvent.keyCode)
			{
				case KeyCode.None: // For some reason when you press enter while renaming a node it regesters as "None"
					FinishRenamingNode();
					break;
			}
			if (inputEvent.control)
			{
				switch (inputEvent.keyCode)
				{
					case KeyCode.RightArrow:
						if (m_RecentTreesDrawer.NextHistory(out ActTree2 nextTree))
						{
							SetActTree(nextTree);
							Repaint();
						}
						break;
					case KeyCode.LeftArrow:
						if (m_RecentTreesDrawer.PrevHistory(out ActTree2 prevTree))
						{
							SetActTree(prevTree);
							Repaint();
						}
						break;
				}
			}
			
			Node addToNode = m_View == View.Graph ? 
				m_Tree.GetNode(m_GraphDrawer.ParentID) :
				m_Tree.GetNode(m_SelectedNodeID);
			if (addToNode != null && inputEvent.control)
			{
				switch (inputEvent.keyCode)
				{
					case KeyCode.N:
						AddChildContext((addToNode, inputEvent.mousePosition));
						Repaint();
						inputEvent.Use(); // Important because Unity uses this key binding
						break;
					case KeyCode.V:
						if (NodeClipboard.CanPaste())
						{
							PasteContext((addToNode, inputEvent.mousePosition));
							Repaint();	
						}
						break;
				}
			}
			
			Node selectedNode = m_Tree.GetNode(m_SelectedNodeID);
			if (selectedNode == null)
			{
				return;
			}
			switch (inputEvent.keyCode)
			{
				case KeyCode.Delete:
					DeleteNode(selectedNode);
					Repaint();
					break;
				case KeyCode.Escape:
					if (NodeClipboard.IsCopied(m_Tree))
					{
						NodeClipboard.Clear();
						Repaint();
					}

					break;
			}
			if (inputEvent.control)
			{
				switch (inputEvent.keyCode)
				{
					case KeyCode.UpArrow:
						MoveUp(selectedNode);
						Repaint();
						break;
					case KeyCode.DownArrow:
						MoveDown(selectedNode);
						Repaint();
						break;
					case KeyCode.D:
						DuplicateContext((selectedNode, inputEvent.mousePosition));
						Repaint();
						break;
					case KeyCode.C:
						if (NodeClipboard.IsCopied(selectedNode, m_Tree))
						{
							NodeClipboard.Clear();
						}
						else
						{
							NodeClipboard.Copy(selectedNode, m_Tree);
						}
						Repaint();
						break;
				}
			}
		}
	}
}
