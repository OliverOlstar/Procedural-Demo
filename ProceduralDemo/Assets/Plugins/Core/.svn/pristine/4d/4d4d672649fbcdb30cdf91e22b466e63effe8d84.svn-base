
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public static class MonoDirector
	{
		private static Dictionary<System.Type, HashSet<UnityEngine.Object>> s_Directors = new Dictionary<System.Type, HashSet<Object>>();

		public static void Register(UnityEngine.Object obj) => RegisterInternal(obj.GetType(), obj);

		public static void RegisterAsType<T>(T obj)
		{
			if (obj is UnityEngine.Object unityObj)
			{
				RegisterInternal(typeof(T), unityObj);
				return;
			}
			Core.DebugUtil.DevException($"MonoDirector.Register() Cannot register object of type {obj.GetType().Name} as it does not inherrit from UnityEngine.Object");
		}

		private static void RegisterInternal(System.Type key, UnityEngine.Object obj)
		{
			if (!s_Directors.TryGetValue(key, out HashSet<UnityEngine.Object> set))
			{
				set = new HashSet<UnityEngine.Object>();
				s_Directors.Add(key, set);
			}
			if (!set.Contains(obj))
			{
				set.Add(obj);
			}
		}

		public static void Deregister(UnityEngine.Object obj) => DeregisterInternal(obj.GetType(), obj);
		public static void DeregisterAsType<T>(T obj)
		{
			if (obj is UnityEngine.Object unityObj)
			{
				DeregisterInternal(typeof(T), unityObj);
				return;
			}
			Core.DebugUtil.DevException($"MonoDirector.Deregister() Cannot register object of type {obj.GetType().Name} as it does not inherrit from UnityEngine.Object");
		}
		private static void DeregisterInternal(System.Type key, UnityEngine.Object obj)
		{

			if (!s_Directors.TryGetValue(key, out HashSet<UnityEngine.Object> set))
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
			if (s_Directors.TryGetValue(typeof(T), out HashSet<UnityEngine.Object> set))
			{
				foreach (UnityEngine.Object i in set)
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
			return TryGet(out T obj) ? obj : throw new System.ArgumentNullException($"Core.MonoDirector.Get() No director of type {typeof(T).Name} exists");
		}

		public static void GetAll<T>(List<T> list) where T : class
		{
			if (s_Directors.TryGetValue(typeof(T), out HashSet<UnityEngine.Object> set))
			{
				foreach (UnityEngine.Object obj in set)
				{
					if (obj is T listObj)
					{
						list.Add(listObj);
					}
				}
			}
		}

		public static void Get<T>(List<T> list) where T : UnityEngine.Object
		{
			if (s_Directors.TryGetValue(typeof(T), out HashSet<UnityEngine.Object> set))
			{
				foreach (UnityEngine.Object obj in set)
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
			foreach (KeyValuePair<System.Type, HashSet<UnityEngine.Object>> pair in s_Directors)
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
			return TryGetRecursive(out T obj) ? obj : throw new System.ArgumentNullException($"Core.GetRecursive.Get() No director assignable from type {typeof(T).Name} exists");
		}

		public static void GetRecursive<T>(List<T> list)
		{
			System.Type key = typeof(T);
			foreach (KeyValuePair<System.Type, HashSet<UnityEngine.Object>> pair in s_Directors)
			{
				if (key.IsAssignableFrom(pair.Key))
				{
					foreach (UnityEngine.Object obj in pair.Value)
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

