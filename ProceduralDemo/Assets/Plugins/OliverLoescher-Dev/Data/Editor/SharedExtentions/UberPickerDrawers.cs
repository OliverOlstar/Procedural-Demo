using UnityEngine;
using UnityEditor;
using ODev.Picker;

namespace ODev.Data.Editor
{
	[CustomPropertyDrawer(typeof(Data.UberPicker.DataIDAttribute))]
	public class DataIDDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			UberPicker.DataIDAttribute at = attribute as Data.UberPicker.DataIDAttribute;
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
			UberPicker.DataVariantAttribute at = attribute as Data.UberPicker.DataVariantAttribute;
			UberPickerGUI.GUIString(
				property,
				position,
				label,
				fieldInfo,
				at,
				new DataVariantPickerPathSource());
		}
	}
}