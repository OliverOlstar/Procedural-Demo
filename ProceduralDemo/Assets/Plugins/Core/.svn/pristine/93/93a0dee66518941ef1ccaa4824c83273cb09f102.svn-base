
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Data
{
	public static class DataImportUtil
	{
		public static object ProcessRow(Raw.Sheet sheet, Raw.Row row, System.Type type)
		{
			object entry = System.Activator.CreateInstance(type);
			Data.Raw.ReflectionCache refCache = sheet.GetReflectionCache(type);
			DataImporterUtil.InvokeMethod(entry, sheet, row, refCache);
			DataImporterUtil.AssignField(entry, sheet, row, refCache);
			return entry;
		}

		private static bool CanInstantiateType(System.Type type) => !type.IsInterface && !type.IsAbstract;

		public static bool TryInstantiateSampleData<TData>(out TData sampleData, out string error) where TData : class, IDataDictItem
		{
			System.Type sampleType = typeof(TData);
			if (!CanInstantiateType(sampleType))
			{
				// If we can't instantiate an instance of the base data type (because it's an interface or abstract) things get a bit weird...
				// Search for a subclass that is valid for instantiation and use that
				// TODO: This code is all kinda weird we have to instantiate a data instance just to call IDataGeneric.TypeNameToClassName
				// Really what we should do is write an IGenericDB interface so the DB can do this job, that'd make way more sense
				foreach (System.Type t in Core.TypeUtility.GetAllTypes())
				{
					if (!typeof(TData).IsAssignableFrom(t))
					{
						continue;
					}
					if (CanInstantiateType(t))
					{
						sampleType = t;
						break;
					}
				}
				if (!CanInstantiateType(sampleType))
				{
					error = $"There is no valid C# subclass of data type '{typeof(TData).Name}'";
					sampleData = null;
					return false;
				}
			}
			sampleData = System.Activator.CreateInstance(sampleType) as TData;
			error = null;
			return true;
		}

		public static void ProcessRows<TData>(Raw.Sheet sheet, out List<TData> list, out string error) where TData : class, IDataDictItem
		{
			list = new List<TData>(sheet.Rows.Count);
			error = null;
			if (!typeof(IDataGeneric).IsAssignableFrom(typeof(TData)))
			{
				foreach (Raw.Row row in sheet.Rows.Values)
				{
					TData entry = ProcessRow(sheet, row, typeof(TData)) as TData;
					list.Add(entry);
				}
				return;
			}
			if (!TryInstantiateSampleData(out TData sampleData, out error))
			{
				return;
			}
			IDataGeneric genericSample = sampleData as IDataGeneric;
			foreach (KeyValuePair<string, Raw.Row> row in sheet.Rows)
			{
				if (!row.Value.TryGetValue("Type", out object typeColumnValue))
				{
					error = "Must have a column named 'Type'";
					return;
				}
				string className = typeColumnValue.ToString();
				System.Type type = genericSample.TypeNameToClassName(className, sheet.SheetName);
				if (type == null)
				{
					error = $"Row with ID '{row.Key}' Type '{typeColumnValue}' is not invalid. Could not find a C# class with name '{className}'";
					return;
				}
				object obj = ProcessRow(sheet, row.Value, type);
				if (!(obj is TData entry))
				{
					error = $"Row with ID '{row.Key}' and Type '{typeColumnValue}' instantiated C# class with type '{obj.GetType().Name}' " +
						$"which cannot be cast to C# class '{typeof(TData).Name}' which is required by DB";
					return;
				}
				list.Add(entry);
			}
		}
	}

	public class DataImporterList<TData> : IDataImporter<List<TData>, TData>
		where TData : class, IDataDictItem
	{
		public List<TData> ImportRawData(DBBase dataBase, Raw.Sheet sheet, out string error)
		{
			DataImportUtil.ProcessRows(sheet, out List<TData> list, out error);
			return list;
		}
	}

	public class DataImporterCollection<TCollections, TCollection, TData> : IDataImporter<TCollections, TData>
		where TCollections : DataCollections<TCollection, TData>
		where TCollection : DataCollection<TData>
		where TData : class, IDataDictItem, IDataCollectionItem
	{
		public TCollections ImportRawData(DBBase dataBase, Raw.Sheet sheet, out string error)
		{
			DataImportUtil.ProcessRows(sheet, out List<TData> allData, out error);
			if (!string.IsNullOrEmpty(error))
			{
				return System.Activator.CreateInstance(typeof(TCollections), new List<TCollection>()) as TCollections;
			}

			Dictionary<string, List<TData>> dict = new Dictionary<string, List<TData>>(allData.Count);
			foreach (TData data in allData)
			{
				if (data.CollectionID == null)
				{
					throw new System.Exception($"DataImporterCollection.ImportRawData() Data.CollectionID of type {typeof(TData).Name} in collection {typeof(TCollection).Name} is null.");
				}
				if (!dict.TryGetValue(data.CollectionID, out List<TData> list))
				{
					list = new List<TData>();
					dict.Add(data.CollectionID, list);
				}
				list.Add(data);
			}
			List<TCollection> collections = new List<TCollection>();
			foreach (List<TData> list in dict.Values)
			{
				TCollection collection = System.Activator.CreateInstance(
					typeof(TCollection),
					list[0].CollectionID,
					list) as TCollection;
				collections.Add(collection);
			}
			return System.Activator.CreateInstance(typeof(TCollections), collections) as TCollections;
		}
	}
}
