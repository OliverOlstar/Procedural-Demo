using ActCore;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Act2
{
	public class GraphDrawer
	{
		public static readonly Color PLAYING_CURRENTLY_COLOR = new Color32(103, 243, 10, 255);
		public static readonly Color PLAYING_PREVIOUSLY_COLOR = new Color32(23, 121, 53, 255);
		public static readonly Color TRANSITION_COLOR = Color.white;

		private int m_ParentID = Node.ROOT_ID;
		public int ParentID => m_ParentID;

		private int m_DraggingNodeID = Node.INVALID_ID;
		private Vector2? m_DraggingOffset = null;

		private int m_PendingRenameNodeID = Node.INVALID_ID;
		private bool m_AsyncRepaintRequest = false;

		private ActGraphController m_Graph = new();
		public ActGraphController Graph => m_Graph;
		private ActTreeRuntimeDebugController m_RuntimeDebugging = new();

		public bool RequestRepaint()
		{
			bool repaint = m_AsyncRepaintRequest; // m_AsyncRepaintRequest is a work around because we can't request call Repaint from an async Task
			m_AsyncRepaintRequest = false;
			return repaint || m_RuntimeDebugging.IsDebugging;
		}

		public void OnGUI(ActTree2 tree, SerializedObject sTree, ActTreeEditorWindow2 window, Rect windowPosition, int selectedNodeID)
		{
			m_RuntimeDebugging.Update(tree);

			Node parent = tree.GetNode(m_ParentID);
			if (parent == null)
			{
				m_ParentID = Node.ROOT_ID;
			}
			else
			{
				GUILayout.BeginHorizontal();
				List<int> chain = new List<int>();
				int parentID = m_ParentID;
				do
				{
					chain.Add(parentID);
					parentID = tree.GetParentID(parentID);
				}
				while (parentID != Node.INVALID_ID);
				for (int i = chain.Count - 1; i >= 1; i--)
				{
					if (GUILayout.Button(tree.GetNode(chain[i]).Name, GUILayout.ExpandWidth(false)))
					{
						m_ParentID = chain[i];
						window.Repaint();
					}
				}
				GUILayout.Label(tree.GetNode(chain[0]).Name, GUILayout.ExpandWidth(false));
				GUILayout.EndHorizontal();
			}

			m_Graph.Begin(out bool repaint);
			if (repaint)
			{
				window.Repaint();
			}
			if (parent != null)
			{
				DrawGraph(tree, sTree, window, windowPosition, selectedNodeID);
			}
			m_Graph.End();
		}

		public void DrawGraph(ActTree2 tree, SerializedObject sTree, ActTreeEditorWindow2 window, Rect windowPosition, int selectedNodeID)
		{
			Node parent = tree.GetNode(m_ParentID);
			List<Node> nodes = tree.GetChildren(m_ParentID);
			nodes.Add(parent);
			Dictionary<int, Rect> nodePositions = new Dictionary<int, Rect>();
			Node mouseOverNode = null;
			for (int i = 0; i < nodes.Count; i++)
			{
				Node node = nodes[i];
				bool isParent = node.ID == m_ParentID;
				if (!ActTreeGraphUtil.TryGetPosition(node, isParent, out Vector2 pos))
				{
					pos = ActTreeGraphUtil.GetDefaultPosition(i, nodes.Count, isParent);
				}
				pos += m_Graph.Offset;
				Rect nodeRect = ActTreeGraphUtil.PositionToRect(pos, node.Name);
				if (nodeRect.Contains(Event.current.mousePosition))
				{
					mouseOverNode = node;
				}
				nodePositions.Add(node.GetID(), nodeRect);
			}

			Node.GetEntryNodes(tree, nodes, out Dictionary<Node, int> entryNodeOrder, out int defaultEntryNodeID);
			foreach (KeyValuePair<Node, int> pair in entryNodeOrder)
			{
				Node node = pair.Key;
				int order = pair.Value;
				Rect toRect = nodePositions[node.ID];
				Rect parentRect = nodePositions[m_ParentID];

				Color c = m_RuntimeDebugging.IsCurrentlyPlayingNode(node.ID) ? PLAYING_CURRENTLY_COLOR :
					m_RuntimeDebugging.WasPreviouslyPlayingNode(node.ID) ? PLAYING_PREVIOUSLY_COLOR :
					TRANSITION_COLOR;

				ActTreeGraphUtil.DrawArrowFromRectToRect(parentRect, toRect, c);
				if (node.ID != defaultEntryNodeID)
				{
					Vector2 center = Vector2.Lerp(parentRect.center, toRect.center, 0.5f);
					Vector2 size = 12 * Vector2.one;
					Rect numberRect = new Rect(center - 0.5f * size, size);
					EditorGUI.DrawRect(numberRect, GUI.skin.label.normal.textColor);
					GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
					style.normal.textColor = Color.black;
					style.alignment = TextAnchor.MiddleCenter;
					style.fontSize = 9;
					EditorGUI.LabelField(numberRect, order.ToString(), style);
				}
			}

			UpdateSelection(window, tree, sTree, mouseOverNode, nodePositions);

			for (int i = 0; i < nodes.Count; i++)
			{
				Node node = nodes[i];
				if (node.ID != m_ParentID)
				{
					foreach (NodeTransition trans in node.Transitions)
					{
						DrawTransition(nodePositions, node, trans, TRANSITION_COLOR);
					}
				}
			}
			if (m_RuntimeDebugging.IsDebugging) // Draw debug transition on top of other transitions
			{
				for (int i = 0; i < nodes.Count; i++)
				{
					Node node = nodes[i];
					if (node.ID == m_ParentID || !m_RuntimeDebugging.WasTransitionTakenFromNode(node.GetID()))
					{
						continue;
					}
					foreach (NodeTransition trans in node.Transitions)
					{
						if (m_RuntimeDebugging.IsCurrentlyPlayingNode(trans.GetToID()))
						{
							DrawTransition(nodePositions, node, trans, PLAYING_CURRENTLY_COLOR);
						}
					}
				}
			}

			for (int i = 0; i < nodes.Count; i++)
			{
				Node node = nodes[i];
				Rect nodeRect = nodePositions[node.ID];
				DrawNode(window, tree, node, nodeRect, node.ID == m_ParentID);
			}

			TryShowContextMenu(window, tree, parent, mouseOverNode, entryNodeOrder, defaultEntryNodeID);

			if (sTree.hasModifiedProperties)
			{
				sTree.ApplyModifiedProperties();
			}
		}

		private void DrawTransition(Dictionary<int, Rect> nodePositions, Node node, NodeTransition trans, Color color)
		{
			if (trans.GetToID() == node.ID)
			{
				ActTreeGraphUtil.DrawLoop(nodePositions[node.ID], color);
			}
			else if (nodePositions.TryGetValue(trans.GetToID(), out Rect toNodeRect))
			{
				ActTreeGraphUtil.DrawArrowFromRectToRect(nodePositions[node.ID], toNodeRect, color);
			}
			else
			{
				Debug.LogWarning($"ActTreeGraphDrawer.DrawTransition() {node.Name} has invalid transition to {trans.GetToID()} which doesn't exist");
			}
		}

		private void UpdateSelection(ActTreeEditorWindow2 window, ActTree2 tree, SerializedObject sTree, Node overNode, Dictionary<int, Rect> nodePositions)
		{
			Event inputEvent = Event.current;
			if (!inputEvent.isMouse || inputEvent.button != 0)
			{
				return;
			}

			if (inputEvent.clickCount > 1 && overNode != null)
			{
				if (overNode.ID == m_ParentID)
				{
					m_ParentID = tree.GetParentID(m_ParentID);
				}
				else if (tree.HasChildren(overNode.ID))
				{
					m_ParentID = overNode.ID;
				}
				else if (overNode.GetPointerID() != Node.INVALID_ID)
				{
					m_ParentID = overNode.GetPointerID();
				}
				m_PendingRenameNodeID = Node.INVALID_ID;
				window.FinishRenamingNode();
				window.Repaint();
				return;
			}

			if (m_DraggingNodeID == Node.INVALID_ID)
			{
				if (inputEvent.type == EventType.MouseDown)
				{
					if (overNode != null)
					{
						m_DraggingNodeID = overNode.ID;
						m_DraggingOffset = null;
					}
					else
					{
						window.ClearSelection();
					}
					window.Repaint();
				}
			}
			else
			{
				switch (inputEvent.type)
				{
					case EventType.MouseDrag:
						bool isRenaming = m_PendingRenameNodeID != Node.INVALID_ID || window.RenameNodeID != Node.INVALID_ID;
						if (!isRenaming)
						{
							Vector2 pos = inputEvent.mousePosition;
							Rect nodeRect = nodePositions[m_DraggingNodeID];
							if (!m_DraggingOffset.HasValue)
							{
								m_DraggingOffset = nodeRect.position - pos;
							}
							nodeRect.position = pos - m_Graph.Offset + m_DraggingOffset.Value;
							ActTreeGraphUtil.SetPosition(tree, sTree, m_DraggingNodeID, m_DraggingNodeID == m_ParentID, nodeRect.position);
							window.Repaint();
						}
						break;
					case EventType.MouseUp:
						if (!m_DraggingOffset.HasValue)
						{
							bool isSelected = window.SelectedNodeID == m_DraggingNodeID;
							if (!isSelected)
							{
								window.SetSelectedNode(m_DraggingNodeID);
							}
							else if (m_PendingRenameNodeID == Node.INVALID_ID)
							{
								// Gah this is annoying... A double click steps into a node, but two slow clicks select and then renames a node
								// Need to start a task to wait and see if a double click happens before renaming a node
								m_PendingRenameNodeID = m_DraggingNodeID;
								Task t = new Task(() => Rename(window));
								t.Start();
							}
						}
						m_DraggingNodeID = Node.INVALID_ID;
						window.Repaint();
						break;
				}
			}
		}

		private void Rename(ActTreeEditorWindow2 window)
		{
			Thread.Sleep(100);
			if (m_PendingRenameNodeID == window.SelectedNodeID)
			{
				window.StartRenamingNode(m_PendingRenameNodeID);
				m_AsyncRepaintRequest = true; // Can't call window.Repaint() from a Task so use this work around to request a repaint
			}
			m_PendingRenameNodeID = Node.INVALID_ID;
		}

		private void DrawNode(ActTreeEditorWindow2 window, ActTree2 tree, Node node, Rect nodeRect, bool isParent)
		{
			Node.Properties nodeProperties = Node.GetProperties(tree, node);
			bool isSelected = node.GetID() == window.SelectedNodeID;
			bool isCopied = NodeClipboard.IsCopied(node, tree);
            NodeDrawer.Style nodeStyle = NodeDrawer.Styles.Normal;
            if (isCopied)
            {
	            nodeStyle = NodeDrawer.Styles.Copied;
            }
            else if (isSelected)
            {
	            nodeStyle = NodeDrawer.Styles.Selected;
            }
            else if (nodeProperties.IsRoot)
            {
	            nodeStyle = NodeDrawer.Styles.Root;
            }

            Color bgColor;
            if (nodeStyle.BackgroundColor.HasValue)
            {
	            bgColor = nodeStyle.BackgroundColor.Value;
            }
            else if (m_DraggingNodeID == node.GetID())
            {
	            bgColor =  m_DraggingOffset.HasValue ? Color.green : Color.blue;
            }
            else
            {
	            bgColor = new Color32(96, 96, 96, 255);
            }

			Vector2 boarderSize = 2 * Vector2.one;
			Rect borderRect = nodeRect;
			borderRect.position -= boarderSize;
			borderRect.size += 2 * boarderSize;
			if (m_RuntimeDebugging.IsCurrentlyPlayingNode(node.ID))
			{
				EditorGUI.DrawRect(borderRect, PLAYING_CURRENTLY_COLOR);
			}
			else if (m_RuntimeDebugging.WasPreviouslyPlayingNode(node.ID))
			{
				EditorGUI.DrawRect(borderRect, PLAYING_PREVIOUSLY_COLOR);
			}

			EditorGUI.DrawRect(nodeRect, bgColor);
            
            GUIStyle guiStyle = new GUIStyle(GUI.skin.label);
            guiStyle.fontStyle = isParent ? FontStyle.Bold : FontStyle.Normal;

			NodeDrawer.DrawNodeLabel(window, nodeRect, guiStyle, nodeProperties, NodeDrawer.ParentsProperties.None, nodeStyle);

			if (isParent)
            {
	            GUIContent tabContent = new GUIContent("Parent");
	            GUIStyle tabStyle = new GUIStyle(GUI.skin.label);
	            tabStyle.fontSize = 10;
	            tabStyle.alignment = TextAnchor.MiddleLeft;
	            Vector2 size = tabStyle.CalcSize(tabContent);
	            size.y -= 2;
	            Rect tabPos = new Rect(nodeRect.x, nodeRect.y - size.y, size.x, size.y);
	            EditorGUI.DrawRect(tabPos, bgColor);
	            EditorGUI.LabelField(tabPos, tabContent, tabStyle);
            }
            else if (!nodeProperties.IsLeaf)
            {
				int childCount = tree.GetChildCount(node.ID);
				string tabLabel =
					nodeProperties.ReferencedNode != null ? "*" + nodeProperties.ReferencedNode.Name :
					childCount == 1 ? "1 Child" :
					childCount + " Children";
				GUIContent tabContent = new GUIContent(tabLabel);
				GUIStyle tabStyle = new GUIStyle(GUI.skin.label);
	            tabStyle.fontSize = 9;
	            tabStyle.alignment = TextAnchor.MiddleRight;
	            Vector2 size = tabStyle.CalcSize(tabContent);
	            size.y -= 2;
	            Rect tabPos = new Rect(nodeRect.xMax - size.x, nodeRect.yMax, size.x, size.y);
	            EditorGUI.DrawRect(tabPos, bgColor);
	            EditorGUI.LabelField(tabPos, tabContent, tabStyle);
            }
		}

		private void TryShowContextMenu(
			ActTreeEditorWindow2 window,
			ActTree2 tree,
			Node parent,
			Node overNode,
			Dictionary<Node, int> entryNodeOrder,
			int defaultEntryNodeID)
		{
			Event inputEvent = Event.current;
			if (inputEvent.type != EventType.MouseDown ||
				inputEvent.button != 1)
			{
				return;
			}

			if (overNode == null)
			{
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("Create Node    (ctrl+n)"), false, window.AddChildContext, (parent, inputEvent.mousePosition));
				if (NodeClipboard.CanPaste())
				{
					menu.AddItem(new GUIContent("Paste Node    (ctrl+v)"), false, window.PasteContext, (parent, inputEvent.mousePosition));
				}
				else
				{
					menu.AddDisabledItem(new GUIContent("Paste Node    (ctrl+v)"));
				}
				if (NodeClipboard.IsCopied(tree))
				{
					menu.AddItem(new GUIContent("Clear Copied    (esc)"), false, window.CopyClearContext);
				}
				menu.ShowAsContext();
				return;
			}

			GenericMenu nodeMenu = new GenericMenu();
			if (overNode.ID != parent.ID)
			{
				if (overNode.ID != defaultEntryNodeID)
				{
					if (entryNodeOrder.TryGetValue(overNode, out int entryOrder))
					{
						if (entryOrder > 1)
						{
							nodeMenu.AddItem(new GUIContent("Increase Entry Priority    (ctrl+up)"), false, window.MoveUpContext, overNode);
						}
						if (entryOrder < entryNodeOrder.Count)
						{
							nodeMenu.AddItem(new GUIContent("Decrease Entry Priority    (ctrl+down)"), false, window.MoveDownContext, overNode);
						}
						nodeMenu.AddSeparator("");
					}
					else
					{
						GUIContent makeDefaultContent = new GUIContent("Make Default Entry Node");
						if (overNode.CanBeDefaultEntryNode(tree))
						{
							nodeMenu.AddItem(makeDefaultContent, false, MakeDefaultEntryNodeContext, (window, overNode, defaultEntryNodeID));
						}
						else
						{
							nodeMenu.AddDisabledItem(makeDefaultContent);
						}
						nodeMenu.AddSeparator("");
					}
				}

				nodeMenu.AddItem(new GUIContent("Delete    (delete)"), false, window.DeleteNodeContext, overNode);
				nodeMenu.AddItem(new GUIContent("Duplicate    (ctrl+d)"), false, window.DuplicateContext, (overNode, inputEvent.mousePosition));
			}

			if (NodeClipboard.IsCopied(overNode, tree))
			{
				nodeMenu.AddItem(new GUIContent("Clear Copied    (esc)"), false, window.CopyClearContext);
			}
			else
			{
				nodeMenu.AddItem(new GUIContent("Copy    (ctrl+c)"), false, window.CopyContext, overNode);
			}
			nodeMenu.ShowAsContext();
		}

		private void MakeDefaultEntryNodeContext(object context)
		{
			var tuple = ((ActTreeEditorWindow2, Node, int))context;
			ActTreeEditorWindow2 window = tuple.Item1;
			Node node = tuple.Item2;
			int defaultNodeID = tuple.Item3;

			int nodeIndex = window.Tree.GetNodeIndex(node.ID);
			int defaultNodeIndex = window.Tree.GetNodeIndex(defaultNodeID);
			for (int i = nodeIndex - defaultNodeIndex; i > 0; i--)
			{
				window.MoveUp(node);
			}
		}
	}
}
