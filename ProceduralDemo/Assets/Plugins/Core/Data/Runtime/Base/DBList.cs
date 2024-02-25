
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	public abstract class DBList<TSingleton, TImporter, TData> :
		DBSingleton<TSingleton, TImporter, List<TData>, TData>, IReadOnlyList<TData>
			where TSingleton : DBList<TSingleton, TImporter, TData>
			where TImporter : IDataImporter<List<TData>, TData>, new()
			where TData : IDataDictItem
	{
		private List<TData> m_List = null;

		protected DBList()
		{
			m_List = new List<TData>();
		}

		#region static
		public static int GetCount() { return Instance.m_List.Count; }

		public static TData GetAtIndex(int index)
		{
			TSingleton db = Instance;
			if (index < 0 || index >= db.m_List.Count)
			{
				Debug.LogError(typeof(TSingleton).Name + "[] Index " + index + " out of range");
				return default;
			}
			return db.m_List[index];
		}

		public static int IndexOf(TData data)
		{
			return Instance.m_List.IndexOf(data);
		}

		public static IEnumerable<TData> GetValues() { return Instance.m_List; }

		public static IEnumerable<T> GetValuesOfType<T>() where T : TData
		{
			for (int i = 0; i < Instance.m_List.Count; ++i)
			{
				if (Instance.m_List[i] is T data)
				{
					yield return data;
				}
			}
		}
		#endregion

		#region IReadOnlyList
		public int Count => m_List.Count;

		public TData this[int index] => m_List[index];

		IEnumerator<TData> IEnumerable<TData>.GetEnumerator()
		{
			return m_List.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_List.GetEnumerator();
		}
		#endregion

		#region DBBase
		public override IEnumerable<string> GetEditorPickerIDs()
		{
			foreach (TData data in m_List)
			{
				yield return data.ID;
			}
		}

		public override bool EditorTryGetPrimaryKey(string key, out object dataObject)
		{
			dataObject = EditorTryGetData(key, out IDataDictItem d) ? d : null;
			return dataObject != null;
		}
		public override bool EditorTryGetData(string key, out IDataDictItem data)
		{
			foreach (TData d in m_List)
			{
				if (string.Equals(d.ID, key))
				{
					data = d;
					return true;
				}
			}
			data = null;
			return false;
		}

		sealed public override string SerializeDefaultServerJson()
		{
			string json = JsonConvert.SerializeObject(m_List, Formatting.Indented);
			return json;
		}
		#endregion

		protected internal override void InitializeInternal(IEnumerable<List<TData>> rawData, int dataCount)
		{
			m_List = new List<TData>(dataCount);
			foreach (List<TData> data in rawData)
			{
				int count = data.Count;
				for (int i = 0; i < count; i++)
				{
					TData d = data[i];
					if (d == null)
					{
						Core.DebugUtil.Error(typeof(TSingleton).Name, "[DBList] .Initialize() null at index ", i.ToString()," in sheet ", SheetName);
					}
					else
					{
						m_List.Add(d);
					}
				}
			}
		}
	}
}
