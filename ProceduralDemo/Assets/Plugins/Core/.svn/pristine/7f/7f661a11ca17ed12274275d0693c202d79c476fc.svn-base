using UnityEditor;
using UnityEngine;

namespace Data
{
	public class DataDocumentationSettingsProvider : SettingsProvider
	{
		public const string Path = "Project/ECG/Data Documentation";

		[SettingsProvider]
		private static SettingsProvider Get() => new DataDocumentationSettingsProvider();

		private SerializedObject m_SerializedAsset;
		private SerializedProperty m_DatabaseDocuments;
		private SerializedProperty m_OutputPath;
		private SerializedProperty m_ExporterType;

		private DataDocumentationSettingsProvider()
			: base(Path, SettingsScope.Project)
		{
			label = "Data Documentation";
			m_SerializedAsset = new SerializedObject(DataDocumentationSettings.Asset);
			m_DatabaseDocuments = m_SerializedAsset.FindProperty("m_DatabaseDocuments");
			m_OutputPath = m_SerializedAsset.FindProperty("m_OutputPath");
			m_ExporterType = m_SerializedAsset.FindProperty("m_ExporterType");
		}

		public override void OnGUI(string searchContext)
		{
			m_SerializedAsset.Update();
			DrawDatabaseDocuments();
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(m_OutputPath);
			EditorGUILayout.PropertyField(m_ExporterType);
			if (m_SerializedAsset.hasModifiedProperties)
			{
				m_SerializedAsset.ApplyModifiedProperties();
			}
			if (GUILayout.Button("Export", GUILayout.Width(100f)))
			{
				DataDocumentationSettings.Asset.Export();
			}
		}

		private void DrawDatabaseDocuments()
		{
			if (!EditorGUILayout.PropertyField(m_DatabaseDocuments, false))
			{
				return;
			}
			for (int i = 0; i < m_DatabaseDocuments.arraySize; ++i)
			{
				using (new EditorGUI.IndentLevelScope())
				{
					DrawDatabaseDocument(m_DatabaseDocuments.GetArrayElementAtIndex(i));
				}
			}
		}

		private void DrawDatabaseDocument(SerializedProperty document)
		{
			if (!EditorGUILayout.PropertyField(document, false))
			{
				return;
			}
			using (new EditorGUI.IndentLevelScope())
			{
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.PropertyField(document.FindPropertyRelative("m_DatabaseType"));
				EditorGUI.EndDisabledGroup();
				SerializedProperty columns = document.FindPropertyRelative("m_Columns");
				if (!EditorGUILayout.PropertyField(columns, false))
				{
					return;
				}
				for (int i = 0; i < columns.arraySize; i++)
				{
					using (new EditorGUI.IndentLevelScope())
					{
						DrawColumnDocument(columns.GetArrayElementAtIndex(i));
					}
				}
			}
		}

		private void DrawColumnDocument(SerializedProperty column)
		{
			if (!EditorGUILayout.PropertyField(column, false))
			{
				return;
			}
			using (new EditorGUI.IndentLevelScope())
			{
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.PropertyField(column.FindPropertyRelative("m_ColumnType"), true);
				EditorGUILayout.PropertyField(column.FindPropertyRelative("m_Validation"), true);
				EditorGUILayout.PropertyField(column.FindPropertyRelative("m_SheetNotes"), true);
				EditorGUI.EndDisabledGroup();
				EditorGUILayout.PropertyField(column.FindPropertyRelative("m_ExtraNotes"), true);
			}
		}
	}
}
