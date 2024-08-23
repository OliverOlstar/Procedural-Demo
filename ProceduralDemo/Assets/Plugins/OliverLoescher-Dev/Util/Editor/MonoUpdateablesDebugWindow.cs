using UnityEngine;
using UnityEditor;

namespace ODev.Util
{
	public class MonoUpdateablesDebugWindow : EditorWindow
	{
		[MenuItem("Window/Debug/Mono Updatables")]
		private static void ShowWindow()
		{
			MonoUpdateablesDebugWindow window = GetWindow<MonoUpdateablesDebugWindow>();
			window.titleContent = new GUIContent("Mono Updateables");
			window.Show();
		}

		private void OnGUI()
		{
			Mono.Type? currType = null;
			foreach (Mono.Updateable updateable in Mono.GetAllUpdateables())
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
				GUILayout.Label($"({(int)updateable.Priority})\t{updateable.Action.Target}");
			}
		}
	}
}
