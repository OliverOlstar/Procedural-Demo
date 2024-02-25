
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public class ActNodeEditorWindow : EditorWindow
{
	ActAddContentDrawer m_SequenceDrawer = new ActAddContentDrawer();
	ActConditionDrawer m_ConditionDrawer = new ActConditionDrawer();
	ActTrackDrawer m_TrackDrawer = new ActTrackDrawer();

	ActTree m_Tree = null;
	SerializedObject m_STree = null;
	Act.Node m_Node = null;
	public Act.Node GetNode() { return m_Node; }
	SerializedProperty m_SNode = null;

	[MenuItem("Window/Act Tree/Node Editor")]
	public static ActNodeEditorWindow Get()
	{
		ActNodeEditorWindow window = EditorWindow.GetWindow<ActNodeEditorWindow>("Act Node");
		window.Show();
		return window;
	}

	public void SetNode(ActTree tree, SerializedObject sTree, Act.Node node, SerializedProperty sNode)
	{
		m_Tree = tree;
		m_STree = sTree;
		m_Node = node;
		m_SNode = sNode;
		ActSelectionDrawer.ValidateSelected(m_Tree, m_Node);
		Focus();
	}

	public void ClearNode()
	{
		m_Tree = null;
		m_STree = null;
		m_Node = null;
		m_SNode = null;
		ActSelectionDrawer.ClearSelection();
		Repaint();
	}

	void OnEnable()
	{
		Undo.undoRedoPerformed += OnUndo;
	}

	void OnDisable()
	{
		Undo.undoRedoPerformed -= OnUndo;
	}

	void OnUndo()
	{
		if (m_Tree == null || m_STree == null || m_Node == null || m_SNode == null)
		{
			return;
		}

//		int nodeID = m_Node.GetID();
//		string path = AssetDatabase.GetAssetPath(m_Tree);
//
//		// Clear references
//		m_Tree = null;
//		m_STree = null;
//		m_Node = null;
//		m_SNode = null;
//
//		// Try to get updated references
//		m_Tree = AssetDatabase.LoadAssetAtPath<ActTree>(path);
//		if (m_Tree != null)
//		{
//			m_STree = new SerializedObject(m_Tree);
//			for (int i = 0; i < m_Tree.GetTracks().Count; i++)
//			{
//				if (m_Tree.GetAllNodes()[i].GetID() == nodeID)
//				{
//					m_Node = m_Tree.GetAllNodes()[i];
//					m_SNode = m_STree.FindProperty("m_Nodes").GetArrayElementAtIndex(i);
//					break;
//				}
//			}
//		}

		AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_Tree)); // Undo could have made changes to tree
		ActTreeDirtyTimestamps.SetDirty(m_Tree);
		Repaint();
	}

	void OnGUI()
	{
		if (m_Tree == null || m_STree == null || m_Node == null || m_SNode == null)
		{
			return;
		}

		EditorGUILayout.BeginHorizontal();
		SerializedProperty serName = m_SNode.FindPropertyRelative("m_Name");
		serName.stringValue = EditorGUILayout.TextField(serName.stringValue);

		GUILayout.Label(Core.Str.Build("ID: ", m_Node.GetID().ToString()), GUILayout.ExpandWidth(false));
		GUIStyle style = new GUIStyle(GUI.skin.button);
		style.normal.textColor = ActNodeDebugPlay.CanPlay(m_Tree) ? Color.black : Color.grey;
		if (GUILayout.Button("Play", style, GUILayout.ExpandWidth(false)))
		{
			ActNodeDebugPlay.Play(m_Tree, m_Node);
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		SerializedProperty serPointerID = m_SNode.FindPropertyRelative("m_PointerID");
		int pointerID = EditorGUILayout.IntField(serPointerID.displayName, serPointerID.intValue);
		serPointerID.intValue = pointerID;
		Act.Node referencedNode = null;
		if (pointerID != Act.Node.INVALID_ID)
		{
			foreach (Act.Node node in m_Tree.GetAllNodes())
			{
				if (node.GetID() == pointerID)
				{
					referencedNode = node;
					if (GUILayout.Button(referencedNode.GetName()))
					{
						ActTreeEditorWindow.Get().SetSelectedNode(referencedNode.GetID());
					}
					break;
				}
			}
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();

		m_ConditionDrawer.OnGUI(m_Tree, ref m_STree, m_Node);

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Content", EditorStyles.boldLabel);
		m_SequenceDrawer.OnGUI(m_Tree, ref m_STree, m_Node);

		EditorGUILayout.BeginVertical();

		m_TrackDrawer.OnGUI(m_Tree, ref m_STree, m_Node);

		if (ActSelectionDrawer.GetTrack(m_Tree, m_Node) != null)
		{
			ActSelectionDrawer.OnGUI(m_Tree, ref m_STree, m_Node);
		}

		GUILayout.FlexibleSpace();

		EditorGUILayout.EndVertical();

		if (m_STree.hasModifiedProperties)
		{
			m_STree.ApplyModifiedProperties();
			ActTreeDirtyTimestamps.SetDirty(m_Tree);
		}
	}
}
