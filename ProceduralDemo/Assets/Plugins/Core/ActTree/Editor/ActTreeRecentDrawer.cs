
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ActTreeRecentDrawer
{
	private static readonly string[] LAST_TREES_KES = new string[]
	{
		"ActTreeEditorWindowLastTree0",
		"ActTreeEditorWindowLastTree1",
		"ActTreeEditorWindowLastTree2",
		"ActTreeEditorWindowLastTree3",
	};

	List<ActTree> m_History = new List<ActTree>();

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
			if (obj is ActTree tree)
			{
				m_History.Add(tree);
			}
		}
	}

	public void AddToHistory(ActTree tree)
	{
		m_History.Insert(0, tree);
		for (int i = 1; i < m_History.Count; i++)
		{
			ActTree t = m_History[i];
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

	public bool HistoryGUI(ActTree currentTree, out string selectedPath)
	{
		selectedPath = null;
		if (m_History.Count <= 1)
		{
			return false;
		}
		int selected = -1;
		string[] names = new string[m_History.Count];
		for (int i = 0; i < m_History.Count; i++)
		{
			ActTree tree = m_History[i];
			if (tree == null)
			{
				names[i] = "Missing";
				continue;
			}
			names[i] = tree.name.Length > 12 ? tree.name.Substring(0, 12) + "..." : tree.name;
			if (currentTree != null && currentTree.GetInstanceID() == tree.GetInstanceID())
			{
				selected = i;
			}
		}
		if (names.Length <= 1)
		{
			return false;
		}
		int newSelected = GUILayout.Toolbar(selected, names);
		if (newSelected == selected || m_History[newSelected] == null)
		{
			return false;
		}
		selectedPath = AssetDatabase.GetAssetPath(m_History[newSelected]);
		return true;
	}

	public bool NextHistory(out string selectedPath)
	{
		selectedPath = null;
		if (m_History.Count < 2)
		{
			return false;
		}
		ActTree first = m_History[0];
		m_History.RemoveAt(0);
		if (first != null)
		{
			m_History.Add(first);
		}
		selectedPath = m_History[0] != null ? AssetDatabase.GetAssetPath(m_History[0]) : null;
		return selectedPath != null;
	}

	public bool PrevHistory(out string selectedPath)
	{
		selectedPath = null;
		if (m_History.Count < 2)
		{
			return false;
		}
		ActTree last = m_History[m_History.Count - 1];
		m_History.RemoveAt(m_History.Count - 1);
		if (last != null)
		{
			m_History.Insert(0, last);
		}
		selectedPath = m_History[0] != null ? AssetDatabase.GetAssetPath(m_History[0]) : null;
		return selectedPath != null;
	}
}
