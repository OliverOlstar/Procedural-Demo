
using UnityEngine;
using UnityEditor;
using System.IO;

public class BundleTools : EditorWindow
{
	[MenuItem("Window/Debug/Bundle Tools", false, 9999)]
	static void CreateWizard()
	{
		BundleTools window = GetWindow<BundleTools>("Bundle Tools");
		window.Show();
	}

	int m_Index = 0;
	Vector2 m_Scroll = Vector2.zero;

	void OnGUI()
	{
		string[] names = AssetDatabase.GetAllAssetBundleNames();

		EditorGUILayout.LabelField("Bundle");

		EditorGUILayout.BeginHorizontal();
		m_Index = EditorGUILayout.Popup(m_Index, names);
		if (m_Index < names.Length && GUILayout.Button("X", GUILayout.ExpandWidth(false)))
		{
			AssetDatabase.RemoveAssetBundleName(names[m_Index], true);
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.LabelField("Contents");
		m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
		if (m_Index < names.Length)
		{
			string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(names[m_Index]);
			foreach (string asset in assets)
			{
				if (GUILayout.Button(Path.GetFileName(asset), GUI.skin.label))
				{
					Selection.activeObject= AssetDatabase.LoadAssetAtPath(asset, typeof(Object));
				}
			}
		}
		EditorGUILayout.EndScrollView();
	}
}

