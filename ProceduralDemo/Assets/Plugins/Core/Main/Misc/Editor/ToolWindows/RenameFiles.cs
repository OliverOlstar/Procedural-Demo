
using UnityEngine;
using UnityEditor;

namespace Core
{
	public class RenameFiles : EditorWindow
	{
		Object m_Selected = null;
		string m_OldName = string.Empty;
		string m_NewName = string.Empty;

		[MenuItem("Core/File/Rename")]
		static void CreateWizard()
		{
			RenameFiles window = GetWindow<RenameFiles>("Rename Files");
			window.Show();
		}

		void OnSelectionChange()
		{
			if (Selection.activeObject != m_Selected)
			{
				m_Selected = Selection.activeObject;
				if (m_Selected != null)
				{
					m_OldName = m_Selected.name;
					m_NewName = m_Selected.name;
				}
			}
			Repaint();
		}

		void OnGUI()
		{
			GUILayout.Label("Old Name");
			m_OldName = GUILayout.TextArea(m_OldName);
			GUILayout.Label("New Name");
			m_NewName = GUILayout.TextArea(m_NewName);
			if (GUILayout.Button("Rename"))
			{
				string[] assets = AssetDatabase.FindAssets(m_OldName);
				foreach (string s in assets)
				{
					string path = AssetDatabase.GUIDToAssetPath(s);
					string newPath = path.Replace(m_OldName, m_NewName);
					AssetDatabase.MoveAsset(path, newPath);
				}
			}
		}
	}
}
