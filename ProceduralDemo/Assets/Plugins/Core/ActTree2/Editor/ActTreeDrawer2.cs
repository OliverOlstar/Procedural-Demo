using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Act2
{
	public class TreeDrawer
	{
		private NodeDrawer m_ActNodeDrawer = new NodeDrawer();
		private ActTreeRuntimeDebugController m_RuntimeDebugging = new();
		public bool IsDebugging => m_RuntimeDebugging.IsDebugging;

		private Vector2 m_ScrollPos = Vector2.zero;

		public void OnGUI(ActTreeEditorWindow2 window)
		{
			m_RuntimeDebugging.Update(window.Tree);

			m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);
			NodeGUI(window, window.Tree, window.Tree.RootNode);
			EditorGUILayout.EndScrollView();

			CheckInput(window);
		}

		private Rect NodeGUI(
			ActTreeEditorWindow2 window,
			ActTree2 tree, 
			Node node,
			NodeDrawer.ParentsProperties parentState = NodeDrawer.ParentsProperties.None)
		{
			bool isCollapsed = m_ActNodeDrawer.OnGUILayout(
				window,
				this,
				node,
				parentState,
				out Node.Properties nodeProperties,
				out Rect nodeRect,
				out Rect controlRect);

			if (m_RuntimeDebugging.IsDebugging)
			{
				Rect sidebarRect = new Rect(controlRect);
				sidebarRect.xMax = sidebarRect.xMin + 17;
				float alpha = 0.15f;
				Rect highlightRect = new Rect(controlRect);
				highlightRect.xMin = sidebarRect.xMax;
				Rect markerRect = sidebarRect;
				markerRect.xMin = sidebarRect.xMax - 2;
				markerRect.yMin++;
				markerRect.yMax--;
				if (m_RuntimeDebugging.IsCurrentlyPlayingNode(node.ID))
				{
					EditorGUI.DrawRect(markerRect, new Color(0.2f, 0.9f, 0.2f, 1.0f));
					EditorGUI.DrawRect(highlightRect, new Color(0.0f, 1.0f, 0.0f, alpha));
				}
				else if (m_RuntimeDebugging.WasPreviouslyPlayingNode(node.ID))
				{
					EditorGUI.DrawRect(markerRect, new Color(0.0f, 0.4f, 0.1f, 1.0f));
					EditorGUI.DrawRect(highlightRect, new Color(0.0f, 0.4f, 0.1f, alpha));
				}
				if (m_RuntimeDebugging.WasTransitionTakenFromNode(node.ID))
				{
					bool toSelf = m_RuntimeDebugging.IsCurrentlyPlayingNode(node.ID);
					GUIStyle s = new GUIStyle(EditorStyles.boldLabel);
					s.normal.textColor = new Color(0.2f, 0.9f, 0.2f, 1.0f);
					GUI.Label(sidebarRect, toSelf ? NodeDrawer.TRANSITION_ARROW_LOOP : NodeDrawer.TRANSITION_ARROW_LEFT, s);
				}
			}

			if (!parentState.HasFlag(NodeDrawer.ParentsProperties.Reference) && GUI.Button(controlRect, string.Empty, GUIStyle.none))
			{
				if (Event.current.button == 0)
				{
					if (window.SelectedNodeID != node.GetID())
					{
						window.SetSelectedNode(node);
					}
					else
					{
						window.StartRenamingNode(node.GetID());
					}
				}
				else
				{
					window.OpenContext(node.GetID());
				}
			}
			if (isCollapsed)
			{
				return nodeRect;
			}

			EditorGUI.indentLevel++;

			if (nodeProperties.ReferencedNode != null)
			{
				parentState |= NodeDrawer.ParentsProperties.Reference;
				NodeGUI(window, tree, nodeProperties.ReferencedNode, parentState);
			}
			if (nodeProperties.HasFalseCondtion)
			{
				parentState |= NodeDrawer.ParentsProperties.False;
			}

			Dictionary<Node, Rect> childRects = new();
			foreach (Node child in tree.Nodes)
			{
				if (child.GetParentID() == node.GetID())
				{
					Rect childRect = NodeGUI(window, tree, child, parentState);
					childRects.Add(child, childRect);
				}
			}
			Node.GetEntryNodes(tree, childRects.Keys, out Dictionary<Node, int> entryNodeOrder, out _);
			Vector2 parentPos = new(nodeRect.xMin + 6, nodeRect.yMax);
			foreach (Node entryNode in entryNodeOrder.Keys)
			{
				Rect childRect = childRects[entryNode];
				Vector2 childPos = new(childRect.xMin - 2, childRect.center.y);
				Vector2 betweenPos = new(parentPos.x, childPos.y);
				Color color = new Color32(255, 255, 255, 50);
				GraphEditor.Lines.DrawLine(parentPos, betweenPos, color);
				GraphEditor.Lines.DrawLine(betweenPos, childPos, color);
				parentPos.y = childPos.y;
			}

			EditorGUI.indentLevel--;

			return nodeRect;
		}

		public void OnDeleted(ActTree2 tree, Node nodeToDelete)
		{
			EditorPrefs.DeleteKey(GetNodeCollapsedPrefKey(tree, nodeToDelete));
		}

		public static bool IsNodeCollapsed(ActTree2 tree, Node node)
		{
			return EditorPrefs.GetBool(GetNodeCollapsedPrefKey(tree, node), false);
		}

		private static string GetNodeCollapsedPrefKey(ActTree2 tree, Node node)
		{
			return "NodeCollapsed:" + AssetDatabase.GetAssetPath(tree) + "_" + node.GetID();
		}

		public static void SetNodeCollapsed(ActTree2 tree, Node node, bool collapsed, bool recursive = false)
		{
			if (collapsed)
			{
				EditorPrefs.SetBool(GetNodeCollapsedPrefKey(tree, node), collapsed);
			}
			else
			{
				EditorPrefs.DeleteKey(GetNodeCollapsedPrefKey(tree, node));
			}
			if (recursive)
			{
				foreach (Node child in tree.Nodes)
				{
					if (child.GetParentID() == node.GetID())
					{
						SetNodeCollapsed(tree, child, collapsed);
					}
				}
			}
		}

		public static void UncollapseParents(ActTree2 tree, Node node)
		{
			// Uncollapse parent nodes, it's weird to select a node that's hidden because it's parent is collapsed
			while (tree.TryGetNode(tree.GetParentID(node.GetID()), out node))
			{
				SetNodeCollapsed(tree, node, false);
			}
		}

		private void CheckInput(ActTreeEditorWindow2 window)
		{
			if (Event.current.type != EventType.KeyDown)
			{
				return;
			}
			if (Event.current.control)
			{
				return;
			}
			switch (Event.current.keyCode)
			{
				case KeyCode.UpArrow:
				case KeyCode.DownArrow:
				case KeyCode.RightArrow:
				case KeyCode.LeftArrow:
					if (window.SelectedNodeID == Node.INVALID_ID)
					{
						window.SetSelectedNode(Node.ROOT_ID);
					}
					break;
			}
			ActTree2 tree = window.Tree;
			switch (Event.current.keyCode)
			{
				case KeyCode.UpArrow:// step through siblings
					MoveToSibling(window, -1);
					break;
				case KeyCode.DownArrow: // step through siblings
					MoveToSibling(window, 1);
					break;
				case KeyCode.RightArrow: // step into leaf
					List<Node> children = tree.GetChildren(window.SelectedNodeID);
					if (children.Count > 0)
					{
						window.SetSelectedNode(children[0]);
					}
					else
					{
						MoveToSibling(window, 1);
					}
					break;
				case KeyCode.LeftArrow: // step up branch
					int parentID = tree.GetParentID(window.SelectedNodeID);
					if (parentID != tree.RootNode.GetID() && parentID != Node.INVALID_ID)
					{
						window.SetSelectedNode(parentID);
					}
					else
					{
						MoveToSibling(window, -1);
					}
					break;
			}
		}

		private void MoveToSibling(ActTreeEditorWindow2 window, int moveBy)
		{
			ActTree2 tree = window.Tree;
			List<Node> siblings = tree.GetSiblings(window.SelectedNodeID);
			if (siblings.Count > 0)
			{
				int index = siblings.FindIndex(x => x.GetID() == window.SelectedNodeID);
				index += moveBy;
				if (index >= siblings.Count)
				{
					index = 0;
				}
				else if (index < 0)
				{
					index = siblings.Count - 1;
				}
				window.SetSelectedNode(siblings[index]);
			}
		}
	}
}
