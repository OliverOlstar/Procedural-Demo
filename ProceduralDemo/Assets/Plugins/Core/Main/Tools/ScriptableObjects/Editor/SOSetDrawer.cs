using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Core
{
	[CustomPropertyDrawer(typeof(SOSetBase), true)]
	public class SOSetDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			System.Type baseType = fieldInfo.FieldType.BaseType;
			while (baseType != null && !baseType.IsGenericType)
			{
				baseType = baseType.BaseType;
			}
			if (baseType == null)
			{
				return;
			}

			SerializedProperty set = property.FindPropertyRelative("m_Set");

			System.Type genericType = baseType.GetGenericArguments()[0];
			List<string> paths = new(AssetDatabaseUtil.Find(genericType));
			if (paths.Count == 0)
			{
				AssetPickerUtilityMenu.TryAttachMenu(ref position, set, genericType, null, OnSelectedObject);
				EditorGUI.Popup(position, label.text, 0, new string[] { "None" });
				return;
			}

			string[] names = new string[paths.Count];
			for (int i = 0; i < paths.Count; i++)
			{
				names[i] = Path.GetFileNameWithoutExtension(paths[i]);
			}

			int setSize = set.arraySize;
			if (setSize > 0 && UberPickerGUI.AttachAssetSelectButton(ref position))
			{
				EditorGUIUtility.PingObject(set.GetArrayElementAtIndex(0).objectReferenceValue);
			}
			AssetPickerUtilityMenu.TryAttachMenu(ref position, set, genericType, null, OnSelectedObject);

			int selectedMask = 0;
			for (int i = 0; i < setSize; i++)
			{
				SerializedProperty element = set.GetArrayElementAtIndex(i);
				string path = AssetDatabase.GetAssetPath(element.objectReferenceValue);
				int index = paths.IndexOf(path);
				if (index >= 0)
				{
					selectedMask |= 1 << index;
				}
			}

			int newMask = EditorGUI.MaskField(position, label, selectedMask, names);
			if (newMask != selectedMask)
			{
				set.ClearArray();
				for (int i = 0; i < paths.Count; i++)
				{
					if (((1 << i) & newMask) != 0)
					{
						set.arraySize++;
						SerializedProperty element = set.GetArrayElementAtIndex(set.arraySize - 1);
						element.objectReferenceValue = AssetDatabase.LoadAssetAtPath(paths[i], genericType);
					}
				}
			}
		}

		private static void OnSelectedObject(SerializedProperty set, string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return;
			}
			set.arraySize++;
			SerializedProperty element = set.GetArrayElementAtIndex(set.arraySize - 1);
			element.objectReferenceValue = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
			set.serializedObject.ApplyModifiedProperties();
		}
	}
}
