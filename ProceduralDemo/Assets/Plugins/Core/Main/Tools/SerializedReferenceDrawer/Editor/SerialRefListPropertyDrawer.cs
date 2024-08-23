#if UNITY_2021_1_OR_NEWER
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;

namespace Core
{
	[CustomPropertyDrawer(typeof(SerialRefList<>), true)]
	public class SerialRefListPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerialRefListDrawer list = SerialRefListDrawerCache.Get(property, label, fieldInfo);
			list.OnGUI(position, property, label);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			SerialRefListDrawer list = SerialRefListDrawerCache.Get(property, label, fieldInfo);
			float height = list.GetPropertyHeight(property);
			return height;
		}
	}

	public class SerialRefListDrawer : ReorderableListDrawer
	{
		private System.Type m_RefType = null;

		public SerialRefListDrawer(SerializedProperty property, FieldInfo fieldInfo, GUIContent label) :
			base(property, label)
		{
			System.Type baseClassType = typeof(ReorderableListBase<>);
			System.Type recurseType = fieldInfo.FieldType;
			do
			{
				recurseType = recurseType.BaseType;
			}
			while (recurseType.Name != baseClassType.Name);
			m_RefType = recurseType.GenericTypeArguments[0];

			m_ReorderableList.onAddDropdownCallback = OnAddDropdownCallback;
		}

		//
		// I don't know what to do about this problem, fed up with trying to fix it, committing this code snipet in case I try to solve it again someday
		// Basically add an element (duplicating) some highly nested lists of lists of SerializedReferences crashes Unity
		//
		//public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		//{
		//	// Trying to recover from a weird case... If a list element contains a SerialRefList and it gets duplicated then
		//	// all the managed objects in the list in the duplicated element are duplicated as well.
		//	// I don't know if we should even support this kind of thing... but I guess leave this code here for now?
		//	SerializedProperty listProperty = property.FindPropertyRelative(ReorderableListDrawerProperties.LIST_PROPERTY_NAME);
		//	for (int i = 0; i < listProperty.arraySize; i++)
		//	{
		//		SerializedProperty el = listProperty.GetArrayElementAtIndex(i);
		//		if (el.managedReferenceValue != null)
		//		{
		//			bool fixedUp = SerializedReferenceEditorUtil.FixUp(el, el.managedReferenceValue.GetType());
		//			if (fixedUp)
		//			{
		//				return;
		//			}
		//		}
		//	}
		//	base.OnGUI(position, property, label);
		//}

		protected override void DrawElement(Rect position, SerializedProperty element)
		{
			if (element.managedReferenceValue == null)
			{
				EditorGUI.HelpBox(position, "NULL", MessageType.Error);
				return;
			}
			GUIContent label = new(element.managedReferenceValue.GetType().Name);
			EditorGUI.PropertyField(position, element, label, true);
		}

		private void OnAddDropdownCallback(Rect buttonRect, ReorderableList reorderableList)
		{
			SerializedReferenceEditorUtil.Cache cache = SerializedReferenceEditorUtil.Cache.Get(m_RefType);
			GenericMenu menu = new();
			for (int i = 0; i < cache.TypeNames.Length; i++)
			{
				menu.AddItem(new GUIContent(cache.TypeNames[i]), false, OnAdd, cache.Types[i]);
			}
			menu.ShowAsContext();
		}

		private void OnAdd(object context)
		{
			if (!m_Properties.TryGetAsync(out SerializedObject obj, out SerializedProperty list))
			{
				Debug.LogError("SerialRefReorderableList.OnAdd() IsValid sanity check failed");
				return;
			}
			list.arraySize++;
			SerializedProperty newElement = list.GetArrayElementAtIndex(list.arraySize - 1);
			newElement.managedReferenceValue = System.Activator.CreateInstance(context as System.Type);
			if (obj.hasModifiedProperties)
			{
				obj.ApplyModifiedProperties();
			}
		}
	}

	public class SerialRefListDrawerCache : ReorderableListDrawerCache
	{
		public static SerialRefListDrawer Get(SerializedProperty property, GUIContent label, FieldInfo fieldInfo)
		{
			(int, string) key = GetKey(property);
			if (TryGetCache(key, out SerialRefListDrawer value))
			{
				return value;
			}
			value = new SerialRefListDrawer(property, fieldInfo, label);
			Set(key, value);
			return value;
		}
	}
}
#endif
