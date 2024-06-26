using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public interface IListableWrapper<TObj>
	{
		TObj Item { get; set; }
	}

	/// <summary>
	/// Wraps an object that has a property drawer and then flattens the wrapper so it's invisible and we just see the nice
	/// the nice property drawer for the wrapped object
	/// </summary>
	public abstract class ListableForPropertyDrawers<Wrapper, TObj> : ListableBase, IList<TObj> 
		where Wrapper : IListableWrapper<TObj>, new()
	{
		[SerializeField, Flatten(false, false, false)]
		private List<Wrapper> m_List = new();

		public ListableForPropertyDrawers(TObj defaultValue)
		{
			m_List.Add(new Wrapper() { Item = defaultValue });
		}

		#region IList
		public TObj this[int index]
		{
			get => m_List[index].Item;
			set => m_List[index] = new Wrapper() { Item = value };
		}

		public int IndexOf(TObj item)
		{
			int length = m_List.Count;
			for (int i = 0; i < length; i++)
			{
				if (Equals(m_List[i].Item, item))
				{
					return i;
				}
			}
			return -1;
		}
		public void Insert(int index, TObj item) => m_List.Insert(index, new Wrapper() { Item = item });
		public void RemoveAt(int index) => m_List.RemoveAt(index);
		#endregion

		#region ICollection
		public int Count => m_List.Count;
		public bool IsReadOnly => false;

		public void Add(TObj item) => m_List.Add(new Wrapper() { Item = item });
		public void Clear() => m_List.Clear();
		public bool Contains(TObj item) => IndexOf(item) >= 0;
		public void CopyTo(TObj[] array, int arrayIndex)
		{
			for (int i = 0; i < m_List.Count && i + arrayIndex < array.Length; i++)
			{
				array[arrayIndex + i] = m_List[i].Item;
			}
		}
		public bool Remove(TObj item)
		{
			int index = IndexOf(item);
			if (index < 0)
			{
				return false;
			}
			RemoveAt(index);
			return true;
		}
		#endregion

		#region IEnumerator
		IEnumerator<TObj> IEnumerable<TObj>.GetEnumerator()
		{
			foreach (Wrapper wrapper in m_List)
			{
				yield return wrapper.Item;
			}
		}
		IEnumerator IEnumerable.GetEnumerator() => m_List.GetEnumerator();
		#endregion
	}

	public abstract class ListableSO<TObj> : ListableForPropertyDrawers<ListableSO<TObj>.Wrapper<TObj>, TObj>
		where TObj : ScriptableObject
	{
		protected ListableSO() : base(null)
		{

		}

		[System.Serializable]
		public struct Wrapper<T> : IListableWrapper<T>
		{
			[SerializeField, UberPicker.AssetNonNull]
			private T m_Item;
			public T Item
			{
				get => m_Item;
				set => m_Item = value;
			}
		}
	}
}
