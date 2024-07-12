#if UNITY_2021_1_OR_NEWER
using UnityEditor;
using UnityEngine;

namespace ODev
{
	[CustomPropertyDrawer(typeof(SerializedReferenceDrawerAttribute))]
	public class SerializedReferenceDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedReferenceDrawerAttribute att = attribute as SerializedReferenceDrawerAttribute;
			System.Type type = PropertyDrawerUtil.GetUnderlyingType(fieldInfo);
			SerializedReferenceEditorUtil.OnPropertyGUI(att.Style, position, label, property, type, att.NullEntryName, null);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			SerializedReferenceDrawerAttribute att = attribute as SerializedReferenceDrawerAttribute;
			System.Type type = PropertyDrawerUtil.GetUnderlyingType(fieldInfo);
			return SerializedReferenceEditorUtil.GetPropertyHeight(att.Style, property, type, att.NullEntryName, null);
		}
	}
}
#endif