
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ODev.Data.Editor
{
	[CustomEditor(typeof(DBBase), true)]
	public class DBEditor : UnityEditor.Editor
	{
		private static readonly int MAX_SIZE = 1000;
		private static readonly int PAGE_SIZE = 100;

		private readonly Dictionary<int, bool> m_Foldouts = new();

		public override void OnInspectorGUI()
		{
			SerializedProperty rawData = serializedObject.FindProperty("m_RawData");
			if (rawData == null)
			{
				return;
			}
			SerializedProperty array = rawData;
			if (!array.isArray)
			{
				array = rawData.FindPropertyRelative("m_Collections");
			}
			if (array == null || !array.isArray || array.arraySize < MAX_SIZE)
			{
				base.OnInspectorGUI();
				return;
			}
			int size = array.arraySize;
			int i = 0;
			int count = 0;
			while (count < size)
			{
				m_Foldouts.TryGetValue(i, out bool foldOut);
				int startIndex = i * PAGE_SIZE;
				int endIndex = Mathf.Min(startIndex + PAGE_SIZE, size);
				foldOut = EditorGUILayout.Foldout(foldOut, startIndex + " - " + endIndex);
				if (foldOut)
				{
					EditorGUI.indentLevel++;
					for (int j = startIndex; j < endIndex; j++)
					{
						SerializedProperty el = array.GetArrayElementAtIndex(j);
						EditorGUILayout.PropertyField(el, true);
					}
					EditorGUI.indentLevel--;
				}
				m_Foldouts[i] = foldOut;
				count += PAGE_SIZE;
				i++;
			}
			if (serializedObject.hasModifiedProperties)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}