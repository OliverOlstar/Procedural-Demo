using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ActValue.Base), true)]
public class ActValueDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty value = property.FindPropertyRelative("m_Value");
		System.Type type = Core.SerializedReferenceEditorUtil.GetChildSerialRefType(value, fieldInfo);
		Core.SerializedReferenceEditorUtil.OnPropertyGUI(Core.SerializedRefGUIStyle.Header, position, label, value, type);
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		SerializedProperty value = property.FindPropertyRelative("m_Value");
		System.Type type = Core.SerializedReferenceEditorUtil.GetChildSerialRefType(value, fieldInfo);
		return Core.SerializedReferenceEditorUtil.GetPropertyHeight(Core.SerializedRefGUIStyle.Header, value, type);
	}
}
