
using UnityEngine;
using UnityEditor;

namespace Core
{
	public class JSONViewer : EditorWindow
	{
		string m_JSON = string.Empty;
		Vector2 m_ScrollPos = Vector3.zero;

		[MenuItem("Core/JSON Viewer")]
		static void CreateWizard()
		{
			JSONViewer window = EditorWindow.GetWindow<JSONViewer>("JSON Viewer");
			window.Show();
		}

		void OnGUI()
		{
			//GUILayout.BeginVertical();
			if (GUILayout.Button("Format"))
			{
				GUI.FocusControl(Core.Str.EMPTY); // Having the text box in focus brings the old text back
				m_JSON = m_JSON.Replace("{", "\n{\n");
				m_JSON = m_JSON.Replace("}", "\n}\n");
			}
			else
			{
				m_ScrollPos = GUILayout.BeginScrollView(m_ScrollPos, true, true);
				//m_JSON = EditorGUILayout.TextArea(m_JSON, GUILayout.ExpandHeight(true), GUILayout.MinHeight(100));
				m_JSON = EditorGUILayout.TextArea(m_JSON);
				GUILayout.EndScrollView();
			}
			//GUILayout.EndVertical();
		}
	}
}
