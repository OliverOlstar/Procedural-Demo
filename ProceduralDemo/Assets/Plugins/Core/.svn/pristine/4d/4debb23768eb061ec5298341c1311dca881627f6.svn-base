
using UnityEngine;
using System.Collections.Generic;

public class PoolList<T> : IEnumerable<T> where T : class
{
	List<T> m_List = null;

	public PoolList(int capacity)
	{
		m_List = new List<T>(capacity);
	}

	public T this[int index]
	{
		get
		{
			return m_List[index];
		}
		set
		{
			m_List[index] = value;
		}
	}

	public int Count { get { return m_List.Count; } }

	public void Add(T item)
	{
		Debug.Assert(m_List.Count < m_List.Capacity,
			"RefList.Add() Adding " + item + " List needs to be re-sized, consider initializing with more capacity " + m_List.Capacity);
		for (int i = 0; i < m_List.Count; i++)
		{
			if (m_List[i] == null)
			{
				m_List[i] = item;
				return;
			}
		}
		m_List.Add(item);
	}

	public void RemoveAt(int index)
	{
		if (index < 0 || index >= m_List.Count)
		{
			return;
		}
		m_List[index] = null;
	}

	public void Clear()
	{
		m_List.Clear();
	}

	public T GetAt(int index)
	{
		if (index < 0 || index >= m_List.Count)
		{
			return null;
		}
		return m_List[index];
	}

	public IEnumerator<T> GetEnumerator()
	{
		return m_List.GetEnumerator();
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
		return m_List.GetEnumerator();
	}
}
