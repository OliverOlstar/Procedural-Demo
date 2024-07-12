
using System.Collections.Generic;
using UnityEngine;

namespace ODev
{
	public static class MonoDirector
	{
		private static Dictionary<System.Type, HashSet<Object>> s_Directors = new();

		public static void Register(Object obj) => RegisterInternal(obj.GetType(), obj);

		public static void RegisterAsType<T>(T obj)
		{
			if (obj is Object unityObj)
			{
				RegisterInternal(typeof(T), unityObj);
				return;
			}
			DebugUtil.DevException($"MonoDirector.Register() Cannot register object of type {obj.GetType().Name} as it does not inherrit from UnityEngine.Object");
		}

		private static void RegisterInternal(System.Type key, Object obj)
		{
			if (!s_Directors.TryGetValue(key, out HashSet<Object> set))
			{
				set = new HashSet<Object>();
				s_Directors.Add(key, set);
			}
			if (!set.Contains(obj))
			{
				set.Add(obj);
			}
		}

		public static void Deregister(Object obj) => DeregisterInternal(obj.GetType(), obj);
		public static void DeregisterAsType<T>(T obj)
		{
			if (obj is Object unityObj)
			{
				DeregisterInternal(typeof(T), unityObj);
				return;
			}
			DebugUtil.DevException($"MonoDirector.Deregister() Cannot register object of type {obj.GetType().Name} as it does not inherrit from UnityEngine.Object");
		}
		private static void DeregisterInternal(System.Type key, Object obj)
		{

			if (!s_Directors.TryGetValue(key, out HashSet<Object> set))
			{
				return;
			}
			if (!set.Contains(obj))
			{
				return;
			}
			set.Remove(obj);
		}

		public static bool TryGet<T>(out T obj) where T : class
		{
			obj = null;
			if (s_Directors.TryGetValue(typeof(T), out HashSet<Object> set))
			{
				foreach (Object i in set)
				{
					obj = i as T;
					if (obj != null)
					{
						return true;
					}
				}
			}
			return obj != null;
		}

		public static T Get<T>() where T : class
		{
			return TryGet(out T obj) ? obj : throw new System.ArgumentNullException($"ODev.MonoDirector.Get() No director of type {typeof(T).Name} exists");
		}

		public static void GetAll<T>(List<T> list) where T : class
		{
			if (s_Directors.TryGetValue(typeof(T), out HashSet<Object> set))
			{
				foreach (Object obj in set)
				{
					if (obj is T listObj)
					{
						list.Add(listObj);
					}
				}
			}
		}

		public static void Get<T>(List<T> list) where T : Object
		{
			if (s_Directors.TryGetValue(typeof(T), out HashSet<Object> set))
			{
				foreach (Object obj in set)
				{
					if (obj is T objOfType)
					{
						list.Add(objOfType);
					}
				}
			}
		}

		public static bool TryGetRecursive<T>(out T obj) where T : class
		{
			obj = null;
			System.Type key = typeof(T);
			foreach (KeyValuePair<System.Type, HashSet<Object>> pair in s_Directors)
			{
				if (key.IsAssignableFrom(pair.Key))
				{
					obj = pair.Value.GetEnumerator().Current as T;
					if (obj != null)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static T GetRecursive<T>() where T : class
		{
			return TryGetRecursive(out T obj) ? obj : throw new System.ArgumentNullException($"ODev.GetRecursive.Get() No director assignable from type {typeof(T).Name} exists");
		}

		public static void GetRecursive<T>(List<T> list)
		{
			System.Type key = typeof(T);
			foreach (KeyValuePair<System.Type, HashSet<Object>> pair in s_Directors)
			{
				if (key.IsAssignableFrom(pair.Key))
				{
					foreach (Object obj in pair.Value)
					{
						if (obj is T objOfType)
						{
							list.Add(objOfType);
						}
					}
				}
			}
		}
	}
}

