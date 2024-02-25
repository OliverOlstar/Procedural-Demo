
using UnityEngine;
using System.Collections.Generic;
using DictKey = System.Tuple<string, System.Type>;

namespace Core
{
	public static class ABMs
	{
		static Dictionary<DictKey, ABMBase> s_Managers = new Dictionary<DictKey, ABMBase>();
		static List<ABMBase> s_ManagerList = new List<ABMBase>();
		static int s_ManagerCount = 0;
		public static int GetManagerCount() { return s_ManagerCount; }
		public static ABMBase Get(int index) => s_ManagerList[Mathf.Clamp(index, 0, s_ManagerCount)];

		public static T Create<T>(string managerName) where T : ABMBase
		{
			DictKey key = new DictKey(managerName, typeof(T));
			if (s_Managers.ContainsKey(key))
			{
				Debug.LogError(Core.Str.Build("AMBs.Create() Manager ", managerName, " alread exists"));
				return s_Managers[key] as T;
			}
			GameObject obj = new GameObject(managerName);
			T manager = obj.AddComponent<T>();
			GameObject.DontDestroyOnLoad(obj);
			s_Managers.Add(key, manager);
			s_ManagerList.Add(manager);
			s_ManagerCount++;
			Debug.Log(Core.Str.Build("ABMs.Create() Manager ", managerName, ", IsSimMode(): ", manager.IsSimulationMode().ToString(), ", UseLocalBundles: ", AssetBundleUtil.UseLocalBundlePath.ToString()));
			return manager;
		}

		public static bool Exists<T>(string managerName) where T : ABMBase => s_Managers.ContainsKey(new DictKey(managerName, typeof(T)));

		public static T Get<T>(string managerName) where T : ABMBase
		{
			DictKey key = new DictKey(managerName, typeof(T));
			if (!s_Managers.ContainsKey(key))
			{
				Core.DebugUtil.DevException(Core.Str.Build("ABM.Get() Manager ", managerName, " of type ", typeof(T).Name, " doesn't exist"));
			}
			return s_Managers[key] as T;
		}

		public static bool TryGet<T>(string managerName, out T manager) where T : ABMBase
		{
			DictKey key = new DictKey(managerName, typeof(T));
			s_Managers.TryGetValue(key, out ABMBase baseManager);
			manager = baseManager as T;
			return manager != null;
		}

		public static bool Initialized()
		{
			for (int i = 0; i < s_ManagerCount; i++)
			{
				if (!s_ManagerList[i].IsInitialized())
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsLoading()
		{
			for (int i = 0; i < s_ManagerCount; i++)
			{
				if (s_ManagerList[i].IsLoading())
				{
					return true;
				}
			}
			return false;
		}

		public static void GetBytes(out ulong current, out ulong total)
		{
			current = 0;
			total = 0;
			for (int i = 0; i < s_ManagerCount; i++)
			{
				s_ManagerList[i].GetBytes(out ulong abmCurrent, out ulong abmTotal);
				current += abmCurrent;
				total += abmTotal;
			}
		}
		// Size of all bundles in all the manifests
		public static ulong GetTotalSize()
		{
			ulong total = 0;
			for (int i = 0; i < s_ManagerCount; i++)
			{
				total += s_ManagerList[i].GetTotalSize();
			}
			return total;
		}
		// Size of all bundles that aren't downloaded
		public static ulong GetTotalDownloadSize()
		{
			ulong total = 0;
			for (int i = 0; i < s_ManagerCount; i++)
			{
				total += s_ManagerList[i].GetTotalDownloadSize();
			}
			return total;
		}

		public static bool IsDownloading()
		{
			for (int i = 0; i < s_ManagerCount; i++)
			{
				if (s_ManagerList[i].GetDownloadCount() > 0)
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsDownloadingEssentialBundles()
		{
			for (int i = 0; i < s_ManagerCount; i++)
			{
				if (s_ManagerList[i].IsDownloadingEssentialBundles())
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsInternetRequired()
		{
			for (int i = 0; i < s_ManagerCount; i++)
			{
				if (s_ManagerList[i].IsInternetRequired())
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsBundleLoaded(string bundleName)
		{
			for (int i = 0; i < s_ManagerList.Count; ++i)
			{
				if (s_ManagerList[i] is ABM abm && abm.IsAssetBundleLoaded(bundleName))
				{
					return true;
				}
			}
			return false;
		}

		public static AssetBundle GetLoadedBundle(string bundleName)
		{
			for (int i = 0; i < s_ManagerList.Count; ++i)
			{
				ABM abm = s_ManagerList[i] as ABM;
				if (abm == null)
				{
					continue;
				}
				LoadedAssetBundle bundle = abm.GetLoadedAssetBundle(bundleName);
				if (bundle != null)
				{
					return bundle.GetAssetBundle();
				}
			}
			return null;
		}

		public static void SetDownloadingMode(ABM.DownloadMode mode)
		{
			for (int i = 0; i < s_ManagerCount; i++)
			{
				s_ManagerList[i].SetDownloadMode(mode);
			}
		}

		public static void SetBackgroundDownloadMode(ABM.BackgroundDownloadMode mode)
		{
			for (int i = 0; i < s_ManagerCount; i++)
			{
				s_ManagerList[i].SetBackgroundDownloadMode(mode);
			}
		}

		public static List<string> GetManagerNames()
		{
			List<string> names = new List<string>(s_ManagerCount);
			foreach (DictKey key in s_Managers.Keys)
			{
				names.Add(key.Item1 + "." + key.Item2);
			}
			return names;
		}

		public static void Clear()
		{
			for (int i = 0; i < s_ManagerCount; i++)
			{
				if (s_ManagerList[i] is ABM abm)
				{
					abm.Clear();
				}
			}
		}

		public static void Reset()
		{
			for (int i = 0; i < s_ManagerCount; i++)
			{
				ABMBase manager = s_ManagerList[i];
				manager.Reset();
			}
		}

		public static void OnDestroy()
		{
			// Need to clear references to managers when they're destroyed to prevent memory leaks
			s_Managers.Clear();
			s_ManagerList.Clear();
			s_ManagerCount = 0;
		}

		public static string BuildDebugString()
		{
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			foreach (var kvp in s_Managers)
			{
				builder.Append(kvp.Key);
				builder.Append("\n");
				builder.Append(kvp.Value.GetDebugString());
				builder.Append("\n\n\n\n");
			}
			return builder.ToString();
		}
	}
}
