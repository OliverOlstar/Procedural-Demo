using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(StaticDataConfig))]
public class StaticDataConfigEditor : Editor
{
	private SerializedProperty m_SheetNamesProperty;
	private ReorderableList m_SheetNamesList;
	private bool m_Foldout = false;

	void OnEnable()
	{
		m_SheetNamesProperty = serializedObject.FindProperty("m_ServerDataSheetNames");
		m_SheetNamesList = new ReorderableList(serializedObject, m_SheetNamesProperty, true, true, true, true)
		{
			drawElementCallback = DrawSheetElementCallback,
			drawHeaderCallback = DrawSheetHeaderCallback,
		};
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		serializedObject.Update();
		EditorGUILayout.Space();
		m_Foldout = EditorGUILayout.Foldout(m_Foldout, m_SheetNamesProperty.displayName);
		if (m_Foldout)
		{
			m_SheetNamesList.DoLayoutList();
		}
		if (serializedObject.hasModifiedProperties)
		{
			serializedObject.ApplyModifiedProperties();
		}
	}

	private void DrawSheetElementCallback(Rect rect, int index, bool isActive, bool isFocused)
	{
		rect.y += 2f;
		rect.height -= 4f;
		SerializedProperty element = m_SheetNamesProperty.GetArrayElementAtIndex(index);
		EditorGUI.PropertyField(rect, element);
	}

	private void DrawSheetHeaderCallback(Rect rect)
	{
		EditorGUI.LabelField(rect, "Server Sheet Names");
	}
}
