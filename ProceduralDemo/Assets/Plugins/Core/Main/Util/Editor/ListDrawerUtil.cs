using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Core
{
	public static class ListDrawerUtil
	{
		private static readonly float SPACE = 4.0f;

		public static void OnGUI(
			Rect position,
			string label,
			SerializedProperty list)
		{
			OnGUIInternal(
				position,
				label,
				list,
				false,
				AddToArray);
		}

		public static void OnGUI(
			Rect position,
			string label,
			SerializedProperty list,
			List<System.Type> types,
			List<string> typeNames,
			string removeSuffix = null)
		{
			OnGUIInternal(
				position,
				label,
				list,
				true,
				AddToSerializedReferenceArray,
				new AddToSerializedReferenceArrayConfig { Names = typeNames, Types = types, },
				GetNameForSerializedRefence,
				string.IsNullOrEmpty(removeSuffix) ? null : new RemoveSuffix { Suffix = removeSuffix });
		}

		private static void OnGUIInternal(
			Rect position,
			string label,
			SerializedProperty list,
			bool drawBGs,
			System.Action<Rect, SerializedProperty, object> addToArray,
			object addToArrayConfig = null,
			System.Func<SerializedProperty, object, string> getElementName = null,
			object getNameConfig = null)
		{
			if (list == null)
			{
				return;
			}
			Rect r = position;
			r.height = EditorGUIUtility.singleLineHeight;
			if (drawBGs)
			{
				EditorGUI.DrawRect(r, 0.5f * Color.white);
			}

			Rect r2 = r;
			r2.width = 20.0f;
			r.width -= r2.width;
			r2.x += r.width;

			GUIStyle style = new(GUI.skin.label);
			style.alignment = TextAnchor.MiddleCenter;
			style.fontSize = 16;

			list.isExpanded = !EditorGUI.Foldout(r, !list.isExpanded, " " + label);
			if (list.isExpanded)
			{
				return;
			}
			EditorGUI.LabelField(r2, "\uE09D", style);
			addToArray.Invoke(r2, list, addToArrayConfig);

			r.y += r.height;
			r2.y += r.height;
			r = UnimaDrawerUtil.AddIndent(r);
			r.y += SPACE;
			r2.y += SPACE;

			if (list.arraySize == 0)
			{
				EditorGUI.HelpBox(r, "Empty", MessageType.Info);
			}

			int removeIndex = -1;
			for (int i = 0; i < list.arraySize; i++)
			{
				SerializedProperty element = list.GetArrayElementAtIndex(i);
				float totalHeight = EditorGUI.GetPropertyHeight(element, true);
				Rect bgRect = r;
				bgRect.width += r2.width;
				bgRect.height = totalHeight;
				if (drawBGs)
				{
					EditorGUI.DrawRect(bgRect, 0.5f * Color.white);
				}

				if (GUI.Button(r2, "\uE09F", style))
				{
					removeIndex = i;
				}

				element = list.GetArrayElementAtIndex(i);

				r.height = totalHeight;
				string name = getElementName == null ? element.displayName : getElementName.Invoke(element, getNameConfig);
				if (string.IsNullOrEmpty(name))
				{
					EditorGUI.LabelField(r, new GUIContent("INVALID"));
				}
				else
				{
					EditorGUI.PropertyField(r, element, new GUIContent(" " + name), true);
				}
				r.y += r.height;
				r2.y += r.height;
				r.y += SPACE;
				r2.y += SPACE;
			}
			if (removeIndex >= 0)
			{
				// When an object reference is assigned first delete will set it none,
				// and the second will remove the array element
				int size = list.arraySize;
				list.DeleteArrayElementAtIndex(removeIndex);
				if (size == list.arraySize)
				{
					list.DeleteArrayElementAtIndex(removeIndex);
				}
			}
		}

		public static float GetArrayHeight(SerializedProperty list)
		{
			if (list == null)
			{
				return 0.0f;
			}
			float height = EditorGUIUtility.singleLineHeight;
			if (list.isExpanded)
			{
				return height;
			}
			height += SPACE;
			int count = list.arraySize;
			if (count == 0)
			{
				height += EditorGUIUtility.singleLineHeight + SPACE;
			}
			for (int i = 0; i < count; i++)
			{
				SerializedProperty element = list.GetArrayElementAtIndex(i);
				float totalHeight = EditorGUI.GetPropertyHeight(element, true);
				height += totalHeight;
				height += SPACE;
			}
			return height;
		}

		private static void AddToArray(Rect r, SerializedProperty list, object args)
		{
			if (GUI.Button(r, GUIContent.none, GUIStyle.none))
			{
				list.arraySize++;
			}
		}

		private class AddToSerializedReferenceArrayConfig
		{
			public List<System.Type> Types;
			public List<string> Names;
		}

		private static void AddToSerializedReferenceArray(Rect r, SerializedProperty list, object args)
		{
			if (!(args is AddToSerializedReferenceArrayConfig config))
			{
				return;
			}
			int index = EditorGUI.Popup(r, 0, config.Names.ToArray(), GUIStyle.none);
			if (index > 0)
			{
				list.InsertArrayElementAtIndex(0);
				SerializedProperty ele = list.GetArrayElementAtIndex(0);
				ele.managedReferenceValue = System.Activator.CreateInstance(config.Types[index]);
			}
		}

		private interface IRemoveSuffix
		{
			string Suffix { get; }
		}

		private class RemoveSuffix : IRemoveSuffix
		{
			public string Suffix { get; set; }
		}

		private static string GetNameForSerializedRefence(SerializedProperty element, object getNameConfig)
		{
			if (string.IsNullOrEmpty(element.managedReferenceFullTypename))
			{
				return null;
			}
			string[] typeStrings = element.managedReferenceFullTypename.Split(' ', '.'); // Remove assembly name and namespace
			string name = typeStrings[^1];
			if (getNameConfig is IRemoveSuffix removeSuffix && name.EndsWith(removeSuffix.Suffix))
			{
				name = name.Substring(0, name.Length - removeSuffix.Suffix.Length);
			}
			return name;
		}
	}
}
