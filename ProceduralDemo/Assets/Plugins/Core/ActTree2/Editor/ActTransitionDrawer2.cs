using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Act2
{
	public class TransitionDrawer
	{
		private NodeItemContextMenu m_ContextMenu = new NodeItemContextMenu(
			new DeleteContextMenu(),
			new MoveContextMenu());

		public void OnGUI(ActTree2 tree, ref SerializedObject sTree, Node node, ref SerializedProperty sNode)
		{
			m_ContextMenu.OnGUI(tree, ref sTree, node, ref sNode);
			if (node.Transitions.Count == 0)
			{
				return;
			}
			Rect transHeaderRect = EditorGUILayout.GetControlRect();
			TrackDrawer.DrawItemLabel(transHeaderRect, SelectionDrawer.SelectionState.None, out GUIStyle transHeaderStyle, out Rect oppHeaderLabelRect, out Rect oppHeaderContentRect);
			transHeaderStyle.fontStyle = FontStyle.Bold;
			transHeaderStyle.alignment = TextAnchor.MiddleLeft;
			EditorGUI.LabelField(oppHeaderLabelRect, "Transitions", transHeaderStyle);
			TimedItemProperties timingProperties = TimedItemUtil.GetTimedItemProperties(node.GetAllTimedItems());
			for (int i = 0; i < node.Transitions.Count; i++)
			{
				NodeTransition trans = node.Transitions[i];
				DrawTransition(
					tree,
					node,
					trans,
					i,
					timingProperties,
					out bool rightClicked);
				if (rightClicked)
				{
					m_ContextMenu.Show(tree, node, trans, i);
				}
				if (SelectionDrawer.IsPrimarySelection(tree, node, NodeItemType.Transition, i))
				{
					SelectionDrawer.OnGUI(tree, node, ref sNode, out _);
				}
			}
		}

		public static bool DrawTransition(
			ActTree2 tree,
			IActNode node,
			NodeTransition transition,
			int index,
			TimedItemProperties timingProperties,
			out bool rightClicked)
		{
			if (!tree.TryGetNode(transition.GetToID(), out Node sib))
			{
				rightClicked = false;
				return false; // Can't draw a sequence without a valid sibling reference
			}
			TrackDrawer.DrawTimedItem(
				tree,
				node,
				transition,
				NodeItemType.Transition,
				index,
				timingProperties,
				true,
				out GUIStyle labelStyle,
				out Rect labelRect,
				out rightClicked);
			NodeDrawer.AttachNodeIcon(labelRect, out labelRect, labelStyle, tree, sib);
			EditorGUI.LabelField(labelRect, sib.Name, labelStyle);
			return true;
		}
	}
}
