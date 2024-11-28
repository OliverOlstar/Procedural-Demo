using UnityEngine;
using UnityEditor;

namespace ODev.Update.Editor
{
	public class MonoUpdateablesDebugWindow : EditorWindow
	{
		private static Vector2 m_ScollPosition = Vector2.zero;

		[MenuItem("Window/Debug/Updatables")]
		private static void ShowWindow()
		{
			MonoUpdateablesDebugWindow window = GetWindow<MonoUpdateablesDebugWindow>();
			window.titleContent = new GUIContent("Mono Updateables");
			window.Show();
		}

		private void OnGUI()
		{
			Type? currType = null;
			m_ScollPosition = GUILayout.BeginScrollView(m_ScollPosition);
			foreach (Updateable updateable in UpdateManager.GetAllUpdateables())
			{
				if (updateable.Type != currType)
				{
					if (currType.HasValue)
					{
						GUILayout.Space(16.0f);
					}
					currType = updateable.Type;
					GUILayout.Label(currType.ToString());
				}

				GUILayout.BeginHorizontal();
				GUILayout.Label($"({(int)updateable.Priority})\t{updateable.Action.Target}");
				if (updateable.IntervalSeconds > 0.0f)
				{
					GUILayout.FlexibleSpace();
					GUILayout.Label($"timeElapsed-{updateable.TimeSinceLastUpdate}");
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
		}
	}
}
