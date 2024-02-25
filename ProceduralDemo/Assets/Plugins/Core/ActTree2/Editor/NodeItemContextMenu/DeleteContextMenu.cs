using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Act2
{
	public class DeleteContextMenu : INodeItemContextMenu
	{
		private bool m_Delete = false;

		void INodeItemContextMenu.AddToMenu(GenericMenu menu, IActObject tree, IActNode node, object item, int index)
		{
			menu.AddItem(new GUIContent("Delete    (delete)"), false, DeleteContext);
		}

		bool INodeItemContextMenu.HandleEvent(IActObject tree, IActNode node, Event inputEvent)
		{
			if (inputEvent.keyCode == KeyCode.Delete)
			{
				DeleteContext();
				return true;
			}
			return false;
		}

		void INodeItemContextMenu.OnGUI(IActObject tree, ref SerializedObject sTree, IActNode node, ref SerializedProperty sNode)
		{
			if (m_Delete)
			{
				m_Delete = false;
				SerializedProperty sConditions = sNode.FindPropertyRelative("m_Conditions");
				SerializedProperty sTransitions = sNode.FindPropertyRelative("m_Transitions");
				SerializedProperty sTracks = sNode.FindPropertyRelative("m_Tracks");
				foreach ((NodeItemType, int) pair in SelectionDrawer.GetSelectedPairs(tree, node))
				{
					NodeItemType itemType = pair.Item1;
					int itemIndex = pair.Item2;
					// When an object reference is assigned first delete will set it none,
					// and the second will remove the array element
					SerializedProperty sItems;
					switch (itemType)
					{
						case NodeItemType.Condition:
							sItems = sConditions;
							break;
						case NodeItemType.Transition:
							sItems = sTransitions;
							break;
						case NodeItemType.Track:
							sItems = sTracks;
							break;
						default:
							continue;
					}
					int size = sItems.arraySize;
					sItems.DeleteArrayElementAtIndex(itemIndex);
					if (size == sItems.arraySize)
					{
						sItems.DeleteArrayElementAtIndex(itemIndex);
					}
					SelectionDrawer.OnDeleted(tree, node, itemType, itemIndex);
				}
			}
		}

		private void DeleteContext()
		{
			m_Delete = true;
		}
	}
}
