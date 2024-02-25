
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Act2
{
	public class ActNodeEditorWindow2 : EditorWindow
	{
		private AddTracksAndTransitionsDrawer m_AddContentDrawer = new AddTracksAndTransitionsDrawer();
		private ConditionDrawer m_ConditionDrawer = new ConditionDrawer();
		private TransitionDrawer m_TransitionDrawer = new TransitionDrawer();
		private TrackDrawer m_TrackDrawer = new TrackDrawer();

		private ActTree2 m_Tree = null;
		private SerializedObject m_STree = null;
		private int m_NodeID = Node.INVALID_ID;
		public Node GetNode() => m_Tree.GetNode(m_NodeID);

		private Vector2 m_ScrollPos = Vector2.zero;
		private Vector2 m_ScrollPos2 = Vector2.zero;

		private NodeItemHotKeys m_HotKeys = new NodeItemHotKeys();

		[MenuItem("Window/Act Tree 2/Node Editor")]
		public static ActNodeEditorWindow2 Get()
		{
			ActNodeEditorWindow2 window = EditorWindow.GetWindow<ActNodeEditorWindow2>();
			window.titleContent = new GUIContent("Act Node 2", EditorUtil.ActTreeIcon);
			window.Show();
			return window;
		}

		public void SetNode(ActTree2 tree, SerializedObject sTree, Node node, SerializedProperty sNode)
		{
			m_Tree = tree;
			m_STree = sTree;
			m_NodeID = node.ID;
			SelectionDrawer.AutoSelectTrack(m_Tree, node);
			Focus();
		}

		public void ClearNode()
		{
			m_Tree = null;
			m_STree = null;
			m_NodeID = Node.INVALID_ID;
			Repaint();
		}

		private void OnEnable()
		{
			Undo.undoRedoPerformed += OnUndo;
		}

		private void OnDisable()
		{
			Undo.undoRedoPerformed -= OnUndo;
		}

		private void OnUndo()
		{
			SelectionDrawer.ClearEverything();
			if (m_Tree == null)
			{
				m_STree = null;
				m_NodeID = Node.INVALID_ID;
				return;
			}
			// Clear references
			m_Tree = AssetDatabase.LoadAssetAtPath<ActTree2>(AssetDatabase.GetAssetPath(m_Tree)); // Undo could have made changes to tree
			m_STree = new SerializedObject(m_Tree);
			Repaint();
		}

		private void OnGUI()
		{
			
			if (m_Tree == null || m_STree == null)
			{
				return;
			}

			int nodeIndex = m_Tree.GetNodeIndex(m_NodeID);
			if (nodeIndex < 0)
			{
				return;
			}
			Node node = m_Tree.Nodes[nodeIndex];
			SerializedProperty sNode = m_STree.FindProperty("m_Nodes").GetArrayElementAtIndex(nodeIndex);
			
			EditorGUILayout.Space(2.0f);

			Rect labelRect = EditorGUILayout.GetControlRect(false, height: 2.0f * EditorGUIUtility.singleLineHeight);
			EditorGUI.DrawRect(labelRect, NodeDrawer.Styles.Root.BackgroundColor.Value);
			GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
			labelStyle.normal.textColor = NodeDrawer.Styles.Root.GetTextColor();
			labelStyle.fontSize = 18;
			NodeDrawer.AttachNodeIcon(labelRect, out labelRect, labelStyle, m_Tree, node, NodeDrawer.Styles.Normal);

			float playButtonWidth = 54.0f;
			labelRect.x += 2.0f;
			labelRect.width -= playButtonWidth + 2.0f + 4.0f;
			GUI.Label(labelRect, node.Name, labelStyle);

			labelRect.height = EditorGUIUtility.singleLineHeight;
			labelRect.y += 4.0f;
			labelStyle.fontSize = 0;
			labelStyle.alignment = TextAnchor.MiddleRight;
			GUI.Label(labelRect, Core.Str.Build("ID: ", node.GetID().ToString(), "  "), labelStyle);

			labelRect.x += labelRect.width;
			labelRect.width = playButtonWidth;
			GUI.enabled = NodeDebugPlay.CanPlay(m_Tree);
			if (GUI.Button(labelRect, "Play \u25ba", EditorStyles.miniButton))
			{
				NodeDebugPlay.Play(m_Tree, node);
			}
			GUI.enabled = true;

			SerializedProperty sAvailable = sNode.FindPropertyRelative("m_Availability");
			EditorGUILayout.PropertyField(sAvailable);

			EditorGUILayout.PropertyField(sNode.FindPropertyRelative("m_NodeType"));
			EditorGUILayout.PropertyField(sNode.FindPropertyRelative("m_EventType"));
			DrawPointer(node, ref sNode);
			EditorGUILayout.PropertyField(sNode.FindPropertyRelative("m_Notes"));
			EditorGUILayout.Space();

			m_ConditionDrawer.OnGUI(m_Tree, ref m_STree, node, ref sNode);
			ApplyChanges();
			EditorGUILayout.Space();
			m_AddContentDrawer.OnGUI(m_Tree, node, ref sNode);
			ApplyChanges();

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);
			
			m_TransitionDrawer.OnGUI(m_Tree, ref m_STree, node, ref sNode);
			ApplyChanges();
			if (node.Transitions.Count > 0)
			{
				EditorGUILayout.Space();
			}
			m_TrackDrawer.OnGUI(m_Tree, ref m_STree, node, ref sNode);
			ApplyChanges();
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();

			if (SelectionDrawer.IsPrimarySelection(m_Tree, node, NodeItemType.Track))
			{
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				m_ScrollPos2 = EditorGUILayout.BeginScrollView(m_ScrollPos2);
				EditorGUILayout.LabelField(SelectionDrawer.TryGetTimedItem(m_Tree, node).ToString(), EditorStyles.boldLabel);
				SelectionDrawer.OnGUI(m_Tree, node, ref sNode, out _);
				EditorGUILayout.EndScrollView();
				EditorGUILayout.EndVertical();
				ApplyChanges();
			}

			GUILayout.FlexibleSpace();

			m_HotKeys.OnGUI(m_Tree, ref m_STree, node, ref sNode, out bool handledHotkey);
			if (handledHotkey)
			{
				Repaint();
			}
		}

		private void DrawPointer(Node node, ref SerializedProperty sNode)
		{
			EditorGUILayout.BeginHorizontal();
			SerializedProperty serPointerID = sNode.FindPropertyRelative("m_PointerID");
			int pointerID = EditorGUILayout.IntField(serPointerID.displayName, serPointerID.intValue);
			serPointerID.intValue = pointerID;
			string error = null;
			if (pointerID != Node.INVALID_ID)
			{
				Node referencedNode = m_Tree.GetNode(pointerID);
				if (referencedNode != null)
				{
					if (referencedNode.IsEventRequired(out System.Type pointerEventType) &&
						(!node.IsEventRequired(out System.Type eventType) || !pointerEventType.IsAssignableFrom(eventType)))
					{
						error = $"Pointer invalid, node '{referencedNode.Name}' requires Event Type: {referencedNode.EventTypeName}";
					}

					Rect nodeRect = EditorGUILayout.GetControlRect();
					if (NodeButton(nodeRect, m_Tree, referencedNode, !string.IsNullOrEmpty(error) ? new Color32(230, 73, 64, 255) : null))
					{
						ActTreeEditorWindow2.Get().SetSelectedNode(referencedNode.GetID());
					}
				}
			}
			EditorGUILayout.EndHorizontal();
			if (!string.IsNullOrEmpty(error))
			{
				EditorGUILayout.HelpBox(error, MessageType.Error);
			}
		}

		public static bool NodeButton(Rect nodeRect, ActTree2 tree, Node node, Color? overrideColor = null)
		{
			bool pressed = false;
			if (GUI.Button(nodeRect, string.Empty, GUIStyle.none))
			{
				pressed = true;
			}
			Color rectColor = overrideColor.HasValue ? overrideColor.Value : NodeDrawer.Styles.Root.BackgroundColor.Value;
			EditorGUI.DrawRect(nodeRect, rectColor);
			GUIStyle nodeLabelStyle = new GUIStyle(EditorStyles.boldLabel);
			nodeLabelStyle.normal.textColor = NodeDrawer.Styles.Root.GetTextColor();
			Node.Properties refNodeProperties = Node.GetProperties(tree, node);
			NodeDrawer.AttachNodeIcon(nodeRect, out nodeRect, nodeLabelStyle, refNodeProperties);
			EditorGUI.LabelField(nodeRect, refNodeProperties.Name, nodeLabelStyle);
			UberPickerGUI.AttachAssetSelectIcon(ref nodeRect);
			return pressed;
		}

		private void ApplyChanges()
		{
			if (m_STree.hasModifiedProperties)
			{
				m_STree.ApplyModifiedProperties();
			}
		}
	}
}
