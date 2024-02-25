
using System.Collections.Generic;
using Data.Raw;
using UnityEngine;

namespace Data
{
	public class DataImporterCollection2021<TCollections, TCollection, TData> : IDataImporter<TCollections, TData>
		where TCollections : DataCollections<TCollection, TData>
		where TCollection : DataCollection<TData>
		where TData : DataBin, IDataDictItem, IDataCollectionItem
	{
		public TCollections ImportRawData(DBBase dataBase, Sheet sheet, out string error)
		{
			error = null;
			List<TData> allData = DataImportUtil2021.Import<TData>(dataBase, sheet, ref error);
			Dictionary<string, List<TData>> dict = new Dictionary<string, List<TData>>(allData.Count);
			foreach (TData data in allData)
			{
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
