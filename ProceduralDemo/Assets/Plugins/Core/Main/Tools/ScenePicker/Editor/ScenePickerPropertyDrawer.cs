using UnityEditor;
using UnityEngine;

namespace Core
{
	[CustomPropertyDrawer((typeof(ScenePicker)), true)]
	public class ScenePickerPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			SerializedProperty scene = prop.FindPropertyRelative("m_SceneName");
			SerializedProperty scenePath = prop.FindPropertyRelative("m_ScenePath");
			SceneAsset currentScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath.stringValue);
			
			prop.serializedObject.Update();
			Object selection = EditorGUI.ObjectField(position, prop.displayName, currentScene, typeof(SceneAsset), false);

			if (selection is SceneAsset)
			{
				scenePath.stringValue = AssetDatabase.GetAssetPath(selection);
				scene.stringValue = (selection as SceneAsset).name;
			}
			
			
			prop.serializedObject.ApplyModifiedProperties();
		}
	}
}