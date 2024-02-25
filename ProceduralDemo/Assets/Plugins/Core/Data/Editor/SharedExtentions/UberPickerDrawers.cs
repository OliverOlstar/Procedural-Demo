using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Core;

[CustomPropertyDrawer(typeof(Data.UberPicker.DataIDAttribute))]
public class DataIDDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		Data.UberPicker.DataIDAttribute at = attribute as Data.UberPicker.DataIDAttribute;
		UberPickerGUI.GUIString(
			property,
			position,
			label,
			fieldInfo,
			at,
			new DataIDPickerPathSource(at.DBType));
	}
}

[CustomPropertyDrawer(typeof(Data.UberPicker.DataVariantAttribute))]
public class DataVariantDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		Data.UberPicker.DataVariantAttribute at = attribute as Data.UberPicker.DataVariantAttribute;
		UberPickerGUI.GUIString(
			property,
			position,
			label,
			fieldInfo,
			at,
			new DataVariantPickerPathSource());
	}
}