using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Act2
{
	public class MoveContextMenu : INodeItemContextMenu
	{
		private bool m_MoveUp = false;
		private bool m_MoveDown = false;

		void INodeItemContextMenu.AddToMenu(GenericMenu menu, IActObject tree, IActNode node, object item, int index)
		{
			menu.AddItem(new GUIContent("Move Up    (ctrl+up)"), false, MoveUpContext);
			menu.AddItem(new GUIContent("Move Down    (ctrl+down)"), false, MoveDownContext);
		}

		bool INodeItemContextMenu.HandleEvent(IActObject tree, IActNode node, Event inputEvent)
		{
			if (inputEvent.keyCode == KeyCode.UpArrow && inputEvent.control)
			{
				MoveUpContext();
				return true;
			}
			if (inputEvent.keyCode == KeyCode.DownArrow && inputEvent.control)
			{
				MoveDownContext();
				return true;
			}
			return false;
		}

		void INodeItemContextMenu.OnGUI(IActObject tree, ref SerializedObject sTree, IActNode node, ref SerializedProperty sNode)
		{
			if (m_MoveUp)
			{
				m_MoveUp = false;
				Move(tree, ref sTree, node, ref sNode, -1);
			}
			if (m_MoveDown)
			{
				m_MoveDown = false;
				Move(tree, ref sTree, node, ref sNode, 1);
			}
		}

		private void Move(IActObject tree, ref SerializedObject sTree, IActNode node, ref SerializedProperty sNode, int delta)
		{
			SerializedProperty sConditions = sNode.FindPropertyRelative("m_Conditions");
			SerializedProperty sTransitions = sNode.FindPropertyRelative("m_Transitions");
			foreach ((NodeItemType, int) pair in SelectionDrawer.GetSelectedPairs(tree, node))
			{
				NodeItemType itemType = pair.Item1;
				int itemIndex = pair.Item2;
				SerializedProperty sItems;
				switch (itemType)
				{
					case NodeItemType.Condition:
						sItems = sConditions;
						break;
					case NodeItemType.Transition:
						sItems = sTransitions;
						break;
					default:
						continue;
				}
				int newIndex = (sItems.arraySize + itemIndex + delta) % sItems.arraySize;
				sItems.MoveArrayElement(itemIndex, newIndex);
				SelectionDrawer.SetSelectedIndex(tree, node, itemType, itemIndex, newIndex);
			}
		}

		private void MoveUpContext()
		{
			m_MoveUp = true;
		}
		private void MoveDownContext()
		{
			m_MoveDown = true;
		}
	}
}
