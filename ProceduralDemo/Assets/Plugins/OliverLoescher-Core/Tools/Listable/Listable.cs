using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OCore
{
	public abstract class ListableBase // Attach property drawer
	{

	}

	public abstract class ListableSingleStyleAttribute : System.Attribute { }
	public abstract class ListableListStyleAttribute : System.Attribute { }
	namespace ListableStyle
	{
		public class AllowEmptyAttribute : System.Attribute { }

		namespace Single
		{
			public class DropdownAttribute : ListableSingleStyleAttribute { }
			public class ArrayAttribute : ListableSingleStyleAttribute { }
			public class ReorderableAttribute : ListableSingleStyleAttribute { }
		}

		namespace List
		{
			public class ReorderableAttribute : ListableListStyleAttribute { }
		}
	}

	public abstract class ListableInternal<T> : ListableBase, IList<T>
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

	public abstract class Listable<T> : ListableInternal<T>
	{
		[SerializeField]
		protected List<T> m_List = new();
		protected override List<T> List => m_List;

		public Listable(T defaultValue)
		{
			m_List.Add(defaultValue);
		}
	}

	[ListableStyle.AllowEmpty]
	public abstract class ListableEmpty<TClass> : ListableInternal<TClass>
	{
		[SerializeField]
		protected List<TClass> m_List = new();
		protected override List<TClass> List => m_List;
	}

	public abstract class ListableClassFlat<TClass> : ListableInternal<TClass> where TClass : new()
	{
		[SerializeField, Flatten(false, false, false)]
		protected List<TClass> m_List = new();
		protected override List<TClass> List => m_List;

		public ListableClassFlat()
		{
			m_List.Add(new TClass());
		}

		public ListableClassFlat(TClass defaultValue)
		{
			m_List.Add(defaultValue);
		}
	}

	public abstract class ListableUnityObject<TObj> : Listable<TObj> where TObj : Object
	{
		public ListableUnityObject() : base(null) { }
	}

	[System.Serializable]
	public class ListableString : Listable<string>
	{
		public ListableString(string defaultValue) : base(defaultValue) { }
	}
	[System.Serializable]
	public class ListableInt : Listable<int>
	{
		public ListableInt(int defaultValue) : base(defaultValue) { }
	}
	[System.Serializable]
	public class ListableFloat : Listable<float>
	{
		public ListableFloat(float defaultValue) : base(defaultValue) { }
	}
	[System.Serializable]
	public class ListableBool : Listable<bool>
	{
		public ListableBool(bool defaultValue) : base(defaultValue) { }
	}
	[System.Serializable]
	public class ListableGameObject : ListableUnityObject<GameObject> { }
	[System.Serializable]
	public class ListableTransform : ListableUnityObject<Transform> { }
}
