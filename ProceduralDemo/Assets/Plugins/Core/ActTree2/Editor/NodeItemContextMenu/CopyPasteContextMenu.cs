using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Act2
{
	public class CopyPasteContextMenu : INodeItemContextMenu
	{
		private bool m_Copy = false;
		private bool m_Paste = false;

		private bool CanCopy(IActObject tree, IActNode node) => 
			SelectionDrawer.IsPrimarySelection(tree, node, NodeItemType.Condition) || 
			SelectionDrawer.IsPrimarySelection(tree, node, NodeItemType.Track);

		private bool CanPaste() => SelectionDrawer.HasCopiedItems();

		void INodeItemContextMenu.AddToMenu(GenericMenu menu, IActObject tree, IActNode node, object item, int index)
		{
			if (CanCopy(tree, node))
			{
				menu.AddItem(new GUIContent("Copy    (ctrl+c)"), false, CopyContext);
			}
			if (CanPaste())
			{
				menu.AddItem(new GUIContent("Paste    (ctrl+v)"), false, PasteContext);
				menu.AddDisabledItem(new GUIContent("Clipboard:"));
				foreach (object copied in SelectionDrawer.GetCopyableItems(tree))
				{
					menu.AddDisabledItem(new GUIContent("    " + copied.ToString()));
				}
			}
		}

		bool INodeItemContextMenu.HandleEvent(IActObject tree, IActNode node, Event inputEvent)
		{
			if (inputEvent.keyCode == KeyCode.C && inputEvent.control && CanCopy(tree, node))
			{
				CopyContext();
				return true;
			}
			if (inputEvent.keyCode == KeyCode.V && inputEvent.control && CanPaste())
			{
				PasteContext();
				return true;
			}
			return false;
		}

		void INodeItemContextMenu.OnGUI(IActObject tree, ref SerializedObject sTree, IActNode node, ref SerializedProperty sNode)
		{
			if (m_Copy)
			{
				m_Copy = false;
				SelectionDrawer.Copy(tree, node);
			}
			if (m_Paste)
			{
				m_Paste = false;
				SerializedProperty sConditions = sNode.FindPropertyRelative("m_Conditions");
				SerializedProperty sTracks = sNode.FindPropertyRelative("m_Tracks");
				foreach (object item in SelectionDrawer.GetCopyableItems(tree))
				{
					switch (item)
					{
						case Condition condition:
							sConditions.arraySize++;
							SerializedProperty sCondition = sConditions.GetArrayElementAtIndex(sConditions.arraySize - 1);
							sCondition.managedReferenceValue = CopySerializedReferenceUtil.CopyObject(condition);
							break;
						case Track track:
							sTracks.arraySize++;
							SerializedProperty sTrack = sTracks.GetArrayElementAtIndex(sTracks.arraySize - 1);
							sTrack.managedReferenceValue = CopySerializedReferenceUtil.CopyObject(track);
							break;
					}
				}
			}
		}

		private void CopyContext()
		{
			m_Copy = true;
		}

		private void PasteContext()
		{
			m_Paste = true;
		}
	}
}
