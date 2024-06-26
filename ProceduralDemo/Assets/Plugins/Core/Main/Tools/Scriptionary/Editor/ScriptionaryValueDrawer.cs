using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Scriptionary
{
	[CustomPropertyDrawer(typeof(ScriptionaryItemReferenceBase))]
	public class ScriptionaryValueDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty sDict = property.FindPropertyRelative("m_Scriptionary");
			if (!(sDict.objectReferenceValue is SOScriptionaryBase scriptionary))
			{
				EditorGUI.PropertyField(position, sDict, label, true);
				return;
			}

			SerializedProperty sKey = property.FindPropertyRelative("m_Key");
			int selected = -1;
			List<GUIContent> keys = new();
			foreach (IScriptionaryItem item in scriptionary._EditorGetItems())
			{
				if (sKey.stringValue == item.Key)
				{
					selected = keys.Count;
				}
				keys.Add(new GUIContent(item.Key));
			}

			if (selected < 0)
			{
				UberPickerGUI.AttachNullWarning(ref position);
				string s = !string.IsNullOrEmpty(sKey.stringValue) ?
					"?" + sKey.stringValue + "?" :
					"";
				keys.Insert(0, new GUIContent(s));
				selected = 0;
			}

			Rect p1 = position;
			p1.width *= 0.7f;
			Rect p2 = position;
			p2.xMin = p1.xMax;

			Core.PropertyDrawerUtil.TryApplyTooltipIcon(p1, label);

			int newSelection = EditorGUI.Popup(p1, label, selected, keys.ToArray());
			if (newSelection != selected)
			{
				sKey.stringValue = keys[newSelection].text;
			}
			EditorGUI.PropertyField(p2, sDict, GUIContent.none, true);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}
	}
}
