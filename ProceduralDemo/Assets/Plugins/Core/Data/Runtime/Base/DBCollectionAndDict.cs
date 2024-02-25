
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	public abstract class DBCollectionAndDict<TSingleton, TImporter, TCollections, TCollection, TData> :
		DBCollection<TSingleton, TImporter, TCollections, TCollection, TData>, IReadOnlyDictionary<string, TData>
			where TSingleton : DBCollectionAndDict<TSingleton, TImporter, TCollections, TCollection, TData>
			where TImporter : IDataImporter<TCollections, TData>, new()
			where TCollections : DataCollections<TCollection, TData>
			where TCollection : DataCollection<TData>
			where TData : IDataDictItem, IDataCollectionItem
	{
		private Dictionary<string, TData> m_Data = null;

		protected DBCollectionAndDict() : base()
		{
			m_Data = new Dictionary<string, TData>();
		}

		#region static
		public static TData Get(string key)
		{
			if (!Instance.m_Data.TryGetValue(key, out TData rt))
			{
				Core.DebugUtil.DevExceptionFormat("{0}.Get() Data {1} does not exist", typeof(TSingleton).Name, key);
			}
			return rt;
		}

		public static bool TryGet(string key, out TData data)
		{
			return Instance.m_Data.TryGetValue(key, out data);
		}

		public static bool Contains(string key)
		{
			return Instance.m_Data.ContainsKey(key);
		}
		#endregion

		#region DBBase
		public override bool EditorTryGetData(string key, out IDataDictItem data)
		{
			data = m_Data.TryGetValue(key, out TData d) ? (IDataDictItem)d : null;
			return data != null;
		}
		#endregion

		#region IReadOnlyDictionary
		public IEnumerable<TData> Values => m_Data.Values;

		public int Count => m_Data.Count;

		public IEnumerable<string> Keys => m_Data.Keys;

		public TData this[string key] => m_Data[key];

		public bool ContainsKey(string key) => string.IsNullOrEmpty(key) ? false : m_Data.ContainsKey(key);

		public bool TryGetValue(string key, out TData value)
		{
			return m_Data.TryGetValue(key, out value);
		}

		public IEnumerator<KeyValuePair<string, TData>> GetEnumerator()
		{
			return m_Data.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_Data.GetEnumerator();
		}
		#endregion

		protected internal override void InitializeInternal(IEnumerable<TCollections> rawData, int dataCount)
		{
			base.InitializeInternal(rawData, dataCount);
			m_Data = new Dictionary<string, TData>(dataCount);
			foreach (TCollections collecions in rawData)
			{
				foreach (TData d in collecions)
				{
					if (!m_Data.ContainsKey(d.ID))
					{
						m_Data.Add(d.ID, d);
					}
					else
					{
						Core.DebugUtil.Error(typeof(TData).Name, ".Initialize() Duplicate key ", d.ID.ToString(), " in sheet ", SheetName);
					}
				}
			}
		}
	}
}
