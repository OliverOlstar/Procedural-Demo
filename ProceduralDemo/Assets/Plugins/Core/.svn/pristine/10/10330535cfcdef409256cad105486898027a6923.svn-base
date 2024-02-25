using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Act2
{
	[CustomPropertyDrawer(typeof(NodeEventType), true)]
	public class ActValueDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			System.Type type = Core.PropertyDrawerUtil.GetUnderlyingType(fieldInfo);
			Core.SerializedReferenceEditorUtil.OnPropertyGUI(
				Core.SerializedRefGUIStyle.Flat, 
				position, 
				label, 
				property,
				type, 
				"None",
				NodeEventType.SuffixToRemove);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			System.Type type = Core.PropertyDrawerUtil.GetUnderlyingType(fieldInfo);
			return Core.SerializedReferenceEditorUtil.GetPropertyHeight(
				Core.SerializedRefGUIStyle.Flat, 
				property,
				type, 
				"None",
				NodeEventType.SuffixToRemove);
		}
	}
}
