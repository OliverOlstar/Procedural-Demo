using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Core
{
	[CustomPropertyDrawer(typeof(ListableBase), true)]
	public class ListablePropertyDrawer : PropertyDrawer
	{
		private static readonly float BUTTON_WIDTH = 14.0f;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty list = property.FindPropertyRelative("m_List");
			if (list.arraySize > 1)
			{
				DrawList(position, property, list, label);
			}
			else
			{
				DrawSingle(position, property, list, label);
			}
		}

		private void DrawList(Rect position, SerializedProperty property, SerializedProperty list, GUIContent label)
		{
			if (PropertyDrawerUtil.HasAttribute<ListableStyle.List.ReorderableAttribute>(fieldInfo))
			{
#if UNITY_2021_1_OR_NEWER // ReorderableList API requires 2021
				ReorderableListDrawer drawer = ReorderableListDrawerCache.Get(property, label);
				drawer.OnGUI(position, property, label);
#endif
			}
			else
			{
				EditorGUI.PropertyField(position, list, label, true);
			}
		}

		private void DrawSingle(Rect position, SerializedProperty property, SerializedProperty prop, GUIContent label)
		{
			bool allowEmpty = PropertyDrawerUtil.HasAttribute<ListableStyle.AllowEmptyAttribute>(fieldInfo);
			if (!allowEmpty && prop.arraySize == 0)
			{
				Debug.LogWarning($"ListablePropertyDrawer.OnGUI() {prop.propertyPath} is not allowed to be empty but has an array size of 0, adding a new element to the list");
				prop.arraySize = 1;
			}

			if (PropertyDrawerUtil.TryGetAttribute(fieldInfo, out ListableSingleStyleAttribute att))
			{
				switch (att)
				{
					case ListableStyle.Single.ArrayAttribute arrayAtt:
						DrawSingleArrayStyle(position, prop, label, allowEmpty);
						return;
					case ListableStyle.Single.ReorderableAttribute reorderableAtt:
						DrawSingleReorderableStyle(position, prop, label, allowEmpty);
						return;
					case ListableStyle.Single.DropdownAttribute invisAtt:
						DrawSingleInvisibleStyle(position, prop, label, allowEmpty);
						return;
				}
			}
			DrawSingleDefaultStyle(position, prop, label, allowEmpty);
		}

		private void DrawSingleDefaultStyle(Rect position, SerializedProperty prop, GUIContent label, bool allowEmpty)
		{
			if (prop.arraySize == 0 && allowEmpty)
			{
				Rect r1 = position;
				r1.width = EditorGUIUtility.labelWidth;
				Rect r2 = position;
				r2.xMin = r1.xMax;
				r2.xMax -= BUTTON_WIDTH;
				EditorGUI.LabelField(r1, label, EditorStyles.boldLabel);
				PropertyDrawerUtil.TryApplyTooltipIcon(r1, label);
				GUI.enabled = false;
				EditorGUI.HelpBox(r2, "None", MessageType.None);
				GUI.enabled = true;
			}

			Rect headerRect = position;
			headerRect.height = GetHeaderHeight();

			Rect buttonRect = headerRect;
			buttonRect.xMin = headerRect.xMax - BUTTON_WIDTH;
			
			GUIStyle BUTTON_STYLE = new GUIStyle(GUI.skin.label);
			BUTTON_STYLE.fontSize = 18;
			BUTTON_STYLE.alignment = TextAnchor.MiddleCenter;
			BUTTON_STYLE.fontStyle = FontStyle.Bold;

			if (GUI.Button(buttonRect, "\ue09d", BUTTON_STYLE))
			{
				prop.arraySize++;
				prop.isExpanded = true;
			}

			if (prop.arraySize == 0)
			{
				return;
			}

			if (allowEmpty)
			{
				buttonRect.x -= buttonRect.width;
				if (GUI.Button(buttonRect, "\uE09F", BUTTON_STYLE))
				{
					prop.arraySize = 0;
					return;
				}
			}

			DrawElement(headerRect, position, prop.GetArrayElementAtIndex(0), label, EditorStyles.boldLabel);
		}

		private float GetHeaderHeight() => EditorGUIUtility.singleLineHeight + 2; // This extra header size is for the Reorderable style, but it's easiest if all styles are the same
		private const float REORDERABLE_HEADER_BUFFER = 6;

		private void DrawSingleReorderableStyle(Rect position, SerializedProperty prop, GUIContent label, bool allowEmpty)
		{
			Rect headerRect = position;
			headerRect.height = GetHeaderHeight();

			EditorGUI.HelpBox(position, string.Empty, MessageType.None);
			Color bgColor = GUI.backgroundColor;
			GUI.backgroundColor = Color.black;
			EditorGUI.HelpBox(headerRect, string.Empty, MessageType.None);
			GUI.backgroundColor = bgColor;

			Rect buttonRect = headerRect;
			buttonRect.xMin = headerRect.xMax - BUTTON_WIDTH;

			buttonRect.x -= REORDERABLE_HEADER_BUFFER;
			headerRect.xMin += REORDERABLE_HEADER_BUFFER;

			GUIStyle BUTTON_STYLE = new GUIStyle(GUI.skin.label);
			BUTTON_STYLE.fontSize = 18;
			BUTTON_STYLE.alignment = TextAnchor.MiddleCenter;
			BUTTON_STYLE.fontStyle = FontStyle.Bold;

			if (GUI.Button(buttonRect, "\ue09d", BUTTON_STYLE))
			{
				prop.arraySize++;
				prop.isExpanded = true;
			}

			if (prop.arraySize == 0)
			{
				EditorGUI.LabelField(headerRect, label);
				PropertyDrawerUtil.TryApplyTooltipIcon(headerRect, label);

				position.yMin += headerRect.height - 2;
				position = PropertyDrawerUtil.IndentRect(position);
				GUI.enabled = false;
				EditorGUI.LabelField(position, "List is Empty");//, use '+' button to add items");
				GUI.enabled = true;

				// Bellow style only used one in the empty list case, might want to switch back to this
				//Rect r2 = headerRect;
				//r2.xMin += EditorGUIUtility.labelWidth;
				//r2.xMax -= BUTTON_WIDTH;
				//GUI.enabled = false;
				//EditorGUI.LabelField(r2, "List is empty, use '+' button to add items");
				//GUI.enabled = true;
				return;
			}

			if (allowEmpty)
			{
				buttonRect.x -= buttonRect.width;
				if (GUI.Button(buttonRect, "\uE09F", BUTTON_STYLE))
				{
					prop.arraySize = 0;
					return;
				}
			}

			position.xMax -= REORDERABLE_HEADER_BUFFER;
			DrawElement(headerRect, position, prop.GetArrayElementAtIndex(0), label, EditorStyles.label);
		}

		private void DrawSingleArrayStyle(Rect position, SerializedProperty prop, GUIContent label, bool allowEmpty)
		{
			Rect headerRect = position;
			headerRect.height = GetHeaderHeight();

			GUI.Box(headerRect, GUIContent.none);

			Rect sizeRect = headerRect;
			sizeRect.xMin = headerRect.xMax - 50;
			int newSize = EditorGUI.IntField(sizeRect, prop.arraySize);
			if (newSize != prop.arraySize)
			{
				prop.arraySize = Mathf.Max(newSize, allowEmpty ? 0 : 1);
			}

			Rect buttonRect = headerRect;
			buttonRect.width = BUTTON_WIDTH;
			buttonRect.x = headerRect.x;
			headerRect.xMin += 15;
			
			GUIStyle BUTTON_STYLE = new GUIStyle(GUI.skin.label);
			BUTTON_STYLE.fontSize = 18;
			BUTTON_STYLE.alignment = TextAnchor.MiddleCenter;
			BUTTON_STYLE.fontStyle = FontStyle.Bold;
			
			GUIStyle BUTTON_STYLE_GREY = new GUIStyle(BUTTON_STYLE);
			BUTTON_STYLE_GREY.normal.textColor = Color.grey;
			
			if (GUI.Button(buttonRect, "\ue09d", BUTTON_STYLE_GREY))
			{
				prop.arraySize++;
				prop.isExpanded = true;
			}

			if (prop.arraySize == 0)
			{
				EditorGUI.LabelField(headerRect, label, EditorStyles.boldLabel);
				PropertyDrawerUtil.TryApplyTooltipIcon(headerRect, label);
			}
			else
			{
				DrawElement(headerRect, position, prop.GetArrayElementAtIndex(0), label, EditorStyles.boldLabel);
			}
		}

		private void DrawElement(Rect headerRect, Rect position, SerializedProperty item, GUIContent labelContent, GUIStyle labelStyle)
		{
			if (item.hasVisibleChildren)
			{
				EditorGUI.LabelField(headerRect, labelContent, labelStyle);
				PropertyDrawerUtil.TryApplyTooltipIcon(headerRect, labelContent);
				position.yMin += headerRect.height;
				position = PropertyDrawerUtil.IndentRect(position);
				EditorGUI.PropertyField(position, item, true);
			}
			else
			{
				EditorGUI.PropertyField(position, item, labelContent, true);
			}
		}

		public enum DropdownOptions
		{
			Single = 0,
			List,
		}
		public enum DropdownOptionsWithEmpty
		{
			None = 0,
			Single,
			List,
		}
		private void DrawSingleInvisibleStyle(Rect position, SerializedProperty list, GUIContent label, bool allowEmpty)
		{
			Rect headerRect = position;
			headerRect.height = GetHeaderHeight();
			position.yMin += headerRect.height;
			FlattenPropertyDrawer.OnGUI(position, list.GetArrayElementAtIndex(0), label, false, false, false);

			bool isNowList = false;
			if (allowEmpty)
			{
				DropdownOptionsWithEmpty currentSelected = list.arraySize == 0 ? DropdownOptionsWithEmpty.None :
					list.arraySize == 1 ? DropdownOptionsWithEmpty.Single :
					DropdownOptionsWithEmpty.List;
				DropdownOptionsWithEmpty newSelected = (DropdownOptionsWithEmpty)EditorGUI.EnumPopup(headerRect, label, currentSelected);
				if (currentSelected != newSelected)
				{
					switch (newSelected)
					{
						case DropdownOptionsWithEmpty.None:
							list.arraySize = 0;
							break;
						case DropdownOptionsWithEmpty.Single:
							list.arraySize = 1;
							break;
						case DropdownOptionsWithEmpty.List:
							list.arraySize = Mathf.Max(list.arraySize, 2);
							isNowList = true;
							break;
					}
				}
			}
			else
			{
				DropdownOptions currentSelected = list.arraySize > 1 ? DropdownOptions.List : DropdownOptions.Single;
				DropdownOptions newSelected = (DropdownOptions)EditorGUI.EnumPopup(headerRect, label, currentSelected);
				if (currentSelected != newSelected)
				{
					switch (newSelected)
					{
						case DropdownOptions.Single:
							list.arraySize = 1;
							break;
						case DropdownOptions.List:
							list.arraySize = Mathf.Max(list.arraySize, 2);
							isNowList = true;
							break;
					}
				}
			}

			// !! WEIRD THING ALERT !!
			// Expanding list items when we convert to list seems like a cool quality of life thing, but it actually serves a nefarious purpose...
			// If our list items contain nested SerialRefListDrawers they need a chance to run SerializedReferenceEditorUtil.FixUp() immediately
			// this happens naturally when they draw their GUI, but not if the list item is collapsed
			// Depending on the order list items are expanded if they haven't run FixUp yet it can cause crashes
			// This issue was discovered in EQS.QueryList
			// Update: I couldn't fix the crashes... I give up, but I'm leaving this code here for now
			if (isNowList)
			{
				for (int i = 0; i < list.arraySize; i++)
				{
					list.GetArrayElementAtIndex(i).isExpanded = true;
				}
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			SerializedProperty prop = property.FindPropertyRelative("m_List");
			switch (prop.arraySize)
			{
				case 0:
					float emptyHeight = GetHeaderHeight();
					if (PropertyDrawerUtil.HasAttribute<ListableStyle.Single.ReorderableAttribute>(fieldInfo))
					{
						emptyHeight += 1.25f * EditorGUIUtility.singleLineHeight;
					}
					return emptyHeight;
				case 1:
					float singleHeight = 2.0f;
					SerializedProperty item = prop.GetArrayElementAtIndex(0);
					if (item.hasVisibleChildren)
					{
						singleHeight += GetHeaderHeight();
					}
					singleHeight += EditorGUI.GetPropertyHeight(item, true);
					return singleHeight;
				default:
					float listHeight = 0.0f;
					if (PropertyDrawerUtil.HasAttribute<ListableStyle.List.ReorderableAttribute>(fieldInfo))
					{
#if UNITY_2021_1_OR_NEWER // ReorderableList API requires 2021
						ReorderableListDrawer drawer = ReorderableListDrawerCache.Get(property, label);
						listHeight += drawer.GetPropertyHeight(property);
#endif
					}
					else
					{
						listHeight += EditorGUI.GetPropertyHeight(prop, true);
					}
					return listHeight;
			}
		}
	}
}
