using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

namespace Data
{
	public class DataDocumentationExporterHtml : DataDocumentationExporter
	{
		protected override void OnExport(string directory, IEnumerable<DatabaseDocument> documents)
		{
			directory = $"{directory}/HTML";
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			CreateIndexHtml(directory, documents);
			directory = $"{directory}/docs";
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			foreach (DatabaseDocument document in documents)
			{
				CreateDocumentHtml(directory, document);
			}
		}

		private void CreateIndexHtml(string directory, IEnumerable<DatabaseDocument> documents)
		{
			string filename = $"{directory}/index.html";
			string contents = CreateIndexContent(documents);
			File.WriteAllText(filename, contents);
			AssetDatabase.ImportAsset(filename);
		}

		private string CreateIndexContent(IEnumerable<DatabaseDocument> documents)
		{
			StringBuilder content = new StringBuilder();
			content.AppendLine($"<html>");
			content.AppendLine($"	<style>");
			content.AppendLine($"		table, th, td {{");
			content.AppendLine($"			border: 1px solid black;");
			content.AppendLine($"			text-align: left;");
			content.AppendLine($"		}}");
			content.AppendLine($"	</style>");
			content.AppendLine($"	<head>");
			content.AppendLine($"		<title>Oko Data Documentation</title>");
			content.AppendLine($"	</head>");
			content.AppendLine($"	<body>");
			content.AppendLine($"	<table>");
			content.AppendLine($"		<tr>");
			content.AppendLine($"			<th>DB Name</th>");
			content.AppendLine($"		</tr>");
			foreach (DatabaseDocument document in documents)
			{
				AppendDocumentRowHtml(content, document);
			}
			content.AppendLine($"	</table>");
			content.AppendLine($"	</body>");
			content.AppendLine($"</html>");
			return content.ToString();
		}

		private void AppendDocumentRowHtml(StringBuilder content, DatabaseDocument document)
		{
			string dbTypeName = EncodeHtmlString(document.DatabaseType.Name);
			string dbName = EncodeHtmlString(document.DatabaseName);
			content.AppendLine($"		<tr>");
			content.AppendLine($"			<td><a href=\"docs/{dbTypeName}.html\" target=\"_blank\">{dbName}</a></th>");
			content.AppendLine($"		</tr>");
		}

		private void CreateDocumentHtml(string directory, DatabaseDocument document)
		{
			string filename = $"{directory}/{document.DatabaseType.Name}.html";
			string content = CreateDocumentContent(document);
			File.WriteAllText(filename, content.ToString());
			AssetDatabase.ImportAsset(filename);
		}

		private string CreateDocumentContent(DatabaseDocument document)
		{
			string dbName = EncodeHtmlString(document.DatabaseName);
			StringBuilder content = new StringBuilder();
			content.AppendLine($"<html>");
			content.AppendLine($"	<style>");
			content.AppendLine($"		table, th, td {{");
			content.AppendLine($"			border: 1px solid black;");
			content.AppendLine($"			text-align: top;");
			content.AppendLine($"			vertical-align: top;");
			content.AppendLine($"		}}");
			content.AppendLine($"	</style>");
			content.AppendLine($"	<head>");
			content.AppendLine($"		<title>{dbName}</title>");
			content.AppendLine($"	</head>");
			content.AppendLine($"	<body>");
			content.AppendLine($"	<table>");
			content.AppendLine($"		<tr>");
			content.AppendLine($"			<th>Column Name</th><th>Type</th><th>Validation</th><th>Sheet Notes</th><th>Extra Notes</th>");
			content.AppendLine($"		</tr>");
			foreach (ColumnDocument column in document.Columns)
			{
				AppendDocumentColumnHtml(content, column);
			}
			content.AppendLine($"	</table>");
			content.AppendLine($"	</body>");
			content.AppendLine($"</html>");
			return content.ToString();
		}

		private void AppendDocumentColumnHtml(StringBuilder content, ColumnDocument column)
		{
			string columnName = EncodeHtmlString(column.ColumnName);
			string columnTypeName = EncodeHtmlString(column.ColumnType?.Name);
			string columnValidation = EncodeHtmlString(column.Validation);
			string columnSheetNotes = EncodeHtmlString(column.SheetNotes);
			string columnExtraNotes = EncodeHtmlString(column.ExtraNotes);
			content.AppendLine($"		<tr>");
			content.AppendLine($"			<td>{columnName}</td><td>{columnTypeName}</td><td>{columnValidation}</td><td>{columnSheetNotes}</td><td>{columnExtraNotes}</td>");
			content.AppendLine($"		</tr>");
		}

		private string EncodeHtmlString(string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return "(null)";
			}
			str = str.Replace("\n", "<br></br>");
			return str;
		}
	}
}
