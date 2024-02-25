using System;
using System.Collections.Generic;
using UnityEditor;

namespace Core
{
	/// <summary>
	/// Can Cache any reference type, it is sensitive to latest asset database changes 
	/// </summary>
	public class AssetDatabaseDepedentValue<T> where T : class
	{
		private T m_Value = null;
		private uint m_Version = 0;

		public bool TryGet(out T value)
		{
			if (m_Version == AssetDatabase.GlobalArtifactDependencyVersion)
			{
				value = m_Value;
				return true;
			}

			value = null;
			return false;
		}

		public void Set(T value)
		{
			m_Value = value;
			m_Version = AssetDatabase.GlobalArtifactDependencyVersion;
		}
	}

	/// <summary>
	/// Caches many values which are dependent on Asset Database changes identified by some key
	/// </summary>
	public class AssetDatabaseDependentValues<K, T> where T : class
	{
		private Dictionary<K, AssetDatabaseDepedentValue<T>> m_Dictionary = new Dictionary<K, AssetDatabaseDepedentValue<T>>();

		public bool TryGet(K key, out T value)
		{
			if (!m_Dictionary.TryGetValue(key, out AssetDatabaseDepedentValue<T> cache))
			{
				value = null;
				return false;
			}
			return cache.TryGet(out value);
		}

		public void Set(K key, T value)
		{
			if (!m_Dictionary.TryGetValue(key, out AssetDatabaseDepedentValue<T> cache))
			{
				cache = new AssetDatabaseDepedentValue<T>();
				m_Dictionary.Add(key, cache);
			}
			cache.Set(value);
		}
	}

	/// <summary>
	/// Most common use case for AssetDatabaseDependentValues is to cache some values for a Property Drawer
	/// </summary>
	public class PropertyDrawerCache<T> where T : class
	{
		private static AssetDatabaseDependentValues<string, T> s_Cache =
			new AssetDatabaseDependentValues<string, T>();
		public static string GetPropertyCacheKey(SerializedProperty property) =>
			Str.Build(property.serializedObject.targetObject.GetType().Name, ".", property.propertyPath);

		public static bool TryGetCache(string key, out T cache)
		{
			if (string.IsNullOrEmpty(key))
			{
				cache = null;
				return false;
			}
			return s_Cache.TryGet(key, out cache);
		}

		public static void SetCache(string key, T cache)
		{
			if (!string.IsNullOrEmpty(key))
			{
				s_Cache.Set(key, cache);
			}
		}
	}
}