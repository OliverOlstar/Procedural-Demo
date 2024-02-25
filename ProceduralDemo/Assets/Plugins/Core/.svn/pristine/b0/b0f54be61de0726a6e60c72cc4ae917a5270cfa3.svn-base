
using Data.Raw;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	public static class DataImportUtil2021
	{
		public static void ProcessRow<TData>(
			Sheet sheet,
			KeyValuePair<string, Raw.Row> row,
			TData currentData,
			object sampleData,
			out TData outputData,
			ref string error)
			where TData : class, IDataDictItem
		{
			outputData = currentData;
			if (sampleData is IDataGeneric genericSampleData)
			{
				if (!row.Value.TryGetValue("Type", out object typeColumn))
				{
					error = sheet.SheetName + " must have a column named 'Type'";
					outputData = null;
					return;
				}
				string typeString = typeColumn.ToString();
				// Make sure type hasnt changed
				System.Type genericDataType = genericSampleData.TypeNameToClassName(typeString, sheet.SheetName);
				if (genericDataType == null)
				{
					error = "[" + row.Key + ",Type] = {" + typeString + "} Invalid type";
					outputData = null;
					return;
				}
				bool newData = outputData == null || outputData.GetType() != genericDataType; // Check if an entry for this data already exists
				if (newData)
				{
					outputData = System.Activator.CreateInstance(genericDataType) as TData;  // Use a specific type
					if (outputData == null)
					{
						error = "[" + row.Key + ",Type] = {" + typeString + "} No matching data class name '" + genericDataType.Name + "'";
						return;
					}
				}
			}
			else if (outputData == null)
			{
				outputData = System.Activator.CreateInstance<TData>();
			}
			if (outputData is IDataPreImport preImport)
			{
				preImport.OnDataPreImport();
			}
			ReflectionCache refCache = sheet.GetReflectionCache(outputData.GetType());
			DataImporterUtil.InvokeMethod(outputData, sheet, row.Value, refCache);
			DataImporterUtil.AssignField(outputData, sheet, row.Value, refCache, sampleData);
		}

		public static List<TData> Import<TData>(DBBase dataBase, Sheet sheet, ref string error) where TData : class, IDataDictItem
		{
			Raw.Rows rawData = sheet.Rows;
			List<TData> list = new List<TData>(rawData.Count);
			Dictionary<string, TData> currentData = new Dictionary<string, TData>(dataBase.EditorRawDataCount);
			foreach (IDataDictItem item in dataBase.GetEditorRawData())
			{
				if (item is TData data)
				{
					currentData.Add(item.ID, data);
				}
			}
			if (!DataImportUtil.TryInstantiateSampleData(out TData sampleData, out error))
			{
				return list;
			}
			// Build new data set
			bool reimport = false;
			foreach (KeyValuePair<string, Raw.Row> row in rawData)
			{
				currentData.TryGetValue(row.Key, out TData entry);
				ProcessRow(sheet, row, entry, sampleData, out TData newEntry, ref error);
				if (!string.IsNullOrEmpty(error))
				{
					break;
				}
				list.Add(newEntry);
				if (entry == null || entry != newEntry)
				{
					// We need to do this after filling out memebers with reflection
					reimport = true;
				}
				else if (entry != null)
				{
					// Remove current data as we will delete all unused datas bellow
					currentData.Remove(entry.ID);
				}
			}
			// Destroy remaining data objects that are no longer needed
			foreach (TData data in currentData.Values)
			{
				reimport = true;
			}
			if (reimport)
			{
//#if UNITY_EDITOR
//				// This call turns out to be very expensive so only call it when necessary
//				string path = UnityEditor.AssetDatabase.GetAssetPath(dataBase);
//				UnityEditor.AssetDatabase.ImportAsset(path, UnityEditor.ImportAssetOptions.ImportRecursive);
//#endif
			}
			return list;
		}
	}
}
