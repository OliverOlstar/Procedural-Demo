#if UNITY_2021_1_OR_NEWER
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace ODev
{
	[CustomPropertyDrawer(typeof(ReorderableListGeneric<>), true)]
	public class ReorderableListPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			ReorderableListDrawer list = ReorderableListDrawerCache.Get(property, label);
			list.OnGUI(position, property, label);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			ReorderableListDrawer list = ReorderableListDrawerCache.Get(property, label);
			float height = list.GetPropertyHeight(property);
			return height;
		}
	}

	public class ReorderableListDrawer
	{
		protected ReorderableListDrawerProperties m_Properties = null;
		public ReorderableListDrawerProperties Properties => m_Properties;

		protected ReorderableList m_ReorderableList = null;
		private GUIContent m_Label = null;

		public ReorderableListDrawer(SerializedProperty property, GUIContent label)
		{
			m_Label = label;

			m_Properties = ReorderableListDrawerProperties.Create(property, out SerializedObject obj, out SerializedProperty list);
			m_ReorderableList = new ReorderableList(obj, list, true, true, true, true);
			m_ReorderableList.drawHeaderCallback = OnDrawHeaderCallback;
			m_ReorderableList.drawElementCallback = OnDrawElementCallback;
			m_ReorderableList.elementHeightCallback = OnElementHeightCallback;

			m_ReorderableList.onReorderCallbackWithDetails = OnReorderCallback;

			m_ReorderableList.onAddCallback = OnAddCallback;
			m_ReorderableList.onRemoveCallback = OnRemoveCallback;
		}

		private void OnDrawHeaderCallback(Rect rect)
		{
			EditorGUI.LabelField(rect, m_Label);
			PropertyDrawerUtil.TryApplyTooltipIcon(rect, m_Label);
		}

		private void OnDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
		{
			if (!m_Properties.TryGetInline(out SerializedObject obj, out SerializedProperty list))
			{
				Debug.LogError("ReorderableListDrawer.DrawSheetElementCallback() IsValid sanity check failed");
				return;
			}
			if (index >= list.arraySize)
			{
				Debug.LogError("ReorderableListDrawer.DrawSheetElementCallback() Index out of range check failed");
				return;
			}
			rect.xMin += 8.0f; // Seems we need to offset slightly for foldout arrow
			SerializedProperty element = list.GetArrayElementAtIndex(index);
			DrawElement(rect, element);
			if (obj.hasModifiedProperties)
			{
				obj.ApplyModifiedProperties();
			}
		}

		protected virtual void DrawElement(Rect position, SerializedProperty element)
		{
			EditorGUI.PropertyField(position, element, true);
		}

		private float OnElementHeightCallback(int index)
		{
			if (!m_Properties.TryGetInline(out SerializedObject obj, out SerializedProperty list))
			{
				Debug.LogError("ReorderableListDrawer.ElementHeightCallback() IsValid sanity check failed");
				return 0.0f;
			}
			if (index >= list.arraySize)
			{
				Debug.LogError("ReorderableListDrawer.ElementHeightCallback() Index out of range sanity check failed");
				return 0.0f;
			}
			SerializedProperty element = list.GetArrayElementAtIndex(index);
			float height = EditorGUI.GetPropertyHeight(element, true);
			return height;
		}

		private void OnReorderCallback(ReorderableList reorderableList, int oldIndex, int newIndex)
		{
			if (!m_Properties.TryGetAsync(out SerializedObject obj, out SerializedProperty list))
			{
				Debug.LogError("ReorderableListDrawer.OnReorderCallback() IsValid sanity check failed");
				return;
			}
			list.MoveArrayElement(oldIndex, newIndex);
			if (obj.hasModifiedProperties)
			{
				obj.ApplyModifiedProperties();
			}
		}

		private void OnAddCallback(ReorderableList reorderableList)
		{
			if (!m_Properties.TryGetAsync(out SerializedObject obj, out SerializedProperty list))
			{
				Debug.LogError("ReorderableListDrawer.OnAddCallback() IsValid sanity check failed");
				return;
			}
			list.arraySize++;
			if (obj.hasModifiedProperties)
			{
				obj.ApplyModifiedProperties();
			}
		}

		private void OnRemoveCallback(ReorderableList reorderableList)
		{
			if (!m_Properties.TryGetAsync(out SerializedObject obj, out SerializedProperty list))
			{
				Debug.LogError("ReorderableListDrawer.OnRemoveCallback() IsValid sanity check failed");
				return;
			}
			foreach (int index in reorderableList.selectedIndices)
			{
				list.DeleteArrayElementAtIndex(index);
			}
			if (obj.hasModifiedProperties)
			{
				obj.ApplyModifiedProperties();
			}
		}

		public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			m_Label = label;
			m_Properties.Initialize(property, out SerializedObject obj, out SerializedProperty list);
			list.isExpanded = true; // List needs to be expanded for our somewhat hacky way of calculating height to work
			m_ReorderableList.serializedProperty = list;
			m_ReorderableList.DoList(position);
		}

		public float GetPropertyHeight(SerializedProperty property)
		{
			m_Properties.Initialize(property, out SerializedObject obj, out SerializedProperty list);
			// Seems like we can use the height from Unity's default list drawer, however there is a special case
			// Reorderable list seems to be a different size in the case where the list is empty
			if (list.arraySize == 0)
			{
				return 69.0f;
			}
			float height = 0.0f;
			height += EditorGUI.GetPropertyHeight(list, true);
			//float height = 2.0f * EditorGUIUtility.singleLineHeight;
			//for (int i = 0; i < m_SheetNamesProperty.arraySize; i++)
			//{
			//	height += ElementHeightCallback(i);
			//}
			return height;
		}
	}

	public class ReorderableListDrawerProperties
	{
		private const string LIST_PROPERTY_NAME = "m_List";

		// Note: I tried caching UnityEngine.Object and re-creating SerializedObject but that doesn't work with nested inspectors
		private SerializedObject m_Object = null;
		private string m_ListPath = null;

		private ReorderableListDrawerProperties() { }

		public static ReorderableListDrawerProperties Create(SerializedProperty propertyFromDrawer, out SerializedObject obj, out SerializedProperty list)
		{
			ReorderableListDrawerProperties instance = new();
			instance.Initialize(propertyFromDrawer, out obj, out list);
			return instance;
		}

		public void Initialize(SerializedProperty propertyFromDrawer, out SerializedObject obj, out SerializedProperty list)
		{
			obj = propertyFromDrawer.serializedObject;
			list = propertyFromDrawer.FindPropertyRelative(LIST_PROPERTY_NAME) ??
				throw new System.InvalidOperationException($"ReorderableList.Initialize() {propertyFromDrawer.propertyPath} has no property named {LIST_PROPERTY_NAME}");
			m_Object = obj;
			m_ListPath = list.propertyPath;
		}

		public bool IsValid()
		{
			bool value = TryGetInline(out _, out _);
			return value;
		}

		/// <summary>
		/// Use this TryGet for functions which are executed in-line during property drawer execution. 
		/// ie. Callstacks that start from PropertyDrawer.OnGUI or PropertyDrawer.GetPropertyHeight. 
		/// In this case it's important we DO NOT call SerializedObject.Update() or it can mess up other PropertyDrawers
		/// </summary>
		public bool TryGetInline(out SerializedObject obj, out SerializedProperty list) => TryGetInternal(out obj, out list, false);

		/// <summary>
		/// Use this TryGet for functions which are executed asynchronously from ReorderableList call backs. 
		/// In this case we must call SerializedObject.Update() to make sure it is up to date
		/// </summary>
		public bool TryGetAsync(out SerializedObject obj, out SerializedProperty list) => TryGetInternal(out obj, out list, true);

		private bool TryGetInternal(out SerializedObject obj, out SerializedProperty list, bool update)
		{
			if (m_Object == null)
			{
				obj = null;
				list = null;
				return false;
			}
			if (update)
			{
				// Try/Catch because SerializedObject seems to randomly get into a mysterious state
				// where it's not null but trying to do anything with it causes exceptions
				try
				{
					m_Object.UpdateIfRequiredOrScript();
				}
				catch (System.Exception e)
				{
					Debug.LogWarning($"ReorderableListProperties.TryGet() SerializedObject {m_ListPath} was corrupted and is no longer valid\n{e}");
					obj = null;
					list = null;
					return false;
				}
			}
			try
			{
				obj = m_Object;
				list = obj.FindProperty(m_ListPath);
				return list != null;
			}
			catch (System.Exception e)
			{
				Debug.LogWarning($"ReorderableListProperties.TryGet() SerializedProperty {m_ListPath} was corrupted and is no longer valid\n{e}");
				obj = null;
				list = null;
				return false;
			}
		}
	}

	public class ReorderableListDrawerCache : Dictionary<(int, string), ReorderableListDrawer>
	{
		private static ReorderableListDrawerCache s_Cache = new();

		public static ReorderableListDrawer Get(SerializedProperty property, GUIContent label)
		{
			(int, string) key = GetKey(property);
			if (TryGetCache(key, out ReorderableListDrawer value))
			{
				return value;
			}
			value = new ReorderableListDrawer(property, label);
			Set(key, value);
			return value;
		}

		public static (int, string) GetKey(SerializedProperty property)
		{
			return (property.serializedObject.targetObject.GetInstanceID(), property.propertyPath);
		}

		public static bool TryGetCache<T>((int, string) key, out T list) where T : ReorderableListDrawer
		{
			if (!s_Cache.TryGetValue(key, out ReorderableListDrawer value))
			{
				list = null;
				return false;
			}
			if (!value.Properties.IsValid())
			{
				list = null;
				return false;
			}
			list = value as T;
			return list != null;
		}

		public static void Set((int, string) key, ReorderableListDrawer value)
		{
			s_Cache[key] = value;
		}
	}
}
#endif
