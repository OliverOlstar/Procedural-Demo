using UnityEditor;
using UnityEngine;

namespace Data
{
	[CustomEditor(typeof(DataDocumentationSettings))]
	public class DataDocumentationSettingsEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Open Settings"))
			{
				SettingsService.OpenProjectSettings(DataDocumentationSettingsProvider.Path);
			}
		}
	}
}
