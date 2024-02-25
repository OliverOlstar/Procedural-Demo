using Core;
using System;
using System.Collections.Generic;

namespace Data
{
	public class DataDocumentationUtility
	{
		private DataDocumentationUtility() { }

		public static void Export(DataDocumentationExporter exporter, DataDocumentationExportOptions options)
		{
			if (exporter == null)
			{
				throw new ArgumentNullException(nameof(exporter));
			}
			if (options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}
			exporter.Export(options.Directory, options.GetDocuments());
		}

		public static void Merge(IEnumerable<DatabaseDocument> existingDocs, IEnumerable<DatabaseDocument> newDocs, List<DatabaseDocument> result)
		{
			result = result ?? throw new ArgumentNullException(nameof(result));
			result.Clear();
			Dictionary<Type, DatabaseDocument> newDocsDict = DictionaryPool<Type, DatabaseDocument>.Request();
			foreach (DatabaseDocument doc in newDocs)
			{
				if (!newDocsDict.TryAdd(doc.DatabaseType, doc))
				{
					throw new Exception($"{nameof(newDocs)} has 2 Documents with the same {nameof(doc.DatabaseType)}: {doc.DatabaseType}");
				}
			}
			foreach (DatabaseDocument existing in existingDocs)
			{
				if (!newDocsDict.TryGetValue(existing.DatabaseType, out DatabaseDocument src))
				{
					continue;
				}
				foreach (ColumnDocument srcColumn in src.Columns)
				{
					if (existing.TryGetColumn(srcColumn.ColumnName, out ColumnDocument destColumn))
					{
						srcColumn.SetExtraNotes(destColumn.ExtraNotes);
					}
				}
			}
			result.AddRange(newDocsDict.Values);
			DictionaryPool<Type, DatabaseDocument>.Return(newDocsDict);
		}
	}
}
