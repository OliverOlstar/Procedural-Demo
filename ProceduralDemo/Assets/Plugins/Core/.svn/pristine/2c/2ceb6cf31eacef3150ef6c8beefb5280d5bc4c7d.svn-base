using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Act2
{
	public interface INodeItemContextMenu
	{
		void AddToMenu(GenericMenu menu, IActObject tree, IActNode node, object item, int index);
		bool HandleEvent(IActObject tree, IActNode node, Event inputEvent);
		void OnGUI(IActObject tree, ref SerializedObject sTree, IActNode node, ref SerializedProperty sNode);
	}

	public class NodeItemContextMenu
	{
		private List<INodeItemContextMenu> m_MenuItems = new List<INodeItemContextMenu>();

		private GenericMenu m_Menu = null;

		public NodeItemContextMenu(params INodeItemContextMenu[] menuItems)
		{
			m_MenuItems.AddRange(menuItems);
		}

		// Note: Want to call this as early as possible when drawing UI, showing a context menu seems to block gui code from executing
		// so we want to resolve the context menu as early as possible and draw the results
		public void OnGUI(IActObject tree, ref SerializedObject sTree, IActNode node, ref SerializedProperty sNode)
		{
			if (m_Menu != null)
			{
				m_Menu.ShowAsContext();
				m_Menu = null;
			}
			foreach (INodeItemContextMenu menu in m_MenuItems)
			{
				menu.OnGUI(tree, ref sTree, node, ref sNode);
			}
		}

		public void Show(IActObject tree, IActNode node, object item = null, int index = -1)
		{
			// Defer showing the menu until next update, this lets the GUI complete drawing before the context menu blocks it from updating
			m_Menu = new GenericMenu();
			m_Menu.allowDuplicateNames = true;
			for (int i = 0; i < m_MenuItems.Count; i++)
			{
				m_MenuItems[i].AddToMenu(m_Menu, tree, node, item, index);
				if (i + 1 < m_MenuItems.Count)
				{
					m_Menu.AddSeparator("");
				}
			}
		}
	}

	public class NodeItemHotKeys
	{
		private List<INodeItemContextMenu> m_Items = new List<INodeItemContextMenu>();

		public NodeItemHotKeys()
		{
			foreach (System.Type type in Core.TypeUtility.GetTypesImplementingInterface(typeof(INodeItemContextMenu)))
			{
				m_Items.Add(System.Activator.CreateInstance(type) as INodeItemContextMenu);
			}
		}

		public NodeItemHotKeys(params INodeItemContextMenu[] menuItems)
		{
			m_Items.AddRange(menuItems);
		}

		public void OnGUI(IActObject tree, ref SerializedObject sTree, IActNode node, ref SerializedProperty sNode, out bool handledHotkey)
		{
			handledHotkey = false;
			if (Event.current.type == EventType.KeyDown)
			{
				foreach (INodeItemContextMenu item in m_Items)
				{
					if (item.HandleEvent(tree, node, Event.current))
					{
						handledHotkey = true;
					}
				}
			}
			foreach (INodeItemContextMenu menu in m_Items)
			{
				menu.OnGUI(tree, ref sTree, node, ref sNode);
			}
		}
	}
}
