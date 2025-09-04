using UnityEngine;
using UnityEditor;

namespace ODev.Updateables.Editor
{
	public class UpdateablesDebugWindow : EditorWindow
	{
		private static Vector2 _scollPosition = Vector2.zero;

		[MenuItem("ODev/Debug/Updatables")]
		private static void ShowWindow()
		{
			UpdateablesDebugWindow window = GetWindow<UpdateablesDebugWindow>();
			window.titleContent = new GUIContent("Registered Updateables");
			window.Show();
		}

		private void OnGUI()
		{
			if (UpdateableManager.Instance == null)
			{
				GUILayout.Label("No UpdateableManager found, it should get created when you enter playmode!");
				return;
			}

			UpdateableType? currType = null;
			_scollPosition = GUILayout.BeginScrollView(_scollPosition);
			foreach (Updateable updateable in UpdateableManager.Instance.EditorGetAllUpdateables())
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
				GUILayout.Label($"({(int)updateable.Priority})\t{updateable}");
				if (updateable.IntervalSeconds > 0.0f)
				{
					GUILayout.FlexibleSpace();
					GUILayout.Label($"deltaTime:{updateable.TimeSinceLastUpdate}");
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
		}
		
		private void Update()
		{
			if (Application.isPlaying)
			{
				Repaint();
			}
		}
	}
}
