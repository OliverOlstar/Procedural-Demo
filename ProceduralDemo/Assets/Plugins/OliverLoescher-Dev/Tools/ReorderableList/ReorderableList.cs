#if UNITY_2021_1_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ODev
{
	public abstract class ReorderableListBase<T> : IList<T>
	{
		protected abstract List<T> List { get; }

		#region IList
		public T this[int index]
		{
			get => List[index];
			set => List[index] = value;
		}

		public int IndexOf(T item) => List.IndexOf(item);
		public void Insert(int index, T item) => List.Insert(index, item);
		public void RemoveAt(int index) => List.RemoveAt(index);
		#endregion

		#region ICollection
		public int Count => List.Count;
		public bool IsReadOnly => false;

		public void Add(T item) => List.Add(item);
		public void Clear() => List.Clear();
		public bool Contains(T item) => List.Contains(item);
		public void CopyTo(T[] array, int arrayIndex) => List.CopyTo(array, arrayIndex);
		public bool Remove(T item) => List.Remove(item);
		#endregion

		#region IEnumerator
		IEnumerator<T> IEnumerable<T>.GetEnumerator() => List.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => List.GetEnumerator();
		#endregion
	}

	/// <summary>Empty class to attach property drawer to</summary>
	public abstract class ReorderableListGeneric<T> : ReorderableListBase<T> { }

	[System.Serializable]
	public class ReorderableList<T> : ReorderableListGeneric<T>
	{
		[SerializeField]
		private List<T> m_List = new();
		protected override List<T> List => m_List;
	}

	[System.Serializable]
	public class ReorderableListFlatClass<T> : ReorderableListGeneric<T> where T : class
	{
		[SerializeField, Flatten(false, false, false)]
		private List<T> m_List = new();
		protected override List<T> List => m_List;
	}

	[System.Serializable]
	public class ReorderableGameObjectList : ReorderableList<GameObject>
	{

	}

	[System.Serializable]
	public class ReorderableStringList : ReorderableList<string>
	{

	}

	[System.Serializable]
	public class ReorderableFloatList : ReorderableList<float>
	{

	}

	[System.Serializable]
	public class ReorderableIntList : ReorderableList<int>
	{

	}
}
#endif
