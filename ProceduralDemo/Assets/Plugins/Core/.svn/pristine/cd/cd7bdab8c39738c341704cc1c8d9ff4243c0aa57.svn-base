using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public abstract class ObjectPoolBase
	{
		public abstract int PooledCount { get; }

#if UNITY_EDITOR
		public static bool _EditorDebugMode = false;

		private static List<ObjectPoolBase> s_EditorPools = new List<ObjectPoolBase>();
		public static IEnumerable<ObjectPoolBase> _EditorPools => s_EditorPools;

		public ObjectPoolBase()
		{
			s_EditorPools.Add(this);
			s_EditorPools.Sort(Compare);
		}

		private static int Compare(ObjectPoolBase a, ObjectPoolBase b)
		{
			return a._EditorName.CompareTo(b._EditorName);
		}

		public abstract int _EditorActiveCount { get; }
		public abstract IEnumerable<string> _EditorActiveStackTraces { get; }
		public abstract string _EditorName { get; }
		public abstract IEnumerable<KeyValuePair<object, string>> _EditorPooledObjects { get; }
		public abstract void _EditorClear();

		[UnityEditor.InitializeOnLoadMethod]
		static void OnLoad()
		{
			foreach (ObjectPoolBase pool in s_EditorPools)
			{
				pool._EditorClear();
			}
		}
#endif
	}

	public class ObjectPool<T> : ObjectPoolBase where T : class, new()
	{
		private Dictionary<T, string> m_Pooled = new Dictionary<T, string>();
		public override int PooledCount => m_Pooled.Count;

#if UNITY_EDITOR
		private Dictionary<int, string> m_EditorActive = new Dictionary<int, string>(); // Don't hold references to active objects so we don't affect memory management

		public override int _EditorActiveCount => m_EditorActive.Count;
		public override IEnumerable<string> _EditorActiveStackTraces => m_EditorActive.Values;
		public override string _EditorName => typeof(T).Name;

		public override IEnumerable<KeyValuePair<object, string>> _EditorPooledObjects
		{
			get
			{
				foreach (KeyValuePair<T, string> pair in m_Pooled)
				{
					yield return new KeyValuePair<object, string>(pair.Key, pair.Value);
				}
			}
		}

		public override void _EditorClear()
		{
			m_Pooled.Clear();
			m_EditorActive.Clear();
		}
#endif

		public void Return(T obj)
		{
			bool dispose = !m_Pooled.ContainsKey(obj);
			if (dispose)
			{
				m_Pooled.Add(obj, null);
			}
#if UNITY_EDITOR
			if (_EditorDebugMode)
			{
				if (dispose)
				{
					m_Pooled[obj] = System.Environment.StackTrace;
				}
				int hash = obj.GetHashCode();
				if (m_EditorActive.ContainsKey(hash))
				{
					m_EditorActive.Remove(hash);
				}
				//if (dispose)
				//{
				//	Debug.Log($"[{typeof(T).Name} Pool] Return {obj} {hash} {Time.time}");
				//}
				//else
				//{
				//	Debug.LogWarning($"[{typeof(T).Name} Pool] Return failed {obj} {hash} {Time.time}");
				//}
			}
#endif
		}

		public T Request()
		{
			T obj = null;
			bool pooled = m_Pooled.Count > 0;
			if (pooled)
			{
				Dictionary<T, string>.KeyCollection.Enumerator enumerator = m_Pooled.Keys.GetEnumerator();
				enumerator.MoveNext();
				obj = enumerator.Current;
				m_Pooled.Remove(obj);
			}
			else
			{
				obj = System.Activator.CreateInstance<T>();
			}
#if UNITY_EDITOR
			if (_EditorDebugMode)
			{
				int hash = obj.GetHashCode();
				if (!m_EditorActive.ContainsKey(hash))
				{
					m_EditorActive.Add(hash, System.Environment.StackTrace);
					//Debug.Log($"[{typeof(T).Name} Pool] Request {(pooled ? "pooled" : "instantiated")} {obj} {hash} {Time.time}");
				}
				//else
				//{
				//	Debug.LogWarning($"[{typeof(T).Name} Pool] Request {(pooled ? "pooled" : "instantiated")} {obj} {hash} already exists? Maybe a hash collision? {Time.time}");
				//}
			}
#endif
			return obj;
		}
	}

	public interface IPooledObject
	{
		int PoolID { get; }
		void Dispose();
	}

	public abstract class PooledObject<T> : IPooledObject where T : class, IPooledObject, new()
	{
		private static Core.ObjectPool<T> s_Pool = new Core.ObjectPool<T>();
		private static int s_ID = 0;

		private int m_ID = 0;
		public int PoolID => m_ID;

		public PooledObject()
		{
			m_ID = ++s_ID;
		}

		protected static T Request()
		{
			return s_Pool.Request();
		}

		public void Dispose()
		{
			OnDispose();
			s_Pool.Return(this as T);
			m_ID = ++s_ID;
		}

		protected virtual void OnDispose()
		{

		}

		public override string ToString()
		{
			return $"{GetType().Name}({PoolID})";
		}
	}

	public struct PooledHandle<T> where T : class, IPooledObject
	{
		private static readonly int INVALID_ID = 0;

		private T m_Instance;
		private int m_ID;
		public int PoolID => m_ID;

		public PooledHandle(T instance)
		{
			m_Instance = instance ?? throw new System.ArgumentNullException($"PooledHandle() Handle {typeof(T).Name} cannot be constructed with null instance");
			m_ID = m_Instance.PoolID != INVALID_ID || Core.Util.IsRelease() ? m_Instance.PoolID :
				throw new System.InvalidOperationException($"PooledHandle() Handle {typeof(T).Name} instance has invalid ID, is it set up to pool properly?");
		}

		public bool IsValid() { return m_Instance != null && m_Instance.PoolID == m_ID; }

		public T Instance
		{
			get
			{
				if (m_Instance == null)
				{
					throw new System.InvalidOperationException($"PooledHandle.Instance Handle {typeof(T).Name} is null it might be assigned first");
				}
#if !RELEASE
				if (m_Instance.PoolID == INVALID_ID)
				{
					throw new System.InvalidOperationException($"PooledHandle.Instance {typeof(T).Name} handle has invalid ID, it needs to be assigned accessing Instance");
				}
				if (m_Instance.PoolID != m_ID)
				{
					throw new System.InvalidOperationException($"PooledHandle.Instance {typeof(T).Name} handle ID {m_ID} has been disposed and assigned new ID {m_Instance.PoolID}");
				}
#endif
				return m_Instance;
			}
		}

		public void Dispose()
		{
			if (m_Instance != null)
			{
				m_Instance.Dispose();
				m_Instance = null;
				m_ID = INVALID_ID;
			}
		}

		public static implicit operator PooledHandle<T>(T instance) => instance != null ? new PooledHandle<T>(instance) : throw new System.ArgumentNullException("Cannot assign a null object to a PooledHandle");

		public static implicit operator T(PooledHandle<T> handle) => handle.Instance;
	}
}
