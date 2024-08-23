using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core
{
	[CustomPropertyDrawer((typeof(ScriptableObjectDictionaryForEditor)), true)]
	public class ScriptableObjectDictionaryPropertyDrawer : PropertyDrawer
	{
		private static readonly float VERTICAL_OFFSET = 2.0f;
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			position.height = EditorGUIUtility.singleLineHeight;
			position.y += VERTICAL_OFFSET;
			EditorGUI.DrawRect(position, 0.4f * Color.white);
			Rect labelPos = position;
			labelPos.width = EditorGUIUtility.labelWidth;
			SerializedProperty so = prop.FindPropertyRelative("key");
			SerializedProperty go = prop.FindPropertyRelative("value");
			if (so.objectReferenceValue == null)
			{
				EditorGUI.LabelField(position, "Missing");
			}
			else if (GUI.Button(labelPos, so.objectReferenceValue.name + " " + UberPickerGUI.POINTER_UNICODE, GUI.skin.label))
			{
				Selection.activeObject = so.objectReferenceValue;
				//EditorGUI.ObjectField(position, so.objectReferenceValue, so.objectReferenceValue.GetType(), false);
			}
			position.x += labelPos.width;
			position.width -= labelPos.width;
			EditorGUI.ObjectField(position, go, GUIContent.none);
		}
			
		public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight + 2.0f * VERTICAL_OFFSET;
		}
	}
		
	[CustomPropertyDrawer(typeof(AutoDictionaryForEditor), true)]
	public class AutoDictionaryPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			Type[] genericArgs = fieldInfo.FieldType.BaseType.GetGenericArguments();
			if (genericArgs == null || genericArgs.Length == 0)
			{
				return;
			}

			// Find all objects of the lists key type
			Type type = genericArgs[1];
			List<Object> objs = new();
			AssetDatabaseUtil.LoadAll(type, objs);

			// Clean up list and make sure each entry is keyed by a valid scriptable object
			SerializedProperty list = prop.FindPropertyRelative("serializedList");
			for (int i = list.arraySize - 1; i >= 0; i--)
			{
				SerializedProperty element = list.GetArrayElementAtIndex(i);
				SerializedProperty so = element.FindPropertyRelative("key");
				if (so.objectReferenceValue == null)
				{
					list.DeleteArrayElementAtIndex(i); // Key must have been deleted, remove it
					continue;
				}
				// Mark the key as found
				for (int j = objs.Count - 1; j >= 0; j--)
				{
					if (objs[j].GetInstanceID() == so.objectReferenceValue.GetInstanceID())
					{
						objs.RemoveAt(j);
						break;
					}
				}
			}
			// Add an entry for each object that wasn't found
			foreach (Object obj in objs)
			{
				list.arraySize++;
				SerializedProperty el = list.GetArrayElementAtIndex(list.arraySize - 1);
				SerializedProperty so = el.FindPropertyRelative("key");
				so.objectReferenceValue = obj;
			}

			// Actually draw the inspector
			Rect labelpos = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			labelpos.x += 16.0f * (EditorGUI.indentLevel - 1);
			GUI.Label(labelpos, prop.displayName, EditorStyles.boldLabel);

			float y = position.y + labelpos.height;

			for (int i = 0; i < list.arraySize; ++i)
			{
				SerializedProperty element = list.GetArrayElementAtIndex(i);
				float height = EditorGUI.GetPropertyHeight(element);
				labelpos.height += height;
				Rect r2 = new(position.x, y, position.width, height);
				EditorGUI.PropertyField(r2, element);
				y += height;
			}
		}
	
		public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
		{
			float height = EditorGUIUtility.singleLineHeight;
			SerializedProperty list = prop.FindPropertyRelative("serializedList");
			for (int i = 0; i < list.arraySize; ++i)
			{
				SerializedProperty element = list.GetArrayElementAtIndex(i);
				float h = EditorGUI.GetPropertyHeight(element);
				height += h;
			}
			height += 2.0f;
			return height;
		}
	}
}
