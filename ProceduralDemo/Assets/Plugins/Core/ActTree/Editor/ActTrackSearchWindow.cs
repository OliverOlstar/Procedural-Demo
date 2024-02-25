
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ActTrackSearchWindow : EditorWindow
{
	private string m_Path = string.Empty;
	private ActTree m_Tree = null;
	private System.Type m_TrackType = null;
	private Vector2 m_Scroll = Vector2.zero;

	[MenuItem("Window/Act Tree/Act Track Search")]
	static void Init()
	{
		ActTrackSearchWindow window = EditorWindow.GetWindow<ActTrackSearchWindow>("Track Search");
		window.Show();
	}

	private void OnGUI()
	{
		ActTreeEditorWindow.SelectActTree(OnActTreeSelected);
		if (m_Tree == null)
		{
			return;
		}
		EditorGUILayout.ObjectField(m_Tree, typeof(ActTree), false);
		List<System.Type> types = new List<System.Type>();
		List<string> names = new List<string>();
		names.Add("Select...");
		foreach (System.Type type in Core.TypeUtility.GetTypesDerivedFrom(typeof(ActTrack)))
		{
			if (type.IsGenericTypeDefinition || type.IsAbstract)
			{
				continue;
			}
			types.Add(type);
			names.Add(type.Name);
		}

		int index = EditorGUILayout.Popup(0, names.ToArray());
		if (index > 0)
		{
			m_TrackType = types[index - 1];
		}
		if (m_TrackType == null)
		{
			return;
		}
		m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
		bool found = false;
		foreach (ActTrack track in m_Tree.GetTracks())
		{
			if (track.GetType() != m_TrackType)
			{
				continue;
			}
			found = true;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.ObjectField(track, typeof(ActTrack), false);
			if (m_Tree.GetNode(track.GetNodeID()) == null)
			{
				GUILayout.Label("Invalid");
			}
			else if (GUILayout.Button("Select"))
			{
				ActTreeEditorWindow treeWindow = ActTreeEditorWindow.Get();
				treeWindow.SetSelectedNode(track.GetNodeID());
				treeWindow.Focus();
			}
			EditorGUILayout.EndHorizontal();
		}
		if (!found)
		{
			GUILayout.Label(m_TrackType.Name + " not found");
		}
		EditorGUILayout.EndScrollView();
	}

	private void OnActTreeSelected(string actTree)
	{
		if (!string.IsNullOrEmpty(actTree) && !string.Equals(actTree, m_Path))
		{
			m_Tree = AssetDatabase.LoadAssetAtPath<ActTree>(actTree);
		}
	}
}
