
using System.Collections;
using System.Collections.Generic;

namespace Data
{
	public abstract class DBCollectionAndDictAndList<TSingleton, TImporter, TCollections, TCollection, TData> :
		DBCollectionAndDict<TSingleton, TImporter, TCollections, TCollection, TData>, IReadOnlyList<TData>
			where TSingleton : DBCollectionAndDictAndList<TSingleton, TImporter, TCollections, TCollection, TData>
			where TImporter : IDataImporter<TCollections, TData>, new()
			where TCollections : DataCollections<TCollection, TData>
			where TCollection : DataCollection<TData>
			where TData : IDataDictItem, IDataCollectionItem
	{
		private List<TData> m_List = null;

		protected DBCollectionAndDictAndList()
		{
			m_List = new List<TData>();
		}

		protected internal override void InitializeInternal(IEnumerable<TCollections> rawData, int dataCount)
		{
			base.InitializeInternal(rawData, dataCount);
			m_List = new List<TData>(dataCount);
			foreach (TCollections data in rawData)
			{
				m_List.AddRange(data);
			}
		}

		public static int IndexOf(TData data)
		{
			return Instance.m_List.IndexOf(data);
		}
		
		#region IReadOnlyList
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
	}
}
