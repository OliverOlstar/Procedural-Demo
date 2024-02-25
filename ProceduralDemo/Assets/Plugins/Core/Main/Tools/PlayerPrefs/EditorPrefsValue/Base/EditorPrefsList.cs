using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;

namespace Core
{
	public abstract class EditorPrefsList<T> : EditorPrefsValue<List<T>>, ICollection<T>, IEnumerable<T>, IEnumerable, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection, IList
	{
		public static implicit operator List<T>(EditorPrefsList<T> value)
		{
			return value.Get();
		}

		protected abstract T ReadValue(int index);
		protected abstract void WriteValue(int index, T value);

		private List<T> m_Values;

		public EditorPrefsList(string key)
			: base(key)
		{
			m_Values = new List<T>();
			int index = 0;
			while (EditorPrefs.HasKey(Key + index))
			{
				m_Values.Add(ReadValue(index));
				index++;
			}
		}

		private EditorPrefsList(string key, IEnumerable<T> value)
			: base(key, new List<T>(value))
		{
		}

		protected sealed override List<T> Get()
		{
			return new List<T>(m_Values);
		}

		protected sealed override void Set(List<T> value)
		{
			Clear();
			m_Values = new List<T>(value);
			WriteAllPrefsValues();
		}

		private void WriteAllPrefsValues()
		{
			for (int i = 0; i < m_Values.Count; ++i)
			{
				WriteValue(i, m_Values[i]);
			}
		}

		private void ClearAllPrefsKeys()
		{
			for (int i = 0; i < m_Values.Count; ++i)
			{
				EditorPrefs.DeleteKey(Key + i);
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
				WriteValue(index, value);
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
			WriteValue(m_Values.Count - 1, value);
		}

		public void AddRange(IEnumerable<T> collection)
		{
			int index = 0;
			foreach (var value in collection)
			{
				WriteValue(m_Values.Count - 1 + index, value);
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
			ClearAllPrefsKeys();
			bool removed = m_Values.Remove(item);
			WriteAllPrefsValues();
			return removed;
		}

		public int RemoveAll(Predicate<T> match)
		{
			ClearAllPrefsKeys();
			int removed = m_Values.RemoveAll(match);
			WriteAllPrefsValues();
			return removed;
		}

		public void RemoveAt(int index)
		{
			ClearAllPrefsKeys();
			m_Values.RemoveAt(index);
			WriteAllPrefsValues();
		}

		public void RemoveRange(int index, int count)
		{
			ClearAllPrefsKeys();
			m_Values.RemoveRange(index, count);
			WriteAllPrefsValues();
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
				WriteValue(index, (T)value);
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
		#endregion
	}
}
