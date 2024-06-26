using UnityEngine;
using UnityEditor;

namespace ModelLocator
{
	[CustomEditor(typeof(DummyModelLocator), true)]
	public class DummyModelLocatorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DummyModelLocator dummy = (DummyModelLocator)target;
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Save", GUILayout.Height(2.0f * EditorGUIUtility.singleLineHeight)))
			{
				ModelLocatorEditor.TrySave(dummy);
				Selection.activeObject = dummy.Source;
				EditorGUIUtility.PingObject(dummy);
			}
			if (GUILayout.Button("Cancel", GUILayout.Height(2.0f * EditorGUIUtility.singleLineHeight)))
			{
				Selection.activeObject = dummy.Source;
				EditorGUIUtility.PingObject(dummy);
				DestroyImmediate(dummy.gameObject);
			}
			GUILayout.EndHorizontal();
		}
	}
}
