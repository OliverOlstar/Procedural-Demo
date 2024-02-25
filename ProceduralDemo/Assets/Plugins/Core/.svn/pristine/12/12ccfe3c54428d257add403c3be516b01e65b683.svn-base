using Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	[CoreEditor.EditorSettings(ASSET_PATH)]
	public class DataDocumentationSettings : CoreEditor.EditorSettings<DataDocumentationSettings>
	{
		[ContextMenu("Test")]
		void Test()
		{
			Asset.MergeDocuments(m_DatabaseDocuments);
		}

		private const string ASSET_PATH = "Assets/EditorResources/ECG/DataDocumentationSettings.asset";
		private const string OUTPUT_PATH = "Documentation/Data";

		[SerializeField]
		private List<DatabaseDocument> m_DatabaseDocuments = null;
		[SerializeField]
		private string m_OutputPath = OUTPUT_PATH;
		[SerializeField, DerivedTypeConstraint(typeof(DataDocumentationExporter))]
		private SerializedType m_ExporterType = new SerializedType();

		DataDocumentationSettings()
		{
			m_DatabaseDocuments = new List<DatabaseDocument>();
		}

		public void MergeDocuments(IEnumerable<DatabaseDocument> newDocuments)
		{
			List<DatabaseDocument> result = ListPool<DatabaseDocument>.Request();
			DataDocumentationUtility.Merge(m_DatabaseDocuments, newDocuments, result);
			m_DatabaseDocuments.Clear();
			m_DatabaseDocuments.AddRange(result);
			ListPool<DatabaseDocument>.Return(result);
		}

		public IEnumerable<DatabaseDocument> GetDocuments()
		{
			foreach (DatabaseDocument document in m_DatabaseDocuments)
			{
				yield return document;
			}
		}

		public void Export()
		{
			Type type = m_ExporterType.Value;
			if (type == null)
			{
				Error($"No exporter type selected");
				return;
			}
			if (!type.Is<DataDocumentationExporter>())
			{
				Error($"Exporter is not valid type? {type.FullName}");
				return;
			}
			DataDocumentationExporter exporter = Activator.CreateInstance(type) as DataDocumentationExporter;
			DataDocumentationExportOptions options = new DataDocumentationExportOptions(m_OutputPath, m_DatabaseDocuments);
			DataDocumentationUtility.Export(exporter, options);
		}

		void Error(string message)
		{
			// TODO: throw an exception? 
			Debug.LogError($"[DataDocumentation] {message}");
		}
	}
}
