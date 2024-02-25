
using ExcelDataReader;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Data
{
	public static class ImportExcel
	{
		public static readonly HashSet<string> VALID_EXTENSIONS = new HashSet<string> { ".XLS", ".XLSX" };

		public static List<DataTable> ParseDirectoryToDataTables(string excelPath, string progressTitle = null)
		{
			if (!Directory.Exists(excelPath))
			{
				return new List<DataTable>();
			}
			string[] files = Directory.GetFiles(excelPath);
			List<DataTable> dataTables = new List<DataTable>(files.Length);
			for (int i = 0; i < files.Length; i++)
			{
				string file = files[i];
				string extension = Path.GetExtension(file);
				if (!VALID_EXTENSIONS.Contains(extension.ToUpper()))
				{
					continue;
				}
				string fileName = Path.GetFileNameWithoutExtension(file);
				if (fileName.StartsWith("~") || fileName.StartsWith("_"))
				{
					continue;
				}
				if (!string.IsNullOrEmpty(progressTitle))
				{
					EditorUtility.DisplayProgressBar(progressTitle, fileName, (float)(i + 1) / files.Length);
				}
				DataSet dataSet = ParseExcelToDataSet(file);
				foreach (DataTable dt in dataSet.Tables)
				{
					if (!dt.TableName.StartsWith("_"))
					{
						dataTables.Add(dt);
					}
				}
			}
			EditorUtility.ClearProgressBar();
			return dataTables;
		}

		public static Dictionary<string, Raw.Sheet> ConvertDataTablesToRawSheets(List<DataTable> dataTables)
		{
			Dictionary<string, Raw.Sheet> allSheets = new Dictionary<string, Raw.Sheet>(dataTables.Count);
			foreach (DataTable dataTable in dataTables)
			{
				Raw.Sheet sheet = ImportSheet(dataTable);
				if (sheet == null)
				{
					continue;
				}
				if (allSheets.ContainsKey(sheet.SheetName))
				{
					DataValidationUtil.Raise(sheet.SheetName, "Duplicate sheet with the name");
					continue;
				}
				allSheets.Add(sheet.SheetName, sheet);
			}
			return allSheets;
		}

		public static Dictionary<string, Raw.Sheet> ParseDirectoryAndConverToRawSheets(string excelPath, string progressTitle = null)
		{
			List<DataTable> dataTables = ParseDirectoryToDataTables(excelPath, progressTitle);
			Dictionary<string, Raw.Sheet> allSheets = ConvertDataTablesToRawSheets(dataTables);
			return allSheets;
		}

		private static DataSet ParseExcelToDataSet(string excelFilePath)
		{
			FileStream stream = File.Open(excelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			IExcelDataReader excelReader;
			//1. Reading Excel file
			if (Path.GetExtension(excelFilePath).ToUpper() == ".XLS")
			{
				//1.1 Reading from a binary Excel file ('97-2003 format; *.xls)
				excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
			}
			else
			{
				//1.2 Reading from a OpenXml Excel file (2007 format; *.xlsx)
				excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
			}
			//3. DataSet - Create column names from first row
			DataSet dataSet = null;
			try
			{
				dataSet = excelReader.AsDataSet(new ExcelDataSetConfiguration()
				{
					ConfigureDataTable = tableReader => new ExcelDataTableConfiguration()
					{
						UseHeaderRow = false,
						//ReadHeaderRow = rowreader =>
						//{
						//	rowreader.Read();
						//}
						//FilterColumn = (rowReader, columnIndex) =>
						//{
						//	return true;
						//}
					}
				});
			}
			catch (System.Exception e)
			{
				DataValidationUtil.Raise("ExcelToJson.convertExcelSheetsToJsonData() " + e);
			}
			excelReader.Close();
			stream.Close();
			return dataSet;
		}

		private static Raw.Sheet ImportSheet(DataTable dt)
		{
			if (dt.Rows.Count == 0)
			{
				DataValidationUtil.Raise(dt.TableName, "Sheet has no rows");
				return null;
			}
			if (dt.Columns.Count == 0)
			{
				DataValidationUtil.Raise(dt.TableName, "Sheet has no columns");
				return null;
			}
			Raw.Sheet sheet = new Raw.Sheet();
			sheet.SheetName = dt.TableName;
			sheet.ColumnNames = new HashSet<string>();
			sheet.Rows = new Raw.Rows(dt.Rows.Count);
			HashSet<int> skipColumns = new HashSet<int>();
			DataRow topRow = dt.Rows[0];
			int idColumnIndex = 0;
			sheet.IDColumnName = topRow[idColumnIndex].ToString();
			for (int j = 0; j < dt.Columns.Count; j++)
			{
				string columnName = topRow[j].ToString();
				if (columnName == "ID") // Try to find a row named "ID" for the main ID
				{
					sheet.IDColumnName = "ID";
					idColumnIndex = j;
				}
				if (string.IsNullOrEmpty(columnName) || columnName.StartsWith("_"))
				{
					skipColumns.Add(j);
				}
				else
				{
					sheet.ColumnNames.Add(columnName);
				}
			}
			for (int i = 1; i < dt.Rows.Count; i++)
			{
				Raw.Row rowDict = new Raw.Row();
				DataRow row = dt.Rows[i];
				string id = row[idColumnIndex].ToString();
				if (string.IsNullOrEmpty(id) || id.StartsWith("_"))
				{
					continue;
				}
				if (sheet.Rows.ContainsKey(id))
				{
					DataValidationUtil.Raise(dt.TableName, id, "Duplicate ID");
					continue;
				}
				for (int j = 0; j < dt.Columns.Count; j++)
				{
					if (skipColumns.Contains(j))
					{
						continue;
					}
					object obj = row[j];
					if (obj is System.Double && IsDoubleAnInt((System.Double)obj))
					{
						obj = System.Convert.ToInt64(obj);
					}
					rowDict[topRow[j].ToString()] = obj;
				}
				sheet.Rows.Add(id, rowDict);
			}
			return sheet;
		}

		private static bool IsDoubleAnInt(System.Double d)
		{
			return System.Math.Abs(d % 1) <= (System.Double.Epsilon * 100);
		}
	}
}
