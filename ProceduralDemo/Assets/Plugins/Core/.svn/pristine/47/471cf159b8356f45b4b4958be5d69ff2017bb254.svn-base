using System.Collections.Generic;

namespace Data
{
	public class DataDocumentationExportOptions
	{
		public string Directory { get; }

		private List<DatabaseDocument> m_Documents;

		public DataDocumentationExportOptions(string directory, IEnumerable<DatabaseDocument> documents)
		{
			Directory = directory;
			m_Documents = new List<DatabaseDocument>(documents);
		}

		public IEnumerable<DatabaseDocument> GetDocuments()
		{
			foreach (DatabaseDocument document in m_Documents)
			{
				yield return document;
			}
		}
	}
}
