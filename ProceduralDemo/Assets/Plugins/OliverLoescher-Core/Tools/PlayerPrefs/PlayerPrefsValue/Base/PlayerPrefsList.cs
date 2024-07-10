using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OCore.PlayerPrefs
{
	public abstract class PlayerPrefsList<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection, IList
	{
		protected abstract T GetIndex(int index, bool isGobal);
		protected abstract void SetIndex(int index, T value, bool isGobal);

		public string m_Key;
		public bool m_IsGlobalPref = false;
		protected List<T> m_Values;

		public string Key => m_Key;
		public List<T> Values
		{
			get => m_Values;
			set
			{
				Set(m_Values);
			}
		}

		private string IndexKey(int index) => m_Key + index;

		public PlayerPrefsList(string key, bool isGlobalPref = false)
		{
			m_Key = key;
			m_IsGlobalPref = isGlobalPref;
			m_Values = new List<T>();
			int index = 0;
			while (PlayerPrefs.HasKey(IndexKey(index)))
			{
				m_Values.Add(GetIndex(index, m_IsGlobalPref));
				index++;
			}
		}

		public PlayerPrefsList(string key, List<T> value, bool isGlobalPref = false)
		{
			m_Key = key;
			m_IsGlobalPref = isGlobalPref;
			Set(value);
		}

		private void Set(List<T> value)
		{
			m_Values = value;
			int index = 0;
			while (PlayerPrefs.HasKey(IndexKey(index)))
			{
				if (index < m_Values.Count)
				{
					// Add
					SetIndex(index, m_Values[index], m_IsGlobalPref);
				}
				else
				{
					// Key exist outside list, remove it
					if (m_IsGlobalPref)
					{
						PlayerPrefs.DeleteGlobalKey(IndexKey(index));
					}
					else
					{
						PlayerPrefs.DeleteKey(IndexKey(index));
					}
				}
				index++;
			}
		}

		private void WriteAllPrefsValues()
		{
			for (int i = 0; i < m_Values.Count; ++i)
			{
				SetIndex(i, m_Values[i], m_IsGlobalPref);
			}
		}

		private void ClearAllPrefsKeys()
		{
			for (int i = 0; i < m_Values.Count; ++i)
			{
				PlayerPrefs.DeleteKey(IndexKey(i));
			}
		}

		#region List<T> Wrappers
		public T this[int index]
		{
			get
			{
				return m_Values[index];
			}
			set
			{
				m_Values[index] = value;
				SetIndex(index, value, m_IsGlobalPref);
			}
		}

		public int Count
		{
			get
			{
				return m_Values.Count;
			}
		}

		public int Capacity
		{
			get
			{
				return m_Values.Capacity;
			}
			set
			{
				m_Values.Capacity = value;
			}
		}

		public void Add(T value)
		{
			m_Values.Add(value);
			SetIndex(m_Values.Count - 1, value, m_IsGlobalPref);
		}

		public void AddRange(IEnumerable<T> collection)
		{
			int index = 0;
			foreach (T value in collection)
			{
				SetIndex(m_Values.Count - 1 + index, value, m_IsGlobalPref);
				index++;
			}
			m_Values.AddRange(collection);
		}

		public ReadOnlyCollection<T> AsReadOnly()
		{
			return m_Values.AsReadOnly();
		}

		public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
		{
			return m_Values.BinarySearch(index, count, item, comparer);
		}

		public int BinarySearch(T item)
		{
			return m_Values.BinarySearch(item);
		}

		public int BinarySearch(T item, IComparer<T> comparer)
		{
			return m_Values.BinarySearch(item, comparer);
		}

		public void Clear()
		{
			ClearAllPrefsKeys();
			m_Values.Clear();
		}

		public bool Contains(T item)
		{
			return m_Values.Contains(item);
		}

		public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
		{
			return m_Values.ConvertAll(converter);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			m_Values.CopyTo(array, arrayIndex);
		}

		public void CopyTo(T[] array)
		{
			m_Values.CopyTo(array);
		}

		public void CopyTo(int index, T[] array, int arrayIndex, int count)
		{
			m_Values.CopyTo(index, array, arrayIndex, count);
		}

		public bool Exists(Predicate<T> match)
		{
			return m_Values.Exists(match);
		}

		public T Find(Predicate<T> match)
		{
			return m_Values.Find(match);
		}

		public List<T> FindAll(Predicate<T> match)
		{
			return m_Values.FindAll(match);
		}

		public int FindIndex(int startIndex, int count, Predicate<T> match)
		{
			return m_Values.FindIndex(startIndex, count, match);
		}

		public int FindIndex(int startIndex, Predicate<T> match)
		{
			return m_Values.FindIndex(startIndex, match);
		}

		public int FindIndex(Predicate<T> match)
		{
			return m_Values.FindIndex(match);
		}

		public T FindLast(Predicate<T> match)
		{
			return m_Values.FindLast(match);
		}

		public int FindLastIndex(int startIndex, int count, Predicate<T> match)
		{
			return m_Values.FindLastIndex(startIndex, count, match);
		}

		public int FindLastIndex(int startIndex, Predicate<T> match)
		{
			return m_Values.FindLastIndex(startIndex, match);
		}

		public int FindLastIndex(Predicate<T> match)
		{
			return m_Values.FindLastIndex(match);
		}

		public void ForEach(Action<T> action)
		{
			m_Values.ForEach(action);
		}

		public List<T>.Enumerator GetEnumerator()
		{
			return m_Values.GetEnumerator();
		}

		public List<T> GetRange(int index, int count)
		{
			return m_Values.GetRange(index, count);
		}

		public int IndexOf(T item, int index, int count)
		{
			return m_Values.IndexOf(item, index, count);
		}

		public int IndexOf(T item, int index)
		{
			return m_Values.IndexOf(item, index);
		}

		public int IndexOf(T item)
		{
			return m_Values.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			m_Values.Insert(index, item);
			WriteAllPrefsValues();
		}

		public void InsertRange(int index, IEnumerable<T> collection)
		{
			m_Values.InsertRange(index, collection);
			WriteAllPrefsValues();
		}

		public int LastIndexOf(T item)
		{
			return m_Values.LastIndexOf(item);
		}

		public int LastIndexOf(T item, int index)
		{
			return m_Values.LastIndexOf(item, index);
		}

		public int LastIndexOf(T item, int index, int count)
		{
			return m_Values.LastIndexOf(item, index, count);
		}

		public bool Remove(T item)
		{
			if (m_Values.Remove(item))
			{
				DeletePlayerPrefs(m_Values.Count, 1); // Remove key from end
				WriteAllPrefsValues();
				return true;
			}
			return false;
		}

		public int RemoveAll(Predicate<T> match)
		{
			ClearAllPrefsKeys();
			int removed = m_Values.RemoveAll(match);
			if (removed > 0)
			{
				WriteAllPrefsValues();
			}
			return removed;
		}

		public bool RemoveAt(int index)
		{
			if (index < m_Values.Count)
			{
				m_Values.RemoveAt(index);
				DeletePlayerPrefs(m_Values.Count, 1); // Remove key from end
				WriteAllPrefsValues();
				return true;
			}
			return false;
		}

		public void RemoveRange(int index, int count)
		{
			m_Values.RemoveRange(index, count);
			DeletePlayerPrefs(index, count);
			WriteAllPrefsValues();
		}

		private void DeletePlayerPrefs(int start, int count)
		{
			int index = 0;
			while (index < count)
			{
				if (m_IsGlobalPref)
				{
					PlayerPrefs.DeleteGlobalKey(IndexKey(index + start));
				}
				else
				{
					PlayerPrefs.DeleteKey(IndexKey(index + start));
				}
				index++;
			}
		}

		public void Reverse(int index, int count)
		{
			m_Values.Reverse(index, count);
			WriteAllPrefsValues();
		}

		public void Reverse()
		{
			m_Values.Reverse();
			WriteAllPrefsValues();
		}

		public void Sort(Comparison<T> comparison)
		{
			m_Values.Sort(comparison);
			WriteAllPrefsValues();
		}

		public void Sort(int index, int count, IComparer<T> comparer)
		{
			m_Values.Sort(index, count, comparer);
			WriteAllPrefsValues();
		}

		public void Sort()
		{
			m_Values.Sort();
			WriteAllPrefsValues();
		}

		public void Sort(IComparer<T> comparer)
		{
			m_Values.Sort(comparer);
			WriteAllPrefsValues();
		}

		public T[] ToArray()
		{
			return m_Values.ToArray();
		}

		public void TrimExcess()
		{
			ClearAllPrefsKeys();
			m_Values.TrimExcess();
			WriteAllPrefsValues();
		}

		public bool TrueForAll(Predicate<T> match)
		{
			return m_Values.TrueForAll(match);
		}
		#endregion

		#region ICollection Implementation
		int ICollection.Count
		{
			get
			{
				return (m_Values as ICollection).Count;
			}
		}

		bool ICollection.IsSynchronized
		{
			get
			{
				return (m_Values as ICollection).IsSynchronized;
			}
		}

		object ICollection.SyncRoot
		{
			get
			{
				return (m_Values as ICollection).SyncRoot;
			}
		}

		void ICollection.CopyTo(Array array, int index)
		{
			(m_Values as ICollection).CopyTo(array, index);
		}
		#endregion

		#region ICollection<T> Implementation 
		int ICollection<T>.Count
		{
			get
			{
				return (m_Values as ICollection<T>).Count;
			}
		}

		bool ICollection<T>.IsReadOnly
		{
			get
			{
				return (m_Values as ICollection<T>).IsReadOnly;
			}
		}

		void ICollection<T>.Add(T item)
		{
			Add(item);
		}

		void ICollection<T>.Clear()
		{
			Clear();
		}

		bool ICollection<T>.Contains(T item)
		{
			return Contains(item);
		}

		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			CopyTo(array, arrayIndex);
		}

		bool ICollection<T>.Remove(T item)
		{
			return Remove(item);
		}
		#endregion

		#region IEnumerator<T> Implementation
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return (m_Values as IEnumerable<T>).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (m_Values as IEnumerable<T>).GetEnumerator();
		}
		#endregion

		#region IList Implementation
		bool IList.IsFixedSize
		{
			get
			{
				return (m_Values as IList).IsFixedSize;
			}
		}

		bool IList.IsReadOnly
		{
			get
			{
				return (m_Values as IList).IsReadOnly;
			}
		}

		object IList.this[int index]
		{
			get
			{
				return (m_Values as IList)[index];
			}
			set
			{
				SetIndex(index, (T)value, m_IsGlobalPref);
			}
		}

		int IList.Add(object value)
		{
			Add((T)value);
			return m_Values.Count;
		}

		void IList.Clear()
		{
			Clear();
		}

		bool IList.Contains(object value)
		{
			return (m_Values as IList).Contains(value);
		}

		int IList.IndexOf(object value)
		{
			return (m_Values as IList).IndexOf(value);
		}

		void IList.Insert(int index, object value)
		{
			Insert(index, (T)value);
		}

		void IList.Remove(object value)
		{
			Remove((T)value);
		}

		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		void IList<T>.RemoveAt(int index)
		{
			RemoveAt(index);
		}
		#endregion
	}
}