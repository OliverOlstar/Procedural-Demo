using UnityEngine;
using UnityEditor;

namespace ODev.Picker
{
	[CustomPropertyDrawer(typeof(AssetAttribute), true)]
	public class AssetPickerDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			AssetAttribute at = attribute as AssetAttribute;
			UberPickerGUI.GUIObject(
				property,
				position,
				label,
				fieldInfo,
				at,
				new AssetPickerPathSource(new System.Type[] { fieldInfo.FieldType }, at.PathPrefix, canBeNested: at.CanBeNested));
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => UberPickerGUI.GetPropertyHeight(property);
	}

	[CustomPropertyDrawer(typeof(AssetNameAttribute))]
	public class AssetNamePickerDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			AssetNameAttribute at = attribute as AssetNameAttribute;
			UberPickerGUI.GUIString(
				property,
				position,
				label,
				fieldInfo,
				at,
				new AssetPickerPathSource(at.Types, at.PathPrefix));
		}
	}
}