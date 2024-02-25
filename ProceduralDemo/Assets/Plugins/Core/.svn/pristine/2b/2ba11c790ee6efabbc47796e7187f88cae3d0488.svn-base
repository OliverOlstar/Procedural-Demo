using System.IO;
using Data;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DBBase), true)]
public class DatabaseCustomEditor : Editor
{
	public override void OnInspectorGUI()
	{
		TryDisplaySourceButton();
		base.OnInspectorGUI();
	}

	private void TryDisplaySourceButton()
	{
		if (target is DBBase)
		{
			if (!target.GetType().TryGetCustomAttribute(out DatabaseSourceAttribute attribute) ||
				!StaticDataConfig.TryGet(out StaticDataConfig dataConfig))
			{
				return;
			}

			string filePath = Path.Combine(dataConfig.GetRootPath(StaticDataConfig.RootFolder.Source), attribute.FileName);
			if (!File.Exists(filePath))
			{
				EditorGUILayout.HelpBox($"The file '{attribute.FileName}' does not exist in the data source folder '{dataConfig.GetRootPath(StaticDataConfig.RootFolder.Source)}/'.\n" +
					$"Please fix the file name parameter in the {nameof(DatabaseSourceAttribute)} on this DB.", MessageType.Error);
				return;
			}

			if (GUILayout.Button($"Open Source ({attribute.FileName})"))
			{
				Object file = AssetDatabase.LoadAssetAtPath<Object>(filePath);
				if (file != null)
				{
					AssetDatabase.OpenAsset(file);
				}
			}
		}
	}
}