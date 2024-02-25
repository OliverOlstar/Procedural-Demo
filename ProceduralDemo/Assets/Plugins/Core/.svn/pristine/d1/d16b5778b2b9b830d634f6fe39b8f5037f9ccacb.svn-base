using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SerializedType))]
public class SerializedTypeDrawer : PropertyDrawer
{
	private const string NONE = "(None)";

	private Dictionary<string, Type> m_MatchingTypes;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty typeProperty = property.FindPropertyRelative("m_Type");
		string typeName = typeProperty.stringValue;
		string buttonText = NONE;
		if (!string.IsNullOrEmpty(typeName))
		{
			Type type = Type.GetType(typeName);
			if (type != null)
			{
				buttonText = type.Name;
			}
			else
			{
				buttonText = typeName;
			}
		}
		float percent = (EditorGUIUtility.labelWidth + 14f) / position.width;
		Rect[] rects = SplitRect(position, percent);
		EditorGUI.LabelField(rects[0], label);
		if (GUI.Button(rects[1], buttonText, EditorStyles.popup))
		{
			CollectMatchingTypes(property);
			CreateDropDownMenu(typeProperty, typeName, rects[1]);
		}
	}

	private Rect[] SplitRect(Rect rect, float percent)
	{
		Rect left = new Rect(rect.x, rect.y, rect.width * percent, rect.height);
		Rect right = new Rect(left.width, rect.y, rect.width - left.width, rect.height);
		return new Rect[] { left, right };
	}

	private void CollectMatchingTypes(SerializedProperty property)
	{
		m_MatchingTypes = new Dictionary<string, Type>();
		object[] attributes = fieldInfo.GetCustomAttributes(typeof(TypeConstraintAttribute), false);
		foreach (object attribute in attributes)
		{
			if (attribute is TypeConstraintAttribute constraint)
			{
				foreach (Type type in Core.TypeUtility.GetAllTypes())
				{
					if (!m_MatchingTypes.ContainsKey(type.FullName) && constraint.IsTypeValid(type))
					{
						m_MatchingTypes.Add(type.FullName, type);
					}
				}
			}
		}
	}

	private void CreateDropDownMenu(SerializedProperty typeProperty, string currentTypeName, Rect position)
	{
		GenericMenu.MenuFunction2 callback = selected =>
		{
			OnOptionSelected(selected, typeProperty);
		};
		GenericMenu dropDownMenu = new GenericMenu();
		dropDownMenu.AddItem(new GUIContent(NONE), NONE == currentTypeName, callback, NONE);
		dropDownMenu.AddSeparator(string.Empty);
		foreach (var kvp in m_MatchingTypes)
		{
			dropDownMenu.AddItem(new GUIContent(kvp.Key), kvp.Key == currentTypeName, callback, kvp.Key);
		}
		dropDownMenu.DropDown(position);
	}

	private void OnOptionSelected(object selected, SerializedProperty typeProperty)
	{
		string typeName = selected as string;
		if (typeName == NONE)
		{
			typeProperty.stringValue = string.Empty;
			typeProperty.serializedObject.ApplyModifiedProperties();
		}
		else if (typeName != typeProperty.stringValue)
		{
			typeProperty.stringValue = m_MatchingTypes[typeName].AssemblyQualifiedName;
			typeProperty.serializedObject.ApplyModifiedProperties();
		}
	}

	private string[] GetDisplayOptions(Type[] types)
	{
		string[] options = new string[types.Length];
		for (int i = 0; i < options.Length; ++i)
		{
			options[i] = types[i].FullName;
		}
		return options;
	}

	private int GetSelectedIndex(string[] options, string selected)
	{
		for (int i = 0; i < options.Length; ++i)
		{
			if (options[i] == selected)
			{
				return i;
			}
		}
		return -1;
	}
}