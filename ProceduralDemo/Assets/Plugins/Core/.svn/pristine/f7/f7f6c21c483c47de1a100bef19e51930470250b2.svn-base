
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	public abstract class DataCollection<T> : IEnumerable<T>, IDataCollectionItem
	{
		[SerializeField]
		private string m_CollectionID = string.Empty;
		public string CollectionID => m_CollectionID;

#if UNITY_2019_1_OR_NEWER
		[SerializeReference] // Used to serialize generic data without using nested scriptable objects, was introduced in 2019
#else
		[SerializeField]
#endif
		private List<T> m_List = new List<T>();

		public DataCollection(string id, List<T> list)
		{
			m_CollectionID = id;
			m_List = list;
		}

		public int Count { get { return m_List.Count; } }

		IEnumerator<T> IEnumerable<T>.GetEnumerator() { return m_List.GetEnumerator(); }

		IEnumerator IEnumerable.GetEnumerator() { return m_List.GetEnumerator(); }

		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= m_List.Count)
				{
					throw new System.IndexOutOfRangeException();
				}
				return m_List[index];
			}
		}

		public override string ToString()
		{
			Core.Str.Flush();
			Core.Str.Add("[");
			foreach (T data in m_List)
			{
				Core.Str.Add("{");
				Core.Str.Add(data.ToString());
				Core.Str.Add("},");
			}
			Core.Str.Add("]");
			Core.Str.AddLine();
			return Core.Str.Finish();
		}
	}

	public abstract class DataCollections<TCollection, TData> : IReadOnlyCollection<TData>
		where TCollection : DataCollection<TData>
	{
		[SerializeField]
		private List<TCollection> m_Collections = new List<TCollection>();
		public IEnumerable<TCollection> Collections => m_Collections;
		public int CollectionCount => m_Collections.Count;

		public DataCollections(List<TCollection> collections)
		{
			m_Collections = collections;
		}

		int IReadOnlyCollection<TData>.Count
		{
			get
			{
				int count = 0;
				foreach (TCollection collection in m_Collections)
				{
					count += collection.Count;
				}
				return count;
			}
		}

		public IEnumerator<TData> GetEnumerator()
		{
			foreach (TCollection collection in m_Collections)
			{
				foreach (TData data in collection)
				{
					yield return data;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
