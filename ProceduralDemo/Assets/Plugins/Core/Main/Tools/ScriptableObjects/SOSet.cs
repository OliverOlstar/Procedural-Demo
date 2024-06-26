using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	[System.Serializable]
	public class SOSetBase
	{

	}

	[System.Serializable]
	public class SOSet<T> : SOSetBase where T : SOCacheName
	{
		[SerializeField]
		private List<T> m_Set = new();

		private HashSet<int> m_Hash = null;

		private void Initialize()
		{
			if (m_Hash == null)
			{
				m_Hash = new HashSet<int>();
				foreach (T i in m_Set)
				{
					if (i != null)
					{
						m_Hash.Add(i.NameHash);
					}
				}
			}
		}

		public int Count
		{
			get
			{
				Initialize();
				return m_Set.Count;
			}
		}

		public StructEnumerable.List<T> Get
		{
			get
			{
				Initialize();
				return m_Set;
			}
		}

		public bool Contains(T item)
		{
			if (item == null)
			{
				return false;
			}
			Initialize();
			return m_Hash.Contains(item.NameHash);
		}

		public bool ContainsAny(SOSet<T> set)
		{
			Initialize();
			foreach (T item in set.m_Set)
			{
				if (m_Hash.Contains(item.NameHash))
				{
					return true;
				}
			}
			return false;
		}

		public void Add(T item)
		{
			Initialize();
			int key = item.NameHash;
			if (!m_Hash.Contains(key))
			{
				m_Hash.Add(key);
				m_Set.Add(item);
			}
		}

		public void Remove(T item)
		{
			Initialize();
			int key = item.NameHash;
			if (m_Hash.Contains(key))
			{
				m_Hash.Remove(key);
				for (int i = 0; i < m_Set.Count; i++)
				{
					if (m_Set[i].NameHash == key)
					{
						m_Set.RemoveAt(i);
						break;
					}
				}
			}
		}

		public bool ContainsAll(SOSet<T> set)
		{
			Initialize();
			foreach (T item in set.m_Set)
			{
				if (!m_Hash.Contains(item.NameHash))
				{
					return false;
				}
			}
			return true;
		}

		public override string ToString()
		{
			string s = GetType().ToString() + "(";
			foreach (T i in m_Set)
			{
				s += (i == null ? "null" : i.Name) + ", ";
			}
			s += ")";
			return s;
		}
	}
}
