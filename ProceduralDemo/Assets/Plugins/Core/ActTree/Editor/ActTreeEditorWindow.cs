
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

public class ActTreeEditorWindow : EditorWindow
{
	[UnityEditor.Callbacks.OnOpenAsset(1)]
	public static bool OnOpenAsset(int instanceID, int line)
	{
		if (Selection.activeObject is ActTree actTree)
		{
			Get().SetActTree(AssetDatabase.GetAssetPath(actTree));
			return true; //catch open file
		}
		return false; // let unity open the file
	}

	[MenuItem("Window/Act Tree/Tree Editor")]
	public static ActTreeEditorWindow Get()
	{
		ActTreeEditorWindow window = EditorWindow.GetWindow<ActTreeEditorWindow>("Act Tree");
		window.Show();
		return window;
	}

	private ActNodeDrawer m_ActNodeDrawer = new ActNodeDrawer();
	private ActTrackDrawer m_TrackDrawer = new ActTrackDrawer();
	private ActTreeRecentDrawer m_RecentTreesDrawer = new ActTreeRecentDrawer();
	private Vector2 m_ScrollPos = Vector2.zero;
	private ActTree m_Tree = null;
	private SerializedObject m_STree = null;
	private int m_SelectedNodeID = Act.Node.INVALID_ID;
	private Core.EditorPrefsString m_CurrentTreePath;
	private Dictionary<string, int> m_NodeHistory;

	private static readonly StringSearchWindowContext SEARCH_WINDOW_CONTEXT =
		new StringSearchWindowContext(nameof(ActTreeEditorWindow), nameof(ActTreeEditorWindow), GetActTreePaths, new char[] { '/' });

	void OnEnable()
	{
		Undo.undoRedoPerformed += OnUndo;
		m_NodeHistory = new Dictionary<string, int>();
		m_RecentTreesDrawer.InitHistory();

		m_CurrentTreePath = new Core.EditorPrefsString("ActTreePath");
		if ((m_Tree == null || m_STree == null) && m_CurrentTreePath.HasValue)
		{
			SetActTree(m_CurrentTreePath);
		}
	}

	void OnDisable()
	{
		Undo.undoRedoPerformed -= OnUndo;
	}

	void OnUndo()
	{
		if (m_Tree == null || m_STree == null)
		{
			return;
		}

		AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_Tree)); // Undo could have made changes to tree
		ActTrackDrawer.UpdateTrackList(m_Tree, ref m_STree);
		ActConditionDrawer.RebuildConditionsList(m_Tree, ref m_STree);
		ActTreeDirtyTimestamps.SetDirty(m_Tree);
		Repaint();
	}

	void SetActTree(string treePath)
	{
		Object obj = AssetDatabase.LoadMainAssetAtPath(treePath);
		if (obj is ActTree actTree)
		{
			m_CurrentTreePath.Value = treePath;
			m_Tree = actTree;
			m_STree = new SerializedObject(obj);

			if (m_NodeHistory.ContainsKey(treePath))
			{
				SetSelectedNode(m_NodeHistory[treePath]);
			}
			else
			{
				m_NodeHistory.Add(treePath, Act.Node.INVALID_ID);
				m_SelectedNodeID = Act.Node.INVALID_ID;
				ActNodeEditorWindow.Get().ClearNode();
				Focus();
			}

			m_RecentTreesDrawer.AddToHistory(actTree);
		}
	}

	public static void SelectActTree(System.Action<string> onTreeSelected)
	{
		if (GUILayout.Button("Select..."))
		{
			StringSearchWindowProvider.Show(SEARCH_WINDOW_CONTEXT, Get().SetActTree);
		}
	}

	private static List<string> GetActTreePaths()
	{
		string[] treeGuids = AssetDatabase.FindAssets("t:ActTree");
		if (treeGuids.Length == 0)
		{
			return null;
		}
		List<string> treePaths = new List<string>(treeGuids.Length);
		for (int i = 0; i < treeGuids.Length; i++)
		{
			treePaths.Add(AssetDatabase.GUIDToAssetPath(treeGuids[i]));
		}
		return treePaths;
	}

	void OnGUI()
	{
		SelectActTree(SetActTree);
		if (m_RecentTreesDrawer.HistoryGUI(m_Tree, out string recentPath))
		{
			SetActTree(recentPath);
		}

		if (m_Tree == null || m_STree == null)
		{
			return;
		}

		m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

		//EditorGUILayout.LabelField(m_Tree.name);
		NodeGUI(m_Tree.GetRootNode());

		EditorGUILayout.EndScrollView();

		CheckInput();
	}

	string GetNodeCollapsedPrefKey(Act.Node node)
	{
		return $"NodeCollapsed:{m_CurrentTreePath}_{node.GetID()}";
	}

	bool IsNodeCollapsed(Act.Node node)
	{
		return EditorPrefs.GetBool(GetNodeCollapsedPrefKey(node), false);
	}

	void SetNodeCollapsed(Act.Node node, bool collapsed, bool recurse)
	{
		if (collapsed)
		{
			EditorPrefs.SetBool(GetNodeCollapsedPrefKey(node), collapsed);
		}
		else
		{
			EditorPrefs.DeleteKey(GetNodeCollapsedPrefKey(node));
		}
		if (recurse)
		{
			foreach (Act.NodeLink link in m_Tree.GetAllNodeLinks())
			{
				if (link.GetParentID() != node.GetID())
				{
					continue;
				}
				Act.Node child = m_Tree.GetNode(link.GetChildID());
				if (child != null)
				{
					SetNodeCollapsed(child, collapsed, recurse);
				}
			}
		}
	}

	void NodeGUI(Act.Node node, bool isReferencedChild = false)
	{
		m_ActNodeDrawer.Initialize();

		bool isCollapsed = m_ActNodeDrawer.OnGUI(
			m_Tree,
			node,
			IsNodeCollapsed(node),
			m_SelectedNodeID,
			isReferencedChild,
			out Act.Node referencedNode,
			out bool foldoutToggled,
			out Rect controlRect);
		if (foldoutToggled)
		{
			SetNodeCollapsed(node, isCollapsed, Event.current.alt);
		}
		if (!isReferencedChild && GUI.Button(controlRect, string.Empty, GUIStyle.none))
		{
			if (Event.current.button == 0)
			{
				SetSelectedNode(node.GetID());
			}
			else
			{
				OpenContext(node);
			}
		}
		if (isCollapsed)
		{
			return;
		}

		EditorGUI.indentLevel++;

		if (referencedNode != null)
		{
			foreach (Act.NodeLink link in m_Tree.GetAllNodeLinks())
			{
				if (link.GetParentID() != referencedNode.GetID())
				{
					continue;
				}
				Act.Node child = m_Tree.GetNode(link.GetChildID());
				if (child != null)
				{
					NodeGUI(child, true);
				}
			}
		}

		foreach (Act.NodeLink link in m_Tree.GetAllNodeLinks())
		{
			if (link.GetParentID() != node.GetID())
			{
				continue;
			}
			Act.Node child = m_Tree.GetNode(link.GetChildID());
			if (child != null)
			{
				NodeGUI(child, isReferencedChild);
			}
		}
		EditorGUI.indentLevel--;
	}

	void DeleteNode(Act.Node node)
	{
		if (node.GetID() == m_SelectedNodeID)
		{
			m_SelectedNodeID = Act.Node.INVALID_ID;
		}
		Undo.RecordObject(m_Tree, "DeleteNode");
		DeleteNodeRecursive(node);
		EditorUtility.SetDirty(m_Tree);
		AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_Tree));
		ActTrackDrawer.UpdateTrackList(m_Tree, ref m_STree);
		ActConditionDrawer.RebuildConditionsList(m_Tree, ref m_STree);
		Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
	}
	void DeleteNodeRecursive(Act.Node node)
	{
		SerializedProperty serNodes = m_STree.FindProperty("m_Nodes");
		int nodeIndex = 0;
		foreach (Act.Node n in m_Tree.GetAllNodes())
		{
			if (n.GetID() == node.GetID())
			{
				serNodes.DeleteArrayElementAtIndex(nodeIndex);
			}
			else
			{
				nodeIndex++;
			}
		}
		SerializedProperty serLinks = m_STree.FindProperty("m_NodeLinks");
		int linkIndex = 0;
		List<Act.NodeLink> childLinks = new List<Act.NodeLink>();
		foreach (Act.NodeLink link in m_Tree.GetAllNodeLinks())
		{
			if (link.GetChildID() == node.GetID())
			{

				serLinks.DeleteArrayElementAtIndex(linkIndex);
			}
			else
			{
				linkIndex++;
				if (link.GetParentID() == node.GetID())
				{
					childLinks.Add(link);
				}
			}
		}

		SerializedProperty serOpps = m_STree.FindProperty("m_Opportunities");
		int oppIndex = 0;
		foreach (Act.NodeSequence opp in m_Tree.GetAllOpportunities())
		{
			if (opp.GetFromID() == node.GetID() || opp.GetToID() == node.GetID())
			{
				serOpps.DeleteArrayElementAtIndex(oppIndex);
			}
			else
			{
				oppIndex++;
			}
		}

		foreach (ActTrack track in m_Tree.GetTracks())
		{
			if (track != null && track.GetNodeID() == node.GetID())
			{
				Undo.DestroyObjectImmediate(track);
			}
		}
		foreach (ActCondition condition in m_Tree.GetConditions())
		{
			if (condition != null && condition.GetNodeID() == node.GetID())
			{
				Undo.DestroyObjectImmediate(condition);
			}
		}

		m_STree.ApplyModifiedProperties(); // Need to apply the changes before we recurse
		ActTreeDirtyTimestamps.SetDirty(m_Tree);
		foreach (Act.NodeLink link in childLinks)
		{
			DeleteNode(m_Tree.GetNode(link.GetChildID()));
		}
		// Need to clear node if node window is displaying it
		ActNodeEditorWindow nodeEditorWindow = ActNodeEditorWindow.Get();
		if (nodeEditorWindow.GetNode() != null && nodeEditorWindow.GetNode().GetID() == node.GetID())
		{
			nodeEditorWindow.ClearNode();
		}
		// Need to clear the nodes editor pref key
		EditorPrefs.DeleteKey(GetNodeCollapsedPrefKey(node));
	}

	void AddChild(Act.Node node)
	{
		SerializedProperty serNodes = m_STree.FindProperty("m_Nodes");
		SerializedProperty serLinks = m_STree.FindProperty("m_NodeLinks");
		SerializedProperty serNodeID = m_STree.FindProperty("m_NextNodeID");
		int newID = serNodeID.intValue;
		serNodeID.intValue++;
		serNodes.arraySize++;
		SerializedProperty newNode = serNodes.GetArrayElementAtIndex(serNodes.arraySize - 1);
		newNode.FindPropertyRelative("m_ID").intValue = newID;
		newNode.FindPropertyRelative("m_Name").stringValue = "New Node";
		newNode.FindPropertyRelative("m_PointerID").intValue = -1;
		serLinks.arraySize++;
		SerializedProperty newLink = serLinks.GetArrayElementAtIndex(serLinks.arraySize - 1);
		newLink.FindPropertyRelative("m_ParentID").intValue = node.GetID();
		newLink.FindPropertyRelative("m_ChildID").intValue = newID;
		m_STree.ApplyModifiedProperties();
		ActTreeDirtyTimestamps.SetDirty(m_Tree);
	}

	int GetParentID(int nodeID)
	{
		foreach (Act.NodeLink link in m_Tree.GetAllNodeLinks())
		{
			if (link.GetChildID() == nodeID)
			{
				return link.GetParentID();
			}
		}
		return Act.Node.INVALID_ID;
	}

	List<Act.Node> GetSiblings(int nodeID)
	{
		int parentID = GetParentID(nodeID);
		List<Act.Node> siblings = new List<Act.Node>();
		foreach (Act.NodeLink link in m_Tree.GetAllNodeLinks())
		{
			if (link.GetParentID() == parentID && link.GetChildID() != nodeID)
			{
				Act.Node node = m_Tree.GetNode(link.GetChildID());
				if (node != null)
				{
					siblings.Add(node);
				}
			}
		}
		return siblings;
	}

	List<Act.Node> GetChildren(int nodeID)
	{
		List<Act.Node> children = new List<Act.Node>();
		foreach (Act.NodeLink link in m_Tree.GetAllNodeLinks())
		{
			if (link.GetParentID() == nodeID)
			{
				Act.Node node = m_Tree.GetNode(link.GetChildID());
				if (node != null)
				{
					children.Add(node);
				}
			}
		}
		return children;
	}

	void AddChildContext(object obj)
	{
		Act.Node node = (Act.Node)obj;
		AddChild(node);
	}

	void DeleteNodeContext(object obj)
	{
		Act.Node node = (Act.Node)obj;
		DeleteNode(node);
	}

	void MoveUpContext(object obj)
	{
		Act.Node node = (Act.Node)obj;
		MoveUp(node);
	}

	void MoveDownContext(object obj)
	{
		Act.Node node = (Act.Node)obj;
		MoveDown(node);
	}

	void CopyContext(object obj)
	{
		Act.Node node = (Act.Node)obj;
		ActNodeClipboard.Copy(node, m_Tree);
	}

	void PasteContext(object obj)
	{
		Act.Node node = (Act.Node)obj;
		ActNodeClipboard.Paste(node, m_Tree, ref m_STree);
	}

	void CleanupAll()
	{
		AssetDatabase.SaveAssets();
		string[] guids = AssetDatabase.FindAssets("t:ActTree");
		for (int i = 0; i < guids.Length; i++)
		{
			EditorUtility.DisplayProgressBar("Cleanup All", (i + 1) + "/" + guids.Length, (float)i / guids.Length);
			string treePath = AssetDatabase.GUIDToAssetPath(guids[i]);
			Cleanup(treePath);
		}
		AssetDatabase.SaveAssets();
		EditorUtility.ClearProgressBar();
	}

	void CleanupThis()
	{
		string treePath = AssetDatabase.GetAssetPath(m_Tree);
		AssetDatabase.SaveAssets();
		Cleanup(treePath);
		AssetDatabase.SaveAssets();
		m_Tree = AssetDatabase.LoadAssetAtPath<ActTree>(treePath);
		m_STree = new SerializedObject(m_Tree);
		Repaint();
	}

	static void Cleanup(string treePath)
	{
		// Doing all the things I can think of to make sure tree is in sync
		AssetDatabase.ImportAsset(treePath);
		ActTree tree = AssetDatabase.LoadAssetAtPath<ActTree>(treePath);
		if (tree == null)
		{
			Debug.LogError($"ActTreeEditorWindow.Cleanup({treePath}) Is not an ActTree");
			return;
		}
		SerializedObject sTree = new SerializedObject(tree);
		ActTreeDirtyTimestamps.SetDirty(tree);
		CleanupRecursive(tree, sTree, 0);
	}

	static void CleanupRecursive(ActTree tree, SerializedObject sTree, int recursionCount)
	{
		if (recursionCount > 50)
		{
			Debug.LogError($"ActTreeEditorWindow.Cleanup({tree.name}) Recursed over 50 times something went wrong");
			return;
		}

		int nodeOrphansKilled = 0;
		int linkOrphansKilled = 0;
		int oppOrphansKilled = 0;
		int trackMissingRefsKilled = 0;
		int conditionMissingRefsKilled = 0;

		int id = 0;
		List<Act.NodeLink> links = tree.GetAllNodeLinks();
		SerializedProperty serLinks = sTree.FindProperty("m_NodeLinks");
		if (links.Count != serLinks.arraySize)
		{
			Debug.LogError($"ActTreeEditorWindow.Cleanup({tree.name}) Links are out of sync");
			return;
		}
		for (int i = 0; i < links.Count; i++)
		{
			Act.NodeLink link = links[i];
			bool isOrphan =
				tree.GetNode(link.GetParentID()) == null ||
				tree.GetNode(link.GetChildID()) == null;
			if (isOrphan) // if we dont have a parent we should die
			{
				serLinks.DeleteArrayElementAtIndex(id);
				linkOrphansKilled++;
			}
			else
			{
				id++;
			}
		}
		if (linkOrphansKilled > 0)
		{
			sTree.ApplyModifiedProperties();
		}

		id = 0;
		List<Act.Node> nodes = tree.GetAllNodes();
		SerializedProperty serNodes = sTree.FindProperty("m_Nodes");
		if (nodes.Count != serNodes.arraySize)
		{
			Debug.LogError($"ActTreeEditorWindow.Cleanup({tree.name}) Nodes are out of sync");
			return;
		}
		for (int i = 0; i < nodes.Count; i++)
		{
			Act.Node node = nodes[i];
			bool isOrphan = true;
			foreach (Act.NodeLink link in tree.GetAllNodeLinks())
			{
				if (link.GetChildID() == node.GetID())
				{
					isOrphan = false;
					break;
				}
			}
			if (isOrphan) // if we dont have a parent we should die
			{
				serNodes.DeleteArrayElementAtIndex(id);
				nodeOrphansKilled++;
			}
			else
			{
				id++;
			}
		}
		if (nodeOrphansKilled > 0)
		{
			sTree.ApplyModifiedProperties();
		}

		id = 0;
		List<Act.NodeSequence> seqs = tree.GetAllOpportunities();
		SerializedProperty serOpps = sTree.FindProperty("m_Opportunities");
		if (seqs.Count != serOpps.arraySize)
		{
			Debug.LogError($"ActTreeEditorWindow.Cleanup({tree.name}) Opps are out of sync");
			return;
		}
		for (int i = 0; i < seqs.Count; i++)
		{
			Act.NodeSequence sequence = seqs[i];
			bool foundFrom = false;
			bool foundTo = false;
			foreach (Act.Node node in nodes)
			{
				if (sequence.GetFromID() == node.GetID())
				{
					foundFrom = true;
				}
				if (sequence.GetToID() == node.GetID())
				{
					foundTo = true;
				}
				if (foundFrom && foundTo)
				{
					break;
				}
			}
			if (!foundFrom || !foundTo) // if we dont have a from or we dont have a to we should die
			{
				serOpps.DeleteArrayElementAtIndex(id);
				oppOrphansKilled++;
			}
			else
			{
				id++;
			}
		}
		if (oppOrphansKilled > 0)
		{
			sTree.ApplyModifiedProperties();
		}

		id = 0;
		List<ActTrack> tracks = tree.GetTracks();
		SerializedProperty serTracks = sTree.FindProperty("m_Tracks");
		if (tracks.Count != serTracks.arraySize)
		{
			Debug.LogError($"ActTreeEditorWindow.Cleanup({tree.name}) Tracks are out of sync");
			return;
		}
		for (int i = 0; i < tracks.Count; i++)
		{
			ActTrack track = tracks[i];
			if (track == null)
			{
				serTracks.DeleteArrayElementAtIndex(id);
				trackMissingRefsKilled++;
			}
			else
			{
				track.EditorUpdateName();
				id++;
			}
		}
		if (trackMissingRefsKilled > 0)
		{
			sTree.ApplyModifiedProperties();
		}

		id = 0;
		List<ActCondition> conditions = tree.GetConditions();
		SerializedProperty serConditions = sTree.FindProperty("m_Conditions");
		if (conditions.Count != serConditions.arraySize)
		{
			Debug.LogError($"ActTreeEditorWindow.Cleanup({tree.name}) Conditions are out of sync");
			return;
		}
		for (int i = 0; i < conditions.Count; i++)
		{
			ActCondition condition = conditions[i];
			if (condition == null)
			{
				serConditions.DeleteArrayElementAtIndex(id);
				conditionMissingRefsKilled++;
			}
			else
			{
				condition.EditorUpdateName();
				id++;
			}
		}
		if (conditionMissingRefsKilled > 0)
		{
			sTree.ApplyModifiedProperties();
		}

		//AssetDatabase.Refresh(); // This should catch nodes or conditions that have been renamed
		Core.Str.AddLine($"ActTreeEditorWindow.Cleanup({tree.name}) Recursion {recursionCount}");
		if (nodeOrphansKilled > 0) Core.Str.AddLine($"Killed {nodeOrphansKilled} node orphans");
		if (linkOrphansKilled > 0) Core.Str.AddLine($"Killed {linkOrphansKilled} link orphans");
		if (oppOrphansKilled > 0) Core.Str.AddLine($"Killed {oppOrphansKilled} opp orphans");
		if (trackMissingRefsKilled > 0) Core.Str.AddLine($"Killed {trackMissingRefsKilled} track missing refs");
		if (conditionMissingRefsKilled > 0) Core.Str.AddLine($"Killed {conditionMissingRefsKilled} condition missing refs");
		if (nodeOrphansKilled + linkOrphansKilled + oppOrphansKilled > 0)
		{
			CleanupRecursive(tree, sTree, ++recursionCount);
		}
		else
		{
			int destroyedTracks = 0;
			int destroyedConditions = 0;
			Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(tree));
			foreach (Object obj in objs)
			{
				switch (obj)
				{
					case ActTrack track:
						if (tree.GetNode(track.GetNodeID()) == null || !tree.GetTracks().Contains(track))
						{
							DestroyImmediate(obj, true);
							destroyedTracks++;
						}
						break;
					case ActCondition cond:
						if (tree.GetNode(cond.GetNodeID()) == null || !tree.GetConditions().Contains(cond))
						{
							DestroyImmediate(obj, true);
							destroyedConditions++;
						}
						break;
				}
			}
			if (destroyedTracks > 0 || destroyedConditions > 0)
			{
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tree));
				EditorUtility.SetDirty(tree);
				if (destroyedTracks > 0)
				{
					ActTrackDrawer.UpdateTrackList(tree, ref sTree);
				}
				if (destroyedConditions > 0)
				{
					ActConditionDrawer.RebuildConditionsList(tree, ref sTree);
				}
			}
			if (destroyedTracks > 0) Core.Str.AddLine($"Destroyed {destroyedTracks} orphan tracks");
			if (destroyedConditions > 0) Core.Str.AddLine($"Destroyed {destroyedConditions} orphan conditions");
			Core.Str.AddLine($"Clean up complete");
			Debug.Log(Core.Str.Finish());
		}
	}

	void OpenContext(Act.Node node)
	{
		GenericMenu menu = new GenericMenu();
		menu.AddItem(new GUIContent("Add Child"), false, AddChildContext, node);
		menu.AddItem(new GUIContent("Delete"), false, DeleteNodeContext, node);
		menu.AddSeparator("");
		menu.AddItem(new GUIContent("Move Up"), false, MoveUpContext, node);
		menu.AddItem(new GUIContent("Move Down"), false, MoveDownContext, node);
		menu.AddSeparator("");
		menu.AddItem(new GUIContent("Copy"), false, CopyContext, node);
		if (ActNodeClipboard.CanPaste())
		{
			menu.AddItem(new GUIContent("Paste Child"), false, PasteContext, node);
		}
		else
		{
			menu.AddDisabledItem(new GUIContent("Paste Child"));
		}
		menu.AddItem(new GUIContent("Cleanup"), false, CleanupThis);
		menu.AddItem(new GUIContent("Cleanup All"), false, CleanupAll);
		menu.ShowAsContext();
	}

	void MoveUp(Act.Node node)
	{
		int parentID = GetParentID(node.GetID());
		int swapIndex = -1; // We are going to look for the closest sibling above the node to swap with
		List<Act.NodeLink> links = m_Tree.GetAllNodeLinks();
		for (int i = 0; i < links.Count; i++)
		{
			Act.NodeLink link = links[i];
			if (link.GetParentID() != parentID) // Check if this link is a sibiling
			{
				continue;
			}
			if (link.GetChildID() == node.GetID()) // Found node id in the list, see if we have a valid index to swap with
			{
				if (swapIndex >= 0)
				{
					m_STree.FindProperty("m_NodeLinks").MoveArrayElement(i, swapIndex);
					m_STree.ApplyModifiedProperties();
					ActTreeDirtyTimestamps.SetDirty(m_Tree);
				}
				break;
			}
			swapIndex = i; // This index is a swap candidate
		}
	}

	void MoveDown(Act.Node node)
	{
		int parentID = GetParentID(node.GetID());
		int swapIndex = -1; // We are going to look for first node after this one to swap with
		List<Act.NodeLink> links = m_Tree.GetAllNodeLinks();
		for (int i = 0; i < links.Count; i++)
		{
			Act.NodeLink link = links[i];
			if (link.GetParentID() != parentID) // Check if this link is a sibiling
			{
				continue;
			}
			if (swapIndex >= 0) // If swap index is set then swap the next time we find a sibiling
			{
				m_STree.FindProperty("m_NodeLinks").MoveArrayElement(i, swapIndex);
				m_STree.ApplyModifiedProperties();
				ActTreeDirtyTimestamps.SetDirty(m_Tree);
				break;
			}
			if (link.GetChildID() == node.GetID()) // Found node id in the list, see if we have a valid index to swap with
			{
				swapIndex = i;
			}
		}
	}

	public void SetSelectedNode(int nodeID)
	{
		m_SelectedNodeID = nodeID;
		m_NodeHistory[m_CurrentTreePath] = nodeID;
		List<Act.Node> nodes = m_Tree.GetAllNodes();
		bool foundNode = false;
		for (int i = 0; i < nodes.Count; i++)
		{
			Act.Node n = nodes[i];
			if (n.GetID() == m_SelectedNodeID)
			{
				foundNode = true;
				SerializedProperty serNodeList = m_STree.FindProperty("m_Nodes");
				ActNodeEditorWindow.Get().SetNode(m_Tree, m_STree, n, serNodeList.GetArrayElementAtIndex(i));
				Focus();
				break;
			}
		}
		if (!foundNode)
		{
			ActNodeEditorWindow.Get().ClearNode();
			Focus();
		}
	}

	public void UpdateTree()
	{
		if (m_Tree != null)
		{
			m_STree = new SerializedObject(m_Tree);
			ActTreeDirtyTimestamps.SetDirty(m_Tree);
		}
	}

	public void CheckInput()
	{
		if (Event.current.type != EventType.KeyDown)
		{
			return;
		}
		switch (Event.current.keyCode)
		{
			case KeyCode.UpArrow:
			case KeyCode.DownArrow:
			case KeyCode.RightArrow:
			case KeyCode.LeftArrow:
				if (m_SelectedNodeID == Act.Node.INVALID_ID)
				{
					m_SelectedNodeID = Act.Node.ROOT_ID;
				}
				break;
		}
		switch (Event.current.keyCode)
		{
			case KeyCode.UpArrow:// step through siblings
				MoveToSibling(-1);
				Repaint();
				break;
			case KeyCode.DownArrow: // step through siblings
				MoveToSibling(1);
				Repaint();
				break;
			case KeyCode.RightArrow: // step into leaf
				if (Event.current.control)
				{
					if (m_RecentTreesDrawer.NextHistory(out string recentPath))
					{
						SetActTree(recentPath);
						Repaint();
					}
					break;
				}
				List<Act.Node> children = GetChildren(m_SelectedNodeID);
				if (children.Count > 0)
				{
					SetSelectedNode(children[0].GetID());
				}
				else
				{
					MoveToSibling(1);
				}
				Repaint();
				break;
			case KeyCode.LeftArrow: // step up branch
				if (Event.current.control)
				{
					if (m_RecentTreesDrawer.PrevHistory(out string recentPath))
					{
						SetActTree(recentPath);
						Repaint();
					}
					break;
				}
				int parentID = GetParentID(m_SelectedNodeID);
				if (parentID != m_Tree.GetRootNode().GetID() && parentID != Act.Node.INVALID_ID)
				{
					SetSelectedNode(parentID);
				}
				else
				{
					MoveToSibling(-1);
				}
				Repaint();
				break;
		}
	}

	void MoveToSibling(int moveBy)
	{
		List<Act.Node> siblings = GetChildren(GetParentID(m_SelectedNodeID));
		if (siblings.Count > 0)
		{
			int index = siblings.FindIndex(x => x.GetID() == m_SelectedNodeID);
			index += moveBy;
			if (index >= siblings.Count)
			{
				index = 0;
			}
			else if (index < 0)
			{
				index = siblings.Count - 1;
			}
			SetSelectedNode(siblings[index].GetID());
		}
	}
}
