using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Core
{
	public static class ConditionalDrawerUtil
	{
		public static bool CompareMemberToValues(SerializedProperty property, string memberName, object[] memberValues)
		{
			foreach (object value in memberValues)
			{
				if (CompareMemberToValue(property, memberName, value))
				{
					return true;
				}
			}
			return false;
		}
		public static bool CompareMemberToValue(SerializedProperty property, string memberName, object memberValue)
		{
			SerializedProperty prop = null;
			if (property.depth == 0)
			{
				prop = property.serializedObject.FindProperty(memberName);
			}
			else
			{
				string path = property.propertyPath;
				string parentPath = path.Substring(0, path.LastIndexOf('.'));
				SerializedProperty parent = property.serializedObject.FindProperty(parentPath);
				prop = parent.FindPropertyRelative(memberName);
			}
			bool show = true;
			switch (prop.propertyType)
			{
				case SerializedPropertyType.Enum:
					show = prop.intValue == (int)memberValue;
					break;
				case SerializedPropertyType.Boolean:
					show = prop.boolValue == (bool)memberValue;
					break;
			}
			return show;
		}

		public static bool EvaluateFunction(SerializedProperty property, string functionName)
		{
			Object obj = property.serializedObject.targetObject;
			MethodInfo conditon = PropertyDrawerUtil.GetMethodOrProperty(obj.GetType(), functionName);
			if (conditon == null)
			{
				Debug.LogError($"ConditionalDrawerUtil.EvaluateFunction() Attribute on property '{property.propertyPath}' " +
					$"cannot find method to evaluate named '{functionName}'");
				return false;
			}
			bool result = (bool)conditon.Invoke(obj, new Object[] { });
			return result;
		}

		public static void OnGUI(Rect position, SerializedProperty property, GUIContent label, ConditionalBaseAttribute attribute)
		{
			if (attribute.m_FlattenStyle != ConditionalAttributeStyle.None && property.hasVisibleChildren)
			{
				bool box = attribute.m_FlattenStyle == ConditionalAttributeStyle.FlattenBoxed;
				FlattenPropertyDrawer.OnGUI(position, property, label, box, box, attribute.m_Indent);
			}
			else
			{
				if (attribute.m_Indent)
				{
					position = PropertyDrawerUtil.IndentRect(position);
				}
				EditorGUI.PropertyField(position, property, label, true);
			}
		}

		public static float GetPropertyHeight(SerializedProperty property, GUIContent label, ConditionalBaseAttribute attribute)
		{
			if (attribute.m_FlattenStyle != ConditionalAttributeStyle.None)
			{
				bool box = attribute.m_FlattenStyle == ConditionalAttributeStyle.FlattenBoxed;
				float height = FlattenPropertyDrawer.GetPropertyHeight(property, box, box, attribute.m_Indent);
				return height;
			}
			else
			{
				return EditorGUI.GetPropertyHeight(property, true);
			}
		}
	}

	[CustomPropertyDrawer(typeof(ConditionalAttribute))]
	public class ConditionalAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			ConditionalAttribute att = attribute as ConditionalAttribute;
			if (!ConditionalDrawerUtil.CompareMemberToValues(property, att.m_MemberVariableName, att.m_MemberVariableValue))
			{
				return;
			}
			ConditionalDrawerUtil.OnGUI(position, property, label, att);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			ConditionalAttribute att = attribute as ConditionalAttribute;
			if (!ConditionalDrawerUtil.CompareMemberToValues(property, att.m_MemberVariableName, att.m_MemberVariableValue))
			{
				return 0.0f;
			}
			return ConditionalDrawerUtil.GetPropertyHeight(property, label, att);
		}
	}

	[CustomPropertyDrawer(typeof(HideIf))]
	public class HideInspectorIfAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			HideIf att = attribute as HideIf;
			if (ConditionalDrawerUtil.EvaluateFunction(property, att.m_MethodName))
			{
				return;
			}
			ConditionalDrawerUtil.OnGUI(position, property, label, att);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			HideIf att = attribute as HideIf;
			if (ConditionalDrawerUtil.EvaluateFunction(property, att.m_MethodName))
			{
				return 0.0f;
			}
			return ConditionalDrawerUtil.GetPropertyHeight(property, label, att);
		}
	}

	[CustomPropertyDrawer(typeof(ShowIf))]
	public class ShowInspectorIfAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			ShowIf att = attribute as ShowIf;
			if (!ConditionalDrawerUtil.EvaluateFunction(property, att.m_MethodName))
			{
				return;
			}
			ConditionalDrawerUtil.OnGUI(position, property, label, att);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			ShowIf att = attribute as ShowIf;
			if (!ConditionalDrawerUtil.EvaluateFunction(property, att.m_MethodName))
			{
				return 0.0f;
			}
			return ConditionalDrawerUtil.GetPropertyHeight(property, label, att);
		}
	}
}