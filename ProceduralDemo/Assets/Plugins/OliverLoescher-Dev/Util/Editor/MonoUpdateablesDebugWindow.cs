using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

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
			Mono.Type currType = Mono.Type.Early;
			GUILayout.Label(currType.ToString());
			foreach (Mono.Updateable updateable in Mono.GetAllUpdateables())
			{
				if (updateable.Type != currType)
				{
					currType = updateable.Type;
					GUILayout.Space(16.0f);
					GUILayout.Label(currType.ToString());
				}
				GUILayout.Label($"({(int)updateable.Priority})\t{updateable.Action.Target}");
			}
		}
	}
}
