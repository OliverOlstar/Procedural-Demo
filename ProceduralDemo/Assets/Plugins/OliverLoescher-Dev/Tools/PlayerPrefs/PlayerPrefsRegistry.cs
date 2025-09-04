
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ODev.Util;
using UnityEngine;
using UnityEngine.Pool;

namespace ODev.PlayerPrefs
{
	internal static class PlayerPrefsRegistry
	{
		private const string REGISTRY_KEY = nameof(PlayerPrefsRegistry);

		[Serializable]
		public struct Registry
		{
			public string[] Keys;
		}

		private static HashSet<string> s_RuntimeKeys = new();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void RunOnStart()
		{
			LoadRegistry();
		}

		internal static void LoadRegistry()
        {
            string json = UnityEngine.PlayerPrefs.GetString(REGISTRY_KEY);
            if (string.IsNullOrEmpty(json))
			{
				s_RuntimeKeys.Clear();
				return;
            }
            Registry registry = JsonUtility.FromJson<Registry>(json);
            if (registry.Keys != null && registry.Keys.Length > 0)
			{
				s_RuntimeKeys = new(registry.Keys);
				LogRegistry();
            }
        }

        internal static void SaveRegistry()
		{
			LogRegistry();
			Registry registry = new() { Keys = s_RuntimeKeys.ToArray() };
			string json = JsonUtility.ToJson(registry);
			UnityEngine.PlayerPrefs.SetString(REGISTRY_KEY, json);
		}

		internal static void AddKey(string key)
		{
			s_RuntimeKeys.Add(key);
		}

		internal static void DeleteKey(string key)
		{
			s_RuntimeKeys.Remove(key);
		}

		internal static void DeleteAll()
		{
			s_RuntimeKeys.Clear();
		}

        internal static IEnumerable<string> DeleteWhere(Predicate<string> pPredicate)
        {
			List<string> toRemove = ListPool<string>.Get();
			foreach (string key in s_RuntimeKeys)
			{
				if (pPredicate(key))
				{
					toRemove.Add(key);
				}
			}
			foreach (string key in toRemove)
			{
				s_RuntimeKeys.Remove(key);
				yield return key;
			}
			ListPool<string>.Release(toRemove);
		}

		internal static IEnumerable<string> GetAllKeys()
		{
			foreach (string key in s_RuntimeKeys)
			{
				yield return key;
			}
		}

		internal static IEnumerable<string> GetAllKeys(Predicate<string> pPredicate)
		{
			foreach (string key in s_RuntimeKeys)
			{
				if (pPredicate(key))
				{
					yield return key;
				}
			}
		}

		[Conditional("ENABLE_DEBUG_LOGS")]
		private static void LogRegistry([CallerMemberName] string pMethodName = "")
		{
			StringBuilder stringBuilder = new();
			stringBuilder.AppendLine("Registry: [");
			foreach (string key in s_RuntimeKeys)
			{
				stringBuilder.AppendLine($"\t{key}, ");
			}
			stringBuilder.Append("]");
			typeof(PlayerPrefsRegistry).Log(stringBuilder.ToString(), pMethodName);

		}
	}
}