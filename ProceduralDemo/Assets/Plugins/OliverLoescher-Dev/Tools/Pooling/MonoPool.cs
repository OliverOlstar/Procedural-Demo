using System.Collections.Generic;
using UnityEngine;

namespace ODev
{
	public class MonoPool<T> where T : Component
	{
		private List<T> m_Pooled = new(10);

		private Transform m_PooledParent = null;

		private int? m_PrefabID = null;
		private string m_PrefabName = null;

		private Transform GetPooledParent()
		{
			if (m_PooledParent == null)
			{
				m_PooledParent = new GameObject(typeof(T).Name + "Pool").transform;
			}
			return m_PooledParent;
		}

		public void Return(T obj)
		{
			foreach (T item in m_Pooled)
			{
				if (item == obj) // Don't allow an item to be returned multiple times
				{
					return;
				}
			}

			obj.gameObject.SetActive(false);
			obj.transform.parent = GetPooledParent();

			for (int i = 0; i < m_Pooled.Count; i++)
			{
				if (m_Pooled[i] == null)
				{
					m_Pooled[i] = obj;
					return;
				}
			}

			m_Pooled.Add(obj);

		}

		public T Request(Vector3 position, Quaternion rotation)
		{
			if (TryGet(position, rotation, out T obj))
			{
				return obj;
			}
			GameObject go = new(typeof(T).Name);
			go.transform.SetPositionAndRotation(position, rotation);
			return go.AddComponent<T>();
		}

		public T RequestPrefab(GameObject prefab, Vector3 position, Quaternion rotation)
		{
			// Once start requesting prefabs we can only request the same prefab, otherwise the caller can recieve an instance from the
			// pool that is not from the prefab that they asked for
			if (!m_PrefabID.HasValue)
			{
				m_PrefabID = prefab.GetInstanceID();
				m_PrefabName = prefab.name;
			}
			else if (m_PrefabID.Value != prefab.GetInstanceID())
			{
				DebugUtil.DevException($"{GetType().Name}.RequestPrefab() Requested prefab '{prefab.name}' but " +
					$"the pool is already filled with a different prefab '{m_PrefabName}'.");
			}
			if (TryGet(position, rotation, out T obj))
			{
				return obj;
			}
			GameObject go = Object.Instantiate(prefab, position, rotation);
			return go.GetComponent<T>();
		}

		private bool TryGet(Vector3 position, Quaternion rotation, out T obj)
		{
			for (int i = 0; i < m_Pooled.Count; i++)
			{
				T o = m_Pooled[i];
				if (o == null)
				{
					continue;
				}
				m_Pooled[i] = null;
				obj = o;
				obj.transform.SetPositionAndRotation(position, rotation);
				obj.gameObject.SetActive(true);
				return true;
			}
			obj = null;
			return false;
		}
	}
}
