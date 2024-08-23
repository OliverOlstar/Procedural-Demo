using UnityEngine;
using UnityEditor;

namespace Core
{
	[CustomPropertyDrawer(typeof(InspectorNotes))]
	public class InspectorNotesDrawer : PropertyDrawer
	{
		private static bool s_Edit = false;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Not sure if this looks better with a box or not
			//EditorGUI.HelpBox(position, "", MessageType.None);

			property = property.FindPropertyRelative("m_Notes");

			GUI.color = new Color32(99, 179, 85, 255);

			if (!s_Edit && string.IsNullOrEmpty(property.stringValue))
			{
				GUIStyle labelStyle = new(GUI.skin.label);
				labelStyle.fontSize = 10;
				if (GUI.Button(position, "// <Add " + label.text + " here>", labelStyle))
				{
					s_Edit = true;
				}
			}
			else
			{
				Rect x = position;
				x.width = 16.0f;
				position.xMin += x.width;

				if (GUI.Button(x, "", GUIStyle.none))
				{
					s_Edit = !s_Edit;
				}

				int count = Mathf.Max(property.stringValue.Split('\n').Length, 1);
				x.height /= count;
				for (int i = 0; i < count; i++)
				{
					GUI.Label(x, "//");
					x.y += x.height;
				}

				if (!s_Edit)
				{
					if (GUI.Button(position, "", GUIStyle.none))
					{
						s_Edit = true;
					}
					GUIStyle style = GUI.skin.label;
					// It'd be cool to get word wrapping working one day...
					//GUIStyle style = new GUIStyle(GUI.skin.label);
					//style.wordWrap = true;
					EditorGUI.LabelField(position, property.stringValue, style);
				}
				else
				{
					property.stringValue = EditorGUI.TextArea(position, property.stringValue);
				}
			}

			GUI.color = Color.white;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			property = property.FindPropertyRelative("m_Notes");

			if (string.IsNullOrEmpty(property.stringValue))
			{
				return EditorGUIUtility.singleLineHeight - 2.0f;
			}
			float height = 0.0f;
			GUIStyle style;
			if (s_Edit)
			{
				style = EditorStyles.textArea;
			}
			else
			{
				style = GUI.skin.label;
				// It'd be cool to get word wrapping working one day...
				//style = new GUIStyle(GUI.skin.label);
				//style.wordWrap = true;
			}
			Vector2 size = style.CalcSize(new GUIContent(property.stringValue));
			height += size.y;
			return height;
		}
	}
}
