
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	public interface IDataCatalogDB
	{
		bool TryGetCatalogData(string id, out IDataDictItem item);
		IEnumerable<IDataDictItem> CatalogData { get; }
		System.Type DataType { get; }
	}

	public class DataCatalogData : DataBin, IDataValidate
	{
		[SerializeField, Data.Import.Reference.Asset(false)]
		protected DBBase m_Database = null;
		public DBBase DataBase => m_Database;

		public IDataCatalogDB CatalogDB => m_Database as IDataCatalogDB;

		string IDataValidate.OnDataValidate()
		{
			if (m_Database == null)
			{
				return "Has null data base, it must have been corrupted and not serialized correctly";
			}
			if (m_Database is IDataCatalogDB)
			{
				return null;
			}
			return $"{m_Database.GetType().Name} must implement the IDataCatalog interface";
		}

		public virtual SerializeCatalogData CreateServerData(IDataDictItem data)
		{
			SerializeCatalogData jsonData = new SerializeCatalogData()
			{
				ID = data.ID,
				Type = data.Type.Name,
			};
			return jsonData;
		}
	}

	public class SerializeCatalogData
	{
		public string ID;
		public string Type;
	}
}
