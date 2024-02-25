using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Data
{
	public class DataDocumentationExporterJson : DataDocumentationExporter
	{
		protected override void OnExport(string directory, IEnumerable<DatabaseDocument> documents)
		{
			directory = $"{directory}/JSON/";
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			foreach (DatabaseDocument document in documents)
			{
				CreateFile(directory, document);
			}
		}

		private void CreateFile(string directory, DatabaseDocument document)
		{
			string filename = $"{directory}/{document.DatabaseType.Name}.json";
			string json = JsonConvert.SerializeObject(document, Formatting.Indented);
			File.WriteAllText(filename, json);
			AssetDatabase.ImportAsset(filename);
		}
	}
}
