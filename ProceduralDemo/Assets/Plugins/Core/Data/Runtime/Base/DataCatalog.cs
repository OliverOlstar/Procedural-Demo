
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	public interface IDataCatalog
	{
		System.Type CatalogDataType { get; }
		IEnumerable<System.Type> GetCatalogDBTypes();
		bool TryGetValueOfAnyTypeFromCatalog(string id, out IDataDictItem data);
	}

	public abstract class DataCatalog<TSingleton, TCatalogData, TData> : DataBaseBin<TSingleton, TCatalogData>, IDataCatalog, IOverrideExcelToJsonSerialization
			where TSingleton : DataCatalog<TSingleton, TCatalogData, TData>
			where TCatalogData : DataCatalogData, new()
			where TData : class, IDataDictItem
	{
		System.Type IDataCatalog.CatalogDataType => typeof(TData);

		IEnumerable<System.Type> IDataCatalog.GetCatalogDBTypes()
		{
			foreach (TCatalogData data in GetValues())
			{
				yield return data.DataBase.GetType();
			}
		}

		bool IDataCatalog.TryGetValueOfAnyTypeFromCatalog(string id, out IDataDictItem data)
		{
			bool success = TryGetValueFromCatalog(id, out TData d);
			data = d;
			return success;
		}

		public static bool CatalogContains(string id)
		{
			return Instance.TryGetValueFromCatalog<TData>(id, out _);
		}

		public static bool CatalogContains<T>(string id) where T : class, TData
		{
			return Instance.TryGetValueFromCatalog<T>(id, out _);
		}

		public static bool TryGetFromCatalog<T>(string id, out T item) where T : class, TData
		{
			return Instance.TryGetValueFromCatalog(id, out item);
		}

		public static T GetFromCatalog<T>(string id) where T : class, TData
		{
			if (!TryGetFromCatalog(id, out T item))
			{
				Core.DebugUtil.DevException($"{typeof(TSingleton).Name}.GetKey() {id} not found");
			}
			return item;
		}

		public bool TryGetValueFromCatalog<T>(string id, out T item) where T : class, TData
		{
			int count = Count;
			for (int i = 0; i < count; ++i)
			{
				if (this[i].CatalogDB.TryGetCatalogData(id, out IDataDictItem key))
				{
					item = key as T;
					return item != null;
				}
			}
			item = default;
			return false;
		}

		// Note: AddValidationDependencies() is left empty since validating a catalog doesn't actually require accessing it's items.
		/// <summary> Util function for other DBs AddValidationDependencies() to include a catalog easily </summary>
		public void AddValidationDependenciesAndSelf(List<Type> dependencies)
		{
			dependencies.Add(GetType());
			foreach (TCatalogData data in GetValues())
			{
				dependencies.Add(data.DataBase.GetType());
			}
		}

		public override IEnumerable<string> GetEditorPickerIDs()
		{
			foreach (TCatalogData data in GetValues())
			{
				// Note: data.DataBase won't be initialized at edit time, need to fetch this through DBManager so it get's initialized
				if (DBManager.TryGet(data.DataBase.GetType(), out DBBase db))
				{
					foreach (string id in db.GetEditorPickerIDs())
					{
						yield return id;
					}
				}
			}
		}

		string IOverrideExcelToJsonSerialization.OnSerializeServerJson()
		{
			Dictionary<string, SerializeCatalogData> keys = Core.DictionaryPool<string, SerializeCatalogData>.Request();
			foreach (TCatalogData data in Values)
			{
				foreach (IDataDictItem key in data.CatalogDB.CatalogData)
				{
					if (key == null)
					{
						Debug.LogError($"DataCatalog.OnSerializeServerJson() {data.CatalogDB.GetType().Name} in {GetType().Name} contains null data");
						continue;
					}
					if (keys.ContainsKey(key.ID))
					{
						//string format = "Duplicate Key found in KeyCatalog. Type: {0}, Key: '{1}'";
						//Debug.LogErrorFormat(this, format, key.GetType().Name, key.ID);
						continue;
					}
					keys.Add(key.ID, data.CreateServerData(key));
				}
			}
			string json = JsonConvert.SerializeObject(new List<SerializeCatalogData>(keys.Values), Formatting.Indented);
			Core.DictionaryPool<string, SerializeCatalogData>.Return(keys);
			return json;
		}
	}
}
