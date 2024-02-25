using System.Collections.Generic;
using System.IO;

namespace Data
{
	public abstract class DataDocumentationExporter
	{
		public DataDocumentationExporter()
		{
		}

		public void Export(string directory, IEnumerable<DatabaseDocument> documents)
		{
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			OnExport(directory, documents);
		}

		protected abstract void OnExport(string directory, IEnumerable<DatabaseDocument> documents);
	}
}
