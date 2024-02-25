using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Act2
{
	public class DuplicateContextMenu : INodeItemContextMenu
	{
		private bool m_Duplicate = false;

		void INodeItemContextMenu.AddToMenu(GenericMenu menu, IActObject tree, IActNode node, object item, int index)
		{
			menu.AddItem(new GUIContent("Duplicate    (ctrl+d)"), false, DuplicateContext);
		}

		bool INodeItemContextMenu.HandleEvent(IActObject tree, IActNode node, Event inputEvent)
		{
			if (inputEvent.keyCode == KeyCode.D && inputEvent.control)
			{
				DuplicateContext();
				return true;
			}
			return false;
		}

		void INodeItemContextMenu.OnGUI(IActObject tree, ref SerializedObject sTree, IActNode node, ref SerializedProperty sNode)
		{
			if (m_Duplicate)
			{
				m_Duplicate = false;
				SerializedProperty sConditions = sNode.FindPropertyRelative("m_Conditions");
				SerializedProperty sTracks = sNode.FindPropertyRelative("m_Tracks");
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
						case NodeItemType.Track:
							sItems = sTracks;
							break;
						default:
							continue;
					}
					int newIndex = itemIndex + 1;
					sItems.InsertArrayElementAtIndex(newIndex);
					SerializedProperty p1 = sItems.GetArrayElementAtIndex(itemIndex);
					SerializedProperty p2 = sItems.GetArrayElementAtIndex(newIndex);
					switch (itemType)
					{
						case NodeItemType.Condition:
							p2.managedReferenceValue = CopySerializedReferenceUtil.CopyObject(p1.managedReferenceValue);
							break;
						case NodeItemType.Track:
							p2.managedReferenceValue = CopySerializedReferenceUtil.CopyObject(p1.managedReferenceValue);
							break;
					}
					SelectionDrawer.OnInsert(tree, node, itemType, newIndex);
				}
			}
		}

		private void DuplicateContext()
		{
			m_Duplicate = true;
		}
	}
}
