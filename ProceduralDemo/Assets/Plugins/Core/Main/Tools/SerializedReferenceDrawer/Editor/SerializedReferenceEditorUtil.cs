#if UNITY_2021_1_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Core
{
	public static class SerializedReferenceEditorUtil
	{
		public static Dictionary<System.Type, Cache> s_Cache = new Dictionary<System.Type, Cache>();

		public class Cache
		{
			public string[] TypeNames;
			public List<System.Type> Types;
			public bool HasNullEntry;

			public static Cache Get(
				System.Type type, 
				string nullEntryName = null,
				string suffixToRemoveFromTypeNames = null)
			{

				if (s_Cache.TryGetValue(type, out Cache cache))
				{
					return cache;
				}

				cache = new Cache();
				cache.Types = new List<System.Type>();
				if (type != null)
				{
					foreach (System.Type t in TypeUtility.GetAllTypes())
					{
						if (t.IsGenericTypeDefinition ||
							t.IsAbstract ||
							!type.IsAssignableFrom(t) ||
							t.IsDefined(typeof(System.ObsoleteAttribute), false))
						{
							continue;
						}
						cache.Types.Add(t);
					}
				}
				else
				{
					Debug.LogError($"SerializedReferenceEditorUtil.OnPropertyGUI() Cannot find any class with name '{type.Name}'");
				}
				cache.HasNullEntry = !string.IsNullOrEmpty(nullEntryName) || cache.Types.Count == 0;
				if (cache.HasNullEntry)
				{
					cache.Types.Insert(0, null);
				}
				cache.TypeNames = new string[cache.Types.Count];
				for (int i = 0; i < cache.Types.Count; i++)
				{
					if (i == 0 && cache.HasNullEntry)
					{
						cache.TypeNames[i] = !string.IsNullOrEmpty(nullEntryName) ? nullEntryName : "Null";
						continue;
					}
					string name = cache.Types[i].Name;
					if (!string.IsNullOrEmpty(suffixToRemoveFromTypeNames) && name.EndsWith(suffixToRemoveFromTypeNames))
					{
						name = name.Substring(0, name.Length - suffixToRemoveFromTypeNames.Length);
					}
					cache.TypeNames[i] = name;
				}

				s_Cache.Add(type, cache);
				return cache;
			}
		}

		public static void OnPropertyGUI(
			SerializedRefGUIStyle style,
			Rect position,
			GUIContent label,
			SerializedProperty property,
			System.Type type,
			string nullEntryName = null,
			string suffixToRemoveFromTypeNames = null)
		{
			Cache cache = Cache.Get(type, nullEntryName, suffixToRemoveFromTypeNames);
			if (string.IsNullOrEmpty(nullEntryName) && // Null is not allowed, check if we need to fix up reference
				property.managedReferenceValue == null && 
				cache.TypeNames.Length > 0)
			{
				object instance = System.Activator.CreateInstance(cache.Types[0]);
				property.managedReferenceValue = instance;
			}

			position.height = EditorGUIUtility.singleLineHeight;
			switch (style)
			{
				case SerializedRefGUIStyle.Foldout:
					{
						if (property.hasVisibleChildren)
						{
							Rect labelPosition = position;
							labelPosition.width = EditorGUIUtility.labelWidth;
							property.isExpanded = EditorGUI.Foldout(labelPosition, property.isExpanded, label, true);
							TryDrawTypePicker(position, property, cache, null);
							if (property.isExpanded)
							{
								position.y += position.height;
								position = PropertyDrawerUtil.IndentRect(position);
								DrawProperties(position, property);
							}
						}
						else if (!TryDrawTypePicker(position, property, cache, label.text))
						{
							EditorGUI.LabelField(position, label);
						}
						break;
					}
				case SerializedRefGUIStyle.Header:
					{
						EditorGUI.LabelField(position, label, EditorStyles.boldLabel);
						position.y += position.height;
						position = PropertyDrawerUtil.IndentRect(position);
						string popupLabel = label.text + " Type";
						if (TryDrawTypePicker(position, property, cache, popupLabel))
						{
							position.y += position.height;
						}
						DrawProperties(position, property);
						break;
					}
				case SerializedRefGUIStyle.Flat:
					{
						if (TryDrawTypePicker(position, property, cache, label.text))
						{
							position.y += position.height;
						}
						DrawProperties(position, property);
						break;
					}
				case SerializedRefGUIStyle.FlatIndented:
					{
						if (TryDrawTypePicker(position, property, cache, label.text))
						{
							position.y += position.height;
						}
						position = PropertyDrawerUtil.IndentRect(position);
						DrawProperties(position, property);
						break;
					}
			}
		}

		private static bool TryDrawTypePicker(
			Rect position,
			SerializedProperty property,
			Cache cache,
			string label = null)
		{
			switch (cache.TypeNames.Length)
			{
				case 0:
					return false;
				case 1:
					FixUp(property, cache.Types[0]); // As long as we have a valid type, still want to run fix up
					return false;
			}

			object obj = property.managedReferenceValue;
			int index = -1; // Note: -1 is a valid value for index in the case that serialized type has been marked obsolete

			if (obj != null)
			{
				for (int i = 0; i < cache.Types.Count; i++)
				{
					if (obj.GetType() == cache.Types[i])
					{
						index = i;
						break;
					}
				}
			}
			int newIndex = string.IsNullOrEmpty(label) ?
				EditorGUI.Popup(position, " ", index, cache.TypeNames) :
				EditorGUI.Popup(position, label, index, cache.TypeNames);
			if (newIndex != index)
			{
				property.isExpanded = true;
				if (newIndex == 0 && cache.HasNullEntry)
				{
					property.managedReferenceValue = null;
				}
				else
				{
					object instance = System.Activator.CreateInstance(cache.Types[newIndex]);
					property.managedReferenceValue = instance;
				}
			}
			else if (index >= 0)
			{
				//double ms = Core.Chrono.UtcNowMilliseconds;
				FixUp(property, cache.Types[index]);
				//Debug.Log($"FixUp {property.propertyPath} {(Core.Chrono.UtcNowMilliseconds - ms).ToString("F1")}");
			}
			return true;
		}

		private static bool FixUp(SerializedProperty prop, System.Type type)
		{
			if (prop.managedReferenceValue == null)
			{
				return false;
			}
			SerializedProperty p = prop.serializedObject.GetIterator();
			do
			{
				if (p.propertyType != SerializedPropertyType.ManagedReference ||
					p.propertyPath == prop.propertyPath)
				{
					continue;
				}
				if (p.managedReferenceId == prop.managedReferenceId)
				{
					Debug.LogWarning($"SerializedReferenceEditorUtil.FixUp() Property '{prop.propertyPath}' " +
						$"has the same Managed Reference ID '{prop.managedReferenceId}' as another property '{p.propertyPath}' " +
						$"on object '{prop.serializedObject.targetObject.name}'. A new Managed Object instance of type '{type.Name}' will be created");
					object instance = System.Activator.CreateInstance(type);
					prop.managedReferenceValue = instance;
					return true;
				}
			}
			while (p.Next(true));
			return false;
		}

		private static void DrawProperties(Rect position, SerializedProperty property)
		{
			if (property.hasVisibleChildren)
			{
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
		}

		public static float GetPropertyHeight(
			SerializedRefGUIStyle style,
			SerializedProperty property,
			System.Type type,
			string nullEntryName = null,
			string suffixToRemoveFromTypeNames = null)
		{
			Cache cache = Cache.Get(type, nullEntryName, suffixToRemoveFromTypeNames);
			bool drawPicker = cache.TypeNames.Length > 1;
			float height = 0.0f;
			switch (style)
			{
				case SerializedRefGUIStyle.Foldout:
					height += EditorGUIUtility.singleLineHeight;
					if (property.isExpanded && property.hasVisibleChildren)
					{
						height += CalculatePropertiesHeight(property);
					}
					break;
				case SerializedRefGUIStyle.Header:
					height += EditorGUIUtility.singleLineHeight;
					if (drawPicker)
					{
						height += EditorGUIUtility.singleLineHeight;
					}
					height += CalculatePropertiesHeight(property);
					break;
				case SerializedRefGUIStyle.Flat:
				case SerializedRefGUIStyle.FlatIndented:
					if (drawPicker)
					{
						height += EditorGUIUtility.singleLineHeight;
					}
					height += CalculatePropertiesHeight(property);
					break;
			}
			return height;
		}

		private static float CalculatePropertiesHeight(SerializedProperty property)
		{
			float height = 0.0f;
			if (property.hasVisibleChildren)
			{
				height += 4.0f;
				int depth = property.depth;
				property.NextVisible(true);
				do
				{
					height += EditorGUI.GetPropertyHeight(property, true);
				}
				while (property.NextVisible(false) && property.depth > depth);
			}
			return height;
		}

		// Legacy code from some old property drawers we probably don't really want to be using anymore, can just use simpler SerializedReferenceDrawerAttribute now
		public static System.Type GetChildSerialRefType(SerializedProperty childSerialRefProperty, FieldInfo parentFieldInfo)
		{
			// https://docs.unity3d.com/ScriptReference/SerializedProperty-managedReferenceFieldTypename.html
			// The full static type name string returned has the following format: "[assembly-name] [namespace.][parent-class-names][classname]"
			string typeName = childSerialRefProperty.managedReferenceFieldTypename;
			int assemblyIndex = typeName.IndexOf(' ');
			if (assemblyIndex > 0)
			{
				typeName = typeName.Substring(assemblyIndex + 1);
			}
			// Remove assembly name and then add back in with the format required by Type.GetType()
			System.Type fieldType = PropertyDrawerUtil.GetUnderlyingType(parentFieldInfo);
			typeName += ", " + fieldType.Assembly.FullName;
			System.Type type = System.Type.GetType(typeName);
			return type;
		}
	}
}
#endif