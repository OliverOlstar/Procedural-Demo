
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	public abstract class DBDict<TSingleton, TImporter, TData> :
		DBList<TSingleton, TImporter, TData>, IReadOnlyDictionary<string, TData>
			where TSingleton : DBDict<TSingleton, TImporter, TData>
			where TImporter : IDataImporter<List<TData>, TData>, new()
			where TData : IDataDictItem
	{
		private Dictionary<string, TData> m_Data = null;

		protected DBDict() : base()
		{
			m_Data = new Dictionary<string, TData>();
		}

		#region static
		public static TData Get(string key)
		{
			if (!Instance.m_Data.TryGetValue(key, out TData rt))
			{
				Core.DebugUtil.DevExceptionFormat("{0}.Get() Data '{1}' does not exist", typeof(TSingleton).Name, key);
			}
			return rt;
		}

		public static TData GetOrDefault(string key)
		{
			return Instance.m_Data.TryGetValue(key, out TData rt) ? rt : default;
		}

		public static bool TryGet(string key, out TData data)
		{
			if (string.IsNullOrEmpty(key))
			{
				data = default;
				return false;
			}
			return Instance.m_Data.TryGetValue(key, out data);
		}

		public static bool Contains(string key)
		{
			return Instance.m_Data.ContainsKey(key);
		}
		#endregion

		#region IReadOnlyDictionary
		public TData this[string key] => m_Data[key];

		public IEnumerable<string> Keys => m_Data.Keys;
		public IEnumerable<TData> Values => m_Data.Values;

		public bool ContainsKey(string key)
		{
			return string.IsNullOrEmpty(key) ? false : m_Data.ContainsKey(key);
		}

		public bool TryGetValue(string key, out TData value)
		{
			return m_Data.TryGetValue(key, out value);
		}

		public IEnumerator<KeyValuePair<string, TData>> GetEnumerator()
		{
			return m_Data.GetEnumerator();
		}
		#endregion

		#region DBBase
		public override IEnumerable<string> GetEditorPickerIDs() => m_Data.Keys;

		public override bool EditorTryGetPrimaryKey(string key, out object dataObject)
		{
			dataObject = EditorTryGetData(key, out IDataDictItem d) ? d : null;
			return dataObject != null;
		}
		public override bool EditorTryGetData(string key, out IDataDictItem data)
		{
			if (string.IsNullOrEmpty(key))
			{
				data = null;
				return false;
			}
			data = m_Data.TryGetValue(key, out TData d) ? (IDataDictItem)d : null;
			return data != null;
		}
		#endregion

		protected internal override void InitializeInternal(IEnumerable<List<TData>> rawData, int dataCount)
		{
			base.InitializeInternal(rawData, dataCount);
			m_Data = new Dictionary<string, TData>(dataCount);
			foreach (List<TData> data in rawData)
			{
				int count = data.Count;
				for (int i = 0; i < count; i++)
				{
					TData d = data[i];
					if (d == null)
					{
						Core.DebugUtil.Error(typeof(TSingleton).Name, "[DBDict] .Initialize() null at index ", i.ToString(), " in sheet ", SheetName);
					}
					else if (m_Data.ContainsKey(d.ID))
					{
						Core.DebugUtil.Error(typeof(TSingleton).Name, "[DBDict] .Initialize() Duplicate key ", d.ID.ToString(), " in sheet ", SheetName);
					}
					else
					{
						m_Data.Add(d.ID, d);
					}
				}
			}
		}
	}
}
