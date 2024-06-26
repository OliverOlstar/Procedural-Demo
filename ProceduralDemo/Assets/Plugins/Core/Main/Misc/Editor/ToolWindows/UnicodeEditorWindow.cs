using UnityEditor;
using UnityEngine;

public class UnicodeEditorWindow : EditorWindow
{
	[MenuItem("Core/Unicode")]
	public static UnicodeEditorWindow Get()
	{
		UnicodeEditorWindow window = GetWindow<UnicodeEditorWindow>("Unicode");
		window.Show();
		return window;
	}

	private static readonly int START = 0;
	private static readonly int END = 0xFFFF;
	private static readonly int ITEMS_PER_PAGE = 90;

	private Vector2 m_ScrollPos = Vector2.zero;

	private int m_Page = 1;

	private void OnGUI()
	{
		EditorGUILayout.BeginHorizontal();
		//EditorGUILayout.LabelField("Page " + m_Page);
		m_Page = EditorGUILayout.IntSlider("Page", m_Page, 0, (END - START) / ITEMS_PER_PAGE);
		if (GUILayout.Button("Previous"))
		{
			m_Page--;
		}
		if (GUILayout.Button("Next"))
		{
			m_Page++;
		}
		EditorGUILayout.EndHorizontal();

		GUIStyle style = new(GUI.skin.label);
		style.fontSize = 20;
		m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

		int start = START + (m_Page - 1) * ITEMS_PER_PAGE;
		int end = START + m_Page * ITEMS_PER_PAGE;
		int dif = end - start;
		int columns = 3;
		int countPerColumn = Mathf.CeilToInt((float)dif / columns);

		EditorGUILayout.BeginHorizontal();
		for (int i = 0; i < columns; i++)
		{
			EditorGUILayout.BeginVertical();
			int columnStart = start + i * countPerColumn;
			int columnEnd = start + (i + 1) * countPerColumn;
			for (int j = columnStart; j < columnEnd; j++)
			{
				EditorGUILayout.BeginHorizontal();
				string hex = j.ToString("x");
				string uni =
					j >= 0xD800 && j <= 0xDFFF ? string.Empty : // I guess these are invalid
					char.ConvertFromUtf32(j);
				EditorGUILayout.LabelField(uni, style, GUILayout.Width(32.0f), GUILayout.Height(32.0f));
				EditorGUILayout.LabelField(hex, style);
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndScrollView();
	}
}
