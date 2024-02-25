
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Act2
{
	public class TreeRecentDrawer
	{
		private static readonly string[] LAST_TREES_KES = new string[]
		{
		"ActTreeEditorWindowLastTree0",
		"ActTreeEditorWindowLastTree1",
		"ActTreeEditorWindowLastTree2",
		"ActTreeEditorWindowLastTree3",
		};

		List<ActTree2> m_History = new List<ActTree2>();

		public void InitHistory()
		{
			m_History.Clear();
			foreach (string key in LAST_TREES_KES)
			{
				string path = EditorPrefs.GetString(key, string.Empty);
				if (string.IsNullOrEmpty(path))
				{
					continue;
				}
				UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath(path);
				if (obj is ActTree2 tree)
				{
					m_History.Add(tree);
				}
			}
		}

		public void AddToHistory(ActTree2 tree)
		{
			m_History.Insert(0, tree);
			for (int i = 1; i < m_History.Count; i++)
			{
				ActTree2 t = m_History[i];
				if (tree == null)
				{
					continue;
				}
				if (tree != null && tree.GetInstanceID() == t.GetInstanceID())
				{
					m_History.RemoveAt(i);
					break;
				}
			}
			if (m_History.Count > LAST_TREES_KES.Length)
			{
				m_History.RemoveAt(m_History.Count - 1);
			}
			for (int i = 0; i < m_History.Count; i++)
			{
				EditorPrefs.SetString(LAST_TREES_KES[i], AssetDatabase.GetAssetPath(m_History[i]));
			}
		}

		public bool HistoryGUI(ActTree2 currentTree, out ActTree2 selectedTree)
		{
			selectedTree = null;
			if (m_History.Count <= 1)
			{
				return false;
			}
			int selected = -1;
			GUIContent[] names = new GUIContent[m_History.Count];
			for (int i = 0; i < m_History.Count; i++)
			{
				ActTree2 tree = m_History[i];
				if (tree == null)
				{
					names[i] = new GUIContent("Missing");
					continue;
				}
				names[i] = new GUIContent(tree.name.Length > 16 ? tree.name.Substring(0, 16) + "..." : tree.name, tree.name);
				if (currentTree != null && currentTree.GetInstanceID() == tree.GetInstanceID())
				{
					selected = i;
				}
			}
			if (names.Length <= 1)
			{
				return false;
			}
			int fontSize = GUI.skin.button.fontSize;
			GUI.skin.GetStyle("ButtonMid").fontSize = 10;
			GUI.skin.GetStyle("ButtonRight").fontSize = 10;
			GUI.skin.GetStyle("ButtonLeft").fontSize = 10;
			int newSelected = GUILayout.Toolbar(selected, names, GUI.skin.GetStyle("Button"), GUI.ToolbarButtonSize.Fixed); ;
			GUI.skin.GetStyle("ButtonMid").fontSize = fontSize;
			GUI.skin.GetStyle("ButtonRight").fontSize = fontSize;
			GUI.skin.GetStyle("ButtonLeft").fontSize = fontSize;
			if (newSelected == selected)
			{
				return false;
			}
			selectedTree = m_History[newSelected];
			return selectedTree != null;
		}

		public bool NextHistory(out ActTree2 selectedTree)
		{
			selectedTree = null;
			if (m_History.Count < 2)
			{
				return false;
			}
			ActTree2 first = m_History[0];
			m_History.RemoveAt(0);
			if (first != null)
			{
				m_History.Add(first);
			}
			selectedTree = m_History[0];
			return selectedTree != null;
		}

		public bool PrevHistory(out ActTree2 selectedTree)
		{
			selectedTree = null;
			if (m_History.Count < 2)
			{
				return false;
			}
			ActTree2 last = m_History[m_History.Count - 1];
			m_History.RemoveAt(m_History.Count - 1);
			if (last != null)
			{
				m_History.Insert(0, last);
			}
			selectedTree = m_History[0];
			return selectedTree != null;
		}
	}
}
