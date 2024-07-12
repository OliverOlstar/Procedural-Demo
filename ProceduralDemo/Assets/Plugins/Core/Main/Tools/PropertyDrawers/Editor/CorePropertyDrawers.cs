
using System.Linq;
using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using Object = UnityEngine.Object;

namespace Core
{
	public static class PropertyDrawerUtil
	{
		private readonly GUIStyle m_ToolTipIconStyle = new GUIStyle(EditorStyles.label)
		{
			normal.textColor *= 0.6f;
		};

		public static MethodInfo GetMethodOrProperty(Type type, string name)
		{
			BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			MethodInfo method = type.GetMethod(name, flags);
			if (method == null)
			{
				PropertyInfo conditonProperty = type.GetProperty(name, flags);
				if (conditonProperty != null)
				{
					method = conditonProperty.GetGetMethod();
				}
			}
			return method;
		}

		public static Type GetUnderlyingType(FieldInfo fieldInfo) => GetUnderlyingType(fieldInfo.FieldType);
		/// <summary>Utility function for property drawers to get the field type and clean it up in the case the drawer is applied to a list or array</summary>
		public static Type GetUnderlyingType(Type fieldType)
		{
			if (fieldType.IsArray) // Attribute could be attached to an Array element
			{
				fieldType = fieldType.GetElementType();
			}
			else if (fieldType.IsGenericType) // Attribute could be attached to a List element
			{
				fieldType = fieldType.GetGenericArguments()[0];
			}
			return fieldType;
		}

		public static bool TryGetAttribute<TAttribute>(FieldInfo fieldInfo, out TAttribute attribute) where TAttribute : Attribute
		{
			attribute = fieldInfo.GetCustomAttribute<TAttribute>();
			if (attribute != null)
			{
				return true;
			}
			Type type = GetUnderlyingType(fieldInfo);
			attribute = type.GetCustomAttribute<TAttribute>(true);
			if (attribute != null)
			{
				return true;
			}
			return false;
		}
		public static bool HasAttribute<TAttribute>(FieldInfo fieldInfo) where TAttribute : Attribute
		{
			return TryGetAttribute<TAttribute>(fieldInfo, out _);
		}

		/// <summary>This seems to be the most robust way to indent when dealing with nested inspectors.
		/// Using this fixes issues with indenting, then drawing another property drawer that modifies EditorGUI.indentLevel</summary>
		public static Rect IndentRect(Rect rect)
		{
			// Apparently EditorGUI.IndentedRect sometimes indents without increasing EditorGUI.indentLevel but sometimes it doesn't
			Rect indented = EditorGUI.IndentedRect(rect);
			if (Util.Approximately(indented.x, rect.x))
			{
				EditorGUI.indentLevel++;
				indented = EditorGUI.IndentedRect(rect);
				EditorGUI.indentLevel--;
			}
			rect.xMin = indented.x;
			return rect;
		}

		public static void ApplyTooltipAttribute(Rect position, GUIContent label, TooltipAttribute tt)
		{
			if (!string.IsNullOrEmpty(tt.m_Tooltip))
			{
				label.tooltip = tt.m_Tooltip;
			}
			TryApplyTooltipIcon(position, label);
		}

		public static void TryApplyTooltipIcon(Rect position, GUIContent label)
		{
			if (string.IsNullOrEmpty(label.tooltip))
			{
				return;
			}
			position = EditorGUI.IndentedRect(position);
			Vector2 size = EditorStyles.label.CalcSize(label);
			Rect r = new(position.x + size.x - 4, position.y - 4, 12, position.height);
			GUI.Label(r, "\u25E5", m_ToolTipIconStyle);
		}
	}

	[CustomPropertyDrawer(typeof(LightDirAttribute))]
	public class LightDirDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			Vector3 v = Vector3.Normalize(prop.vector3Value);

			v = EditorGUI.Vector3Field(position, label, v);

			prop.vector3Value = Vector3.Normalize(v);
		}
	}

	[CustomPropertyDrawer(typeof(SuffixAttribute))]
	public class SuffixAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			label.text += ((SuffixAttribute)attribute).m_Suffix;
			EditorGUI.PropertyField(position, prop, label);
		}
	}

	[CustomPropertyDrawer(typeof(TooltipAttribute))]
	public class ToolTipAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			PropertyDrawerUtil.ApplyTooltipAttribute(position, label, attribute as TooltipAttribute);
			EditorGUI.PropertyField(position, prop, label, true);
		}
	}

	[CustomPropertyDrawer(typeof(PercentAttribute))]
	public class PercentAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			PercentAttribute pctAtt = attribute as PercentAttribute;
			GUIContent newLabel = new(label); // Seems when we modify incoming label weird stuff happens, maybe only when using Odin Inspector?
			newLabel.text += " %";

			PropertyDrawerUtil.ApplyTooltipAttribute(position, newLabel, pctAtt);

			float percent = prop.floatValue * 100.0f;
			percent = EditorGUI.FloatField(position, newLabel, percent);
			if (pctAtt.m_Clamp)
			{
				percent = Mathf.Clamp(percent, 0.0f, 100.0f);
			}
			prop.floatValue = percent / 100.0f;
		}
	}
	[CustomPropertyDrawer(typeof(FramesAttribute))]
	public class FramesAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			GUIContent newLabel = new(label); // Seems when we modify incoming label weird stuff happens, maybe only when using Odin Inspector?
			newLabel.text += " (30fps)";

			PropertyDrawerUtil.ApplyTooltipAttribute(position, label, attribute as TooltipAttribute);

			float frames = prop.floatValue * Util.FPS30;
			frames = EditorGUI.FloatField(position, newLabel, frames);
			prop.floatValue = frames / Util.FPS30;
		}
	}
	[CustomPropertyDrawer(typeof(Frames60Attribute))]
	public class Frames60AttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			GUIContent newLabel = new(label); // Seems when we modify incoming label weird stuff happens, maybe only when using Odin Inspector?
			newLabel.text += " (60fps)";

			PropertyDrawerUtil.ApplyTooltipAttribute(position, label, attribute as TooltipAttribute);

			float frames = prop.floatValue / Util.SPF60;
			frames = EditorGUI.FloatField(position, newLabel, frames);
			prop.floatValue = frames * Util.SPF60;
		}
	}

	[CustomPropertyDrawer(typeof(DegreeAttribute))]
	public class DegreeAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			GUIContent newLabel = new(label); // Seems when we modify incoming label weird stuff happens, maybe only when using Odin Inspector?
			newLabel.text += " (degrees)";

			PropertyDrawerUtil.ApplyTooltipAttribute(position, label, attribute as TooltipAttribute);

			float degrees = Mathf.Rad2Deg * prop.floatValue;
			degrees = EditorGUI.FloatField(position, newLabel, degrees);
			prop.floatValue = Mathf.Deg2Rad * degrees;
		}
	}

	[CustomPropertyDrawer(typeof(RangedAttribute))]
	public class RangedAttributeDrawer : PropertyDrawer
	{
		// Draw the property inside the given rect
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// First get the attribute since it contains the range for the slider
			RangedAttribute range = (RangedAttribute)attribute;

			// Now draw the property as a Slider or an IntSlider based on whether it's a float or integer.
			if (property.propertyType == SerializedPropertyType.Float)
			{
				EditorGUI.Slider(position, property, range.m_Min, range.m_Max, label);
			}
			else if (property.propertyType == SerializedPropertyType.Integer)
			{
				EditorGUI.IntSlider(position, property, (int)range.m_Min, (int)range.m_Max, label);
			}
			else
			{
				EditorGUI.LabelField(position, label.text, "Use RangedAttribute with float or int.");
			}

			PropertyDrawerUtil.TryApplyTooltipIcon(position, label);
		}
	}

	[CustomPropertyDrawer(typeof(MinAttribute))]
	public class MinAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			MinAttribute min = (MinAttribute)attribute;
			if (property.propertyType == SerializedPropertyType.Float)
			{
				property.floatValue = Mathf.Max(
					min.m_Min,
					EditorGUI.FloatField(position, label, property.floatValue));
			}
			else if (property.propertyType == SerializedPropertyType.Integer)
			{
				property.intValue = Mathf.Max(
					(int)min.m_Min,
					EditorGUI.IntField(position, label, property.intValue));
			}
			else
			{
				EditorGUI.LabelField(position, label.text, "Use MinAttribute with float or int.");
			}
		}
	}

	[CustomPropertyDrawer(typeof(StringAutoCompleteAttribute))]
	public class StringAutoCompletePropertyDrawer : PropertyDrawer
	{
		static readonly Object[] EMPTY = new Object[0];

		string m_RawText = null;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			StringAutoCompleteAttribute stringAttr = (StringAutoCompleteAttribute)attribute;
			if (stringAttr.mGetSuggestionsFunction != null)
			{
				Object obj = property.serializedObject.targetObject;
				MethodInfo getOptions = PropertyDrawerUtil.GetMethodOrProperty(obj.GetType(), stringAttr.mGetSuggestionsFunction);
				stringAttr.mSuggestions = (string[])getOptions.Invoke(obj, EMPTY);
			}
			if (m_RawText == null)
			{
				m_RawText = property.stringValue;
			}

			EditorGUI.BeginChangeCheck();
			string text = EditorGUI.TextArea(position, property.stringValue);
			property.stringValue = text;
			if (EditorGUI.EndChangeCheck())
			{
				if (m_RawText.Length < text.Length)
				{
					TextEditor tEditor = typeof(EditorGUI)
						  .GetField("activeEditor", BindingFlags.Static | BindingFlags.NonPublic)
						  .GetValue(null) as TextEditor;
					string suggestedText = null;
					foreach (string suggestion in stringAttr.mSuggestions)
					{
						if (suggestedText != null && 0 < suggestion.CompareTo(suggestedText))
						{
							continue;
						}
						if (suggestion != text && suggestion.StartsWith(text))
						{
							suggestedText = suggestion;
						}
					}
					if (!string.IsNullOrEmpty(suggestedText))
					{
						property.stringValue = suggestedText;
						tEditor.text = suggestedText;
						tEditor.selectIndex = suggestedText.Length;
					}
				}
				m_RawText = text;
			}
		}
	}

	[CustomPropertyDrawer(typeof(StringDropdownAttribute))]
	public class StringDropdownPropertyDrawer : PropertyDrawer
	{
		static readonly Object[] EMPTY = new Object[0];

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			StringDropdownAttribute stringAttr = (StringDropdownAttribute)attribute;
			if (stringAttr.mGetOptionsFunction != null)
			{
				Object obj = property.serializedObject.targetObject;
				MethodInfo getOptions = obj.GetType().GetMethod(stringAttr.mGetOptionsFunction);
				stringAttr.mOptions = (string[])getOptions.Invoke(obj, EMPTY);
			}

			int index = 0;
			int count = stringAttr.mOptions.Length;
			for (int i = 0; i < count; i++)
			{
				if (property.stringValue == stringAttr.mOptions[i])
				{
					index = i;
					break;
				}
			}
			index = EditorGUI.Popup(position, index, stringAttr.mOptions);
			property.stringValue = stringAttr.mOptions[index];
		}
	}

	[CustomPropertyDrawer(typeof(EnumListAttribute))]
	public class EnumListPropertyDrawer : PropertyDrawer
	{
		static int IndexOf(SerializedProperty prop, ref SerializedProperty array)
		{
			string propPath = prop.propertyPath;
			int arrayIndex = propPath.LastIndexOf(".Array");
			if (arrayIndex < 0)
			{
				return -1;
			}
			string arrayPath = propPath.Substring(0, arrayIndex);
			array = prop.serializedObject.FindProperty(arrayPath);
			int i = 0;
			for (i = 0; i < array.arraySize; i++)
			{
				if (array.GetArrayElementAtIndex(i).propertyPath == propPath)
				{
					break;
				}
			}
			return i;
		}

		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			EnumListAttribute enumAttr = (EnumListAttribute)attribute;
			string[] enumNames = Enum.GetNames(enumAttr.mEnumType);
			SerializedProperty array = null;
			int i = IndexOf(prop, ref array);
			if (i < 0)
			{
				return;
			}

			if (array.arraySize != enumNames.Length)
			{
				array.arraySize = enumNames.Length;
			}

			string enumName = enumNames[i];
			if (enumName.Contains("FREE") ||
				enumName == "None" ||
				enumName == "Last")
			{
				return;
			}

			Rect labelpos = new(position.x, position.y, position.width, 16.0f);
			labelpos.x += 16.0f * (EditorGUI.indentLevel - 1);
			GUI.Label(labelpos, enumName, EditorStyles.boldLabel);

			float y = position.y + labelpos.height;

			bool hasChildren = false;
			string propPath = prop.propertyPath;
			foreach (SerializedProperty p in prop)
			{
				hasChildren = true;
				string[] s = p.propertyPath.Split(new string[] { propPath, "." }, StringSplitOptions.RemoveEmptyEntries);
				if (s.Length > 1)
				{
					break;
				}

				float height = EditorGUI.GetPropertyHeight(p);
				labelpos.height += height;
				//GUI.Box(labelpos, string.Empty);

				//Debug.Log(p.propertyType + "|" + p.propertyPath);
				//GUI.Label(r, p.propertyType + "|" + p.propertyPath);
				Rect r1 = new(labelpos.x, y, position.width, height);
				GUI.Box(r1, string.Empty);
				Rect r2 = new(position.x, y, position.width, height);
				EditorGUI.PropertyField(r2, p, true);

				y += height;
			}

			if (!hasChildren)
			{
				float height = 16.0f;
				float indent = 96.0f;
				Rect r = new(labelpos.x + indent, labelpos.y, position.width - indent, height);
				EditorGUI.PropertyField(r, array.GetArrayElementAtIndex(i), GUIContent.none);
			}
		}

		public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
		{
			float height = 0.0f;

			EnumListAttribute enumAttr = (EnumListAttribute)attribute;
			string[] enumNames = Enum.GetNames(enumAttr.mEnumType);

			SerializedProperty array = null;
			int i = IndexOf(prop, ref array);
			if (i < 0 || i >= enumNames.Length)
			{
				return 0.0f;
			}
			string enumName = enumNames[i];
			if (enumName.Contains("FREE") ||
				enumName == "None" ||
				enumName == "Last")
			{
				return height;
			}

			height += 16.0f;

			string parentPath = prop.propertyPath;
			foreach (SerializedProperty p in prop)
			{
				string[] s = p.propertyPath.Split(new string[] { parentPath, "." }, StringSplitOptions.RemoveEmptyEntries);
				if (s.Length > 1)
				{
					break;
				}
				height += EditorGUI.GetPropertyHeight(p);
			}
			return height;
		}
	}

	[CustomPropertyDrawer(typeof(EnumMaskAttribute))]
	public class EnumMaskAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EnumMaskAttribute at = attribute as EnumMaskAttribute;
			string[] options = Enum.GetNames(at.Type != null && at.Type.IsEnum ? at.Type : fieldInfo.FieldType);
			property.intValue = EditorGUI.MaskField(position, label, property.intValue, options);
		}
	}

	[CustomPropertyDrawer(typeof(EnumIncludeAttribute))]
	public class EnumIncludeAttributeDrawer : PropertyDrawer
	{
		private static EnumIncludeAttribute s_Attribute;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!fieldInfo.FieldType.IsEnum)
			{
				base.OnGUI(position, property, label);
				return;
			}
			s_Attribute = attribute as EnumIncludeAttribute;
			DrawDropdown(position, property, label, IsValid);
		}
		private bool IsValid(int index) => s_Attribute.IncludeIndexes.Contains(index);

		public static void DrawDropdown(in Rect position, SerializedProperty property, GUIContent label, Func<int, bool> isValid)
		{
			string[] options = property.enumDisplayNames;
			int firstValid = -1;
			for (int i = 0; i < options.Length; i++)
			{
				if (isValid.Invoke(i))
				{
					firstValid = i;
					break;
				}
			}
			int lastValid = firstValid;
			for (int i = 0; i < options.Length; i++)
			{
				if (i <= firstValid)
				{
					options[i] = options[firstValid];
				}
				else if (isValid.Invoke(i))
				{
					lastValid = i;
				}
				else
				{
					options[i] = options[lastValid];
				}
			}
			property.enumValueIndex = EditorGUI.Popup(position, label.text, property.enumValueIndex, options);
			property.enumValueIndex = Mathf.Clamp(property.enumValueIndex, firstValid, lastValid);
		}
	}

	[CustomPropertyDrawer(typeof(EnumExcludeAttribute), true)]
	public class EnumExcludeAttributeDrawer : PropertyDrawer
	{
		private static EnumExcludeAttribute s_Attribute;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!fieldInfo.FieldType.IsEnum)
			{
				base.OnGUI(position, property, label);
				return;
			}
			s_Attribute = attribute as EnumExcludeAttribute;
			EnumIncludeAttributeDrawer.DrawDropdown(position, property, label, IsValid);
		}
		private bool IsValid(int index) => !s_Attribute.ExcludeIndexes.Contains(index);
	}
	
	[CustomPropertyDrawer(typeof(EnumExcludeLastAttribute))]
	public class EnumExcludeLastAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!fieldInfo.FieldType.IsEnum)
			{
				base.OnGUI(position, property, label);
				return;
			}
			string[] options = property.enumDisplayNames;
			Array.Resize(ref options, options.Length - 1);
			property.enumValueIndex = EditorGUI.Popup(position, property.enumValueIndex, options);
			property.enumValueIndex = Mathf.Min(property.enumValueIndex, options.Length - 1);
		}
	}

	[CustomPropertyDrawer(typeof(HelpBoxAttribute))]
	public class HelpBoxAttributeDrawer : DecoratorDrawer
	{
		public override float GetHeight()
		{
			// TODO: Trying to find a way to support text wrapping, maybe one of these can help
			//float width = EditorGUIUtility.currentViewWidth;
			//Rect r = GUILayoutUtility.GetLastRect();

			HelpBoxAttribute att = attribute as HelpBoxAttribute;
			string[] split = att.m_MessageText.Split('\n');
			return EditorGUIUtility.singleLineHeight * split.Length;
		}

		public override void OnGUI(Rect position)
		{
			HelpBoxAttribute att = attribute as HelpBoxAttribute;
			MessageType type = default;
			switch (att.m_MessageType)
			{
				case HelpBoxAttribute.MessageType.None:
					type = MessageType.None;
					break;
				case HelpBoxAttribute.MessageType.Info:
					type = MessageType.Info;
					break;
				case HelpBoxAttribute.MessageType.Warning:
					type = MessageType.Warning;
					break;
				case HelpBoxAttribute.MessageType.Error:
					type = MessageType.Error;
					break;
			}
			string msg = att.m_MessageText;
			string[] split = att.m_MessageText.Split('\n');
			if (split.Length == 1)
			{
				msg = "  " + msg;
			}
			EditorGUI.HelpBox(position, msg, type);
		}
	}

	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	public class ReadOnlyPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			GUI.enabled = false;
			ReadOnlyAttribute att = attribute as ReadOnlyAttribute;
			if (att.m_Flatten)
			{
				FlattenPropertyDrawer.OnGUI(position, property, label, false, true, true);
			}
			else
			{
				EditorGUI.PropertyField(position, property, label, true);
			}
			GUI.enabled = true;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			ReadOnlyAttribute att = attribute as ReadOnlyAttribute;
			if (att.m_Flatten)
			{
				return FlattenPropertyDrawer.GetPropertyHeight(property, false, true, true);
			}
			else
			{
				return base.GetPropertyHeight(property, label);
			}
		}
	}

	[CustomPropertyDrawer(typeof(FlattenAttribute))]
	public class FlattenPropertyDrawer : PropertyDrawer
	{
		private static readonly float SMALL_OFFSET = 2.0f;
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!property.hasVisibleChildren)
			{
				EditorGUI.PropertyField(position, property, label); // Already flat
				return;
			}
			FlattenAttribute att = attribute as FlattenAttribute;
			position.y += SMALL_OFFSET;
			OnGUI(position, property, label, att.m_Box, att.m_Title, att.m_Indent);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (!property.hasVisibleChildren)
			{
				return base.GetPropertyHeight(property, label); // Already flat
			}
			FlattenAttribute att = attribute as FlattenAttribute;
			float height = 2.0f * SMALL_OFFSET;
			height += GetPropertyHeight(property, att.m_Box, att.m_Title, att.m_Indent);
			return height;
		}

		public static void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool box, bool title, bool indent)
		{
			if (!property.hasVisibleChildren)
			{
				EditorGUI.PropertyField(position, property, label); // Already flat
				return;
			}
			if (box)
			{
				EditorGUI.HelpBox(position, "", MessageType.None);
				position.y += 2.0f;
				position.x += 4.0f;
				position.width -= 8.0f;
			}

			if (title)
			{
				position.height = EditorGUIUtility.singleLineHeight;
				EditorGUI.LabelField(position, label, EditorStyles.boldLabel);
				position.y += position.height;
			}

			if (indent)
			{
				position = PropertyDrawerUtil.IndentRect(position);
			}

			int depth = property.depth;
			property.NextVisible(true);
			do
			{
				position.height = EditorGUI.GetPropertyHeight(property, true);
				EditorGUI.PropertyField(position, property, true);
				position.y += position.height;
			}
			while (property.NextVisible(false) && property.depth > depth);
		}

		public static float GetPropertyHeight(SerializedProperty property, bool box, bool title, bool indent)
		{
			if (!property.hasVisibleChildren)
			{
				return EditorGUI.GetPropertyHeight(property); // Already flat
			}
			float height = 0.0f;
			if (box)
			{
				height += 4.0f;
			}
			if (title)
			{
				height += EditorGUIUtility.singleLineHeight;
			}
			int depth = property.depth;
			property.NextVisible(true);
			do
			{
				height += EditorGUI.GetPropertyHeight(property, true);
			}
			while (property.NextVisible(false) && property.depth > depth);
			return height;
		}
	}

	[CustomPropertyDrawer(typeof(FilePickerAttribute))]
	public class FilePickerPropertyDrawer : PropertyDrawer
	{
		private const float BUTTON_WIDTH = 25.0f;
		private const float GOTO_WIDTH = 20.0f;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			FilePickerAttribute att = attribute as FilePickerAttribute;

			// Property
			float totalButtonWidth = BUTTON_WIDTH * (att.m_ShowFolders && att.m_ShowFiles ? 2 : 1);
			totalButtonWidth += GOTO_WIDTH * (att.m_GotoButton ? 1 : 0);
			position.width -= totalButtonWidth;
			EditorGUI.PropertyField(position, property, label, true);
			position.x += position.width;

			// Goto
			bool isPathValid = IsValidPath(property.stringValue);
			if (att.m_GotoButton)
			{
				GUI.enabled = isPathValid;
				position.width = GOTO_WIDTH;

				if (GUI.Button(position, UberPickerGUI.POINTER_UNICODE, GUI.skin.label))
				{
					Object obj = AssetDatabase.LoadAssetAtPath(property.stringValue, typeof(Object));
					if (obj != null)
					{
						EditorGUIUtility.PingObject(obj);
					}
				}

				GUI.enabled = true;
				position.x += GOTO_WIDTH;
			}
			
			position.width = BUTTON_WIDTH;
			string currPath = isPathValid ? property.stringValue : Application.dataPath;
			// Folders
			if (att.m_ShowFolders)
			{
				if (GUI.Button(position, "..."))
				{
					string input = EditorUtility.OpenFolderPanel("Pick Folder", currPath, "");
					SetPropertyValue(ref property, input);
				}
				position.x += BUTTON_WIDTH;
			}
			
			// Files
			if (att.m_ShowFiles && GUI.Button(position, "..."))
			{
				string input = EditorUtility.OpenFilePanel("Pick File", currPath, "");
				SetPropertyValue(ref property, input);
			}
		}

		private bool IsValidPath(in string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return false;
			}
			return !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path));
		}

		private void SetPropertyValue(ref SerializedProperty property, string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return;
			}
			property.stringValue = $"Assets{input.Remove(0, Application.dataPath.Length)}";
			GUI.FocusControl(null);
		}
	}
}
