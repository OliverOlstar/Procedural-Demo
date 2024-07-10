
namespace OCore.PlayerPrefs
{
	public static class PlayerPrefs
	{
		private static string m_KeyPrefix = string.Empty;

		public delegate void PrefixChangedEvent();
		public static event PrefixChangedEvent OnPrefixChanged;

		private const int DEFAULT_INT = 0;
		private const float DEFAULT_FLOAT = 0.0f;
		private const string DEFAULT_STRING = null;

		public static string GetKeyPrefix() => m_KeyPrefix;
		public static void SetKeyPrefix(string prefix)
		{
			if (m_KeyPrefix != prefix)
			{
				UnityEngine.Debug.Log($"[PlayerPrefs] Global key prefix changed from '{m_KeyPrefix}' to '{prefix}'");
				m_KeyPrefix = prefix;
				OnPrefixChanged.Invoke();
			}
		}
		public static void ClearKeyPrefix() => SetKeyPrefix(string.Empty);

		public static string GetRealKey(string key)
		{
			return m_KeyPrefix + key;
		}

		// Int
		public static void SetInt(string key, int value)
			=> UnityEngine.PlayerPrefs.SetInt(GetRealKey(key), value);
		public static int GetInt(string key, int defaultValue)
			=> UnityEngine.PlayerPrefs.GetInt(GetRealKey(key), defaultValue);
		public static int GetInt(string key)
			=> UnityEngine.PlayerPrefs.GetInt(GetRealKey(key), DEFAULT_INT);
		public static bool TryGetInt(string key, out int value)
			=> TryGetGlobalInt(GetRealKey(key), DEFAULT_INT, out value);
		public static bool TryGetInt(string key, int defaultValue, out int value)
			=> TryGetGlobalInt(GetRealKey(key), defaultValue, out value);

		public static void SetGlobalInt(string key, int value)
			=> UnityEngine.PlayerPrefs.SetInt(key, value);
		public static int GetGlobalInt(string key, int defaultValue)
			=> UnityEngine.PlayerPrefs.GetInt(key, defaultValue);
		public static int GetGlobalInt(string key)
			=> UnityEngine.PlayerPrefs.GetInt(key, DEFAULT_INT);
		public static bool TryGetGlobalInt(string key, out int value)
			=> TryGetGlobalInt(key, DEFAULT_INT, out value);
		public static bool TryGetGlobalInt(string key, int defaultValue, out int value)
		{
			if (UnityEngine.PlayerPrefs.HasKey(key))
			{
				value = UnityEngine.PlayerPrefs.GetInt(key, defaultValue);
				return true;
			}
			value = defaultValue;
			return false;
		}

		// Float
		public static void SetFloat(string key, float value)
			=> UnityEngine.PlayerPrefs.SetFloat(GetRealKey(key), value);
		public static float GetFloat(string key, float defaultValue)
			=> UnityEngine.PlayerPrefs.GetFloat(GetRealKey(key), defaultValue);
		public static float GetFloat(string key)
			=> UnityEngine.PlayerPrefs.GetFloat(GetRealKey(key), DEFAULT_FLOAT);
		public static bool TryGetFloat(string key, out float value)
			=> TryGetGlobalFloat(GetRealKey(key), DEFAULT_FLOAT, out value);
		public static bool TryGetFloat(string key, float defaultValue, out float value)
			=> TryGetGlobalFloat(GetRealKey(key), defaultValue, out value);

		public static void SetGlobalFloat(string key, float value)
			=> UnityEngine.PlayerPrefs.SetFloat(key, value);
		public static float GetGlobalFloat(string key, float defaultValue)
			=> UnityEngine.PlayerPrefs.GetFloat(key, defaultValue);
		public static float GetGlobalFloat(string key)
			=> UnityEngine.PlayerPrefs.GetFloat(key, DEFAULT_FLOAT);
		public static bool TryGetGlobalFloat(string key, out float value)
			=> TryGetGlobalFloat(key, DEFAULT_FLOAT, out value);
		public static bool TryGetGlobalFloat(string key, float defaultValue, out float value)
		{
			if (UnityEngine.PlayerPrefs.HasKey(key))
			{
				value = UnityEngine.PlayerPrefs.GetFloat(key, defaultValue);
				return true;
			}
			value = defaultValue;
			return false;
		}

		// String
		public static void SetString(string key, string value)
			=> UnityEngine.PlayerPrefs.SetString(GetRealKey(key), value);
		public static string GetString(string key, string defaultValue)
			=> UnityEngine.PlayerPrefs.GetString(GetRealKey(key), defaultValue);
		public static string GetString(string key)
			=> UnityEngine.PlayerPrefs.GetString(GetRealKey(key), string.Empty);
		public static bool TryGetString(string key, out string value)
			=> TryGetGlobalString(GetRealKey(key), DEFAULT_STRING, out value);
		public static bool TryGetString(string key, string defaultValue, out string value)
			=> TryGetGlobalString(GetRealKey(key), defaultValue, out value);

		public static void SetGlobalString(string key, string value)
			=> UnityEngine.PlayerPrefs.SetString(key, value);
		public static string GetGlobalString(string key, string defaultValue)
			=> UnityEngine.PlayerPrefs.GetString(key, defaultValue);
		public static string GetGlobalString(string key)
			=> UnityEngine.PlayerPrefs.GetString(key, string.Empty);
		public static bool TryGetGlobalString(string key, out string value)
			=> TryGetGlobalString(key, DEFAULT_STRING, out value);
		public static bool TryGetGlobalString(string key, string defaultValue, out string value)
		{
			if (UnityEngine.PlayerPrefs.HasKey(key))
			{
				value = UnityEngine.PlayerPrefs.GetString(key, defaultValue);
				return true;
			}
			value = defaultValue;
			return false;
		}

		// Key
		public static bool HasKey(string key)
			=> UnityEngine.PlayerPrefs.HasKey(GetRealKey(key));
		public static void DeleteKey(string key)
			=> UnityEngine.PlayerPrefs.DeleteKey(GetRealKey(key));

		public static bool HasGlobalKey(string key)
			=> UnityEngine.PlayerPrefs.HasKey(key);
		public static void DeleteGlobalKey(string key)
			=> UnityEngine.PlayerPrefs.DeleteKey(key);

		// Other
		public static void DeleteAll()
			=> UnityEngine.PlayerPrefs.DeleteAll();
		public static void Save()
			=> UnityEngine.PlayerPrefs.Save();
	}
}