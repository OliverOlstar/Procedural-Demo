
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public partial class ABM
	{
		public void SetBundlePersistent(string assetBundleName, ABM.PersistentType persistentType)
		{
			UnsetBundlePersistent(assetBundleName);
			if (!m_Persistent.ContainsKey(persistentType))
			{
				m_Persistent.Add(persistentType, new HashSet<string>());
			}
			m_Persistent[persistentType].Add(assetBundleName);
		}
		
		public void UnsetBundlePersistent(string assetBundleName)
		{
			foreach (HashSet<string> bundles in m_Persistent.Values)
			{
				bundles.Remove(assetBundleName);
			}
		}

		public bool IsAssetBundleLoaded(string assetBundleName)
		{
			return IsSimulationMode() ? true : GetLoadedAssetBundle(assetBundleName) != null; // All bundles are loaded all the time in sim mode
		}

		public override bool IsLoading()
		{
			// Need to check all the asset and bundle sources that are currently required
			if (m_LoadOperations.Count > 0 || m_LoadFromCacheOperations.Count > 0 || base.IsLoading())
			{
				return true;
			}
			return false;
		}

		public int LoadBundle(string assetBundleName, bool includeDependencies = true)
		{
			if (!IsInitialized())
			{
				LogError(Core.Str.Build("LoadBundle() Loading \"", assetBundleName, "\" but ABM is not initialized"));
			}
			if (!BundleExists(assetBundleName))
			{
				LogError(Core.Str.Build("LoadBundle() bundle \"", assetBundleName, "\" doesn't exist"));
			}
			return DownloadBundleAndDependencies(assetBundleName, DownloadPriority.Essential, includeDependencies);
		}

		public BundleListHandle LoadBundleList(List<string> bundleNames)
		{
			if (!IsInitialized())
			{
				LogError(Core.Str.Build("LoadBundleList() ABM is not initialized"));
			}
			for (int i = 0; i < bundleNames.Count; i++)
			{
				string assetBundleName = bundleNames[i];
				if (!BundleExists(assetBundleName))
				{
					LogError(Core.Str.Build("LoadBundleList() bundle \"", assetBundleName, "\" doesn't exist"));
				}
				DownloadBundleAndDependencies(assetBundleName);
			}
			BundleListHandle handle = new BundleListHandle(this, bundleNames);
			m_LoadOperations.Add(handle);
			return handle;
		}

		public T[] LoadAllAssets<T>(string assetBundleName) where T : UnityEngine.Object
		{
#if UNITY_EDITOR
			if (IsSimulationMode())
			{
				return AssetBundleUtil.LoadAllAssetsEditor<T>(assetBundleName);
			}
#endif
			if (!IsInitialized())
			{
				LogError(Core.Str.Build("LoadAllAssets() Loading \"", assetBundleName, "\" but ABM is not initialized"));
			}
			if (!BundleExists(assetBundleName))
			{
				LogError(Core.Str.Build("LoadAllAssets() bundle \"", assetBundleName, "\" doesn't exist"));
			}
			LoadedAssetBundle loadedBundle = GetLoadedAssetBundle(assetBundleName);
			if (loadedBundle == null)
			{
				LogError(Core.Str.Build("LoadAllAssets() Failed to load assets bundle \"", assetBundleName, "\" was not loaded"));
				return null;
			}

			return loadedBundle.LoadAll<T>();
		}

		public IEnumerator HackyLoadAllAssetsAsysc<T>(string assetBundleName, List<T> assets)
			where T : UnityEngine.Object
		{
#if UNITY_EDITOR
			if (IsSimulationMode())
			{
				assets.AddRange(AssetBundleUtil.LoadAllAssetsEditor<T>(assetBundleName));
				yield break;
			}
#endif
			if (!IsInitialized())
			{
				LogError(Core.Str.Build("LoadAllAssetsAsync() Loading \"", assetBundleName, "\" but ABM is not initialized"));
			}
			if (!BundleExists(assetBundleName))
			{
				Log(Core.Str.Build("LoadAllAssetsAsync() bundle \"", assetBundleName, "\" doesn't exist"));
			}
			LoadedAssetBundle loadedBundle = GetLoadedAssetBundle(assetBundleName);
			if (loadedBundle == null)
			{
				Log(Core.Str.Build("LoadAllAssetsAsync() Failed to load asset bundle \"", assetBundleName, "\" was not loaded"));
				yield break;
			}
			AssetBundleRequest req = loadedBundle.GetAssetBundle().LoadAllAssetsAsync<T>();
			while (!req.isDone)
			{
				yield return null;
			}
			foreach (Object obj in req.allAssets)
			{
				if (obj is T asset)
				{
					assets.Add(asset);
				}
			}
		}

		public T LoadAsset<T>(string assetBundleName, string assetName) where T : UnityEngine.Object
		{
#if UNITY_EDITOR
			if (IsSimulationMode())
			{
				return AssetBundleUtil.LoadAssetEditor<T>(assetBundleName, assetName);
			}
#endif
			if (!IsInitialized())
			{
				LogError(Core.Str.Build("LoadAsset() Loading \"", assetBundleName, "\" but ABM is not initialized"));
			}
			if (!BundleExists(assetBundleName))
			{
				LogError(Core.Str.Build("LoadAsset() \"", assetName, "\" bundle \"", assetBundleName, "\" doesn't exist"));
			}
			LoadedAssetBundle loadedBundle = GetLoadedAssetBundle(assetBundleName);
			if (loadedBundle == null)
			{
				LogError(Core.Str.Build("LoadAsset() Failed to load \"", assetName, "\" bundle \"", assetBundleName, "\" was not loaded"));
				return null;
			}

			T asset = loadedBundle.Load<T>(assetName);
			if (asset == null)
			{
				LogError(Core.Str.Build("LoadAsset() Failed to load \"", assetName, "\" from \"", assetBundleName));
			}
			return asset;
		}

		public T LoadAsset<T>(string assetName) where T : UnityEngine.Object
		{
			if (!GetBundleNameForAsset(assetName, out string bundleName))
			{
				LogError(Core.Str.Build("LoadAsset() Could not find bundle name for asset ", assetName));
				return null;
			}
			return LoadAsset<T>(bundleName, assetName);
		}

		public T[] LoadAssetWithSubAssets<T>(string assetName) where T : UnityEngine.Object
		{
			string assetBundleName;
			if (!GetBundleNameForAsset(assetName, out assetBundleName))
			{
				LogError(Core.Str.Build("LoadAsset() Could not find bundle name for asset ", assetName));
				return null;
			}
#if UNITY_EDITOR
			if (IsSimulationMode())
			{
				return AssetBundleUtil.LoadAssetWithSubAssets<T>(assetBundleName, assetName);
			}
#endif
			if (!IsInitialized())
			{
				LogError(Core.Str.Build("LoadAssetWithSubAssets() Loading \"", assetBundleName, "\" but ABM is not initialized"));
			}
			if (!BundleExists(assetBundleName))
			{
				LogError(Core.Str.Build("LoadAssetWithSubAssets() \"", assetName, "\" bundle \"", assetBundleName, "\" doesn't exist"));
			}
			LoadedAssetBundle loadedBundle = GetLoadedAssetBundle(assetBundleName);
			if (loadedBundle == null)
			{
				LogError(Core.Str.Build("LoadAsset() Failed to load \"", assetName, "\" bundle \"", assetBundleName, "\" was not loaded"));
				return null;
			}
			T[] asset = loadedBundle.LoadAssetWithSubAssets<T>(assetName);
			return asset;
		}

		public BundleBinaryHandle<T> LoadBinaryAssetAsync<T>(
			string assetBundleName,
			string assetName)
		{
			if (!IsInitialized())
			{
				LogError(Core.Str.Build("LoadBinaryAssetAsync() Loading \"", assetBundleName, "\" but ABM is not initialized"));
			}
			if (!BundleExists(assetBundleName))
			{
				LogError(Core.Str.Build("LoadBinaryAssetAsync() \"", assetName, "\" bundle \"", assetBundleName, "\" doesn't exist"));
			}
			int reference = DownloadBundleAndDependencies(assetBundleName);
			BundleBinaryHandle<T> handle = new BundleBinaryHandle<T>(this, reference, assetBundleName, assetName);
			m_LoadOperations.Add(handle);
			return handle;
		}

		public BundleBinaryHandle<T> LoadBinaryAssetAsyncFromCatalog<T>(string assetName)
		{
			if (!GetBundleNameForAsset(assetName, out string bundleName))
			{
				LogError(Core.Str.Build("LoadBinaryAssetAsyncFromCatalog() Could not find bundle name for asset ", assetName));
				return null;
			}
			return LoadBinaryAssetAsync<T>(bundleName, assetName);
		}

		public BundleAssetHandleAsync<T> LoadAssetAsyncSlow<T>(
			string assetBundleName,
			string assetName,
			DownloadPriority downloadPriority = DownloadPriority.Essential) where T : UnityEngine.Object
		{
			if (!IsInitialized())
			{
				LogError(Core.Str.Build("LoadAssetAsyncSlow() Loading \"", assetBundleName, "\" but ABM is not initialized"));
			}
			if (!BundleExists(assetBundleName))
			{
				LogError(Core.Str.Build("LoadAssetAsyncSlow() \"", assetName, "\" bundle \"", assetBundleName, "\" doesn't exist"));
			}
			int reference = DownloadBundleAndDependencies(assetBundleName, downloadPriority);
			BundleAssetHandleAsync<T> handle = new BundleAssetHandleAsync<T>(this, reference, assetBundleName, assetName);
			m_LoadOperations.Add(handle);
			return handle;
		}

		public BundleAssetHandleAsync<T> LoadAssetAsyncSlowFromCatalog<T>(
			string assetName,
			DownloadPriority downloadPriority = DownloadPriority.Essential) where T : UnityEngine.Object
		{
			if (!GetBundleNameForAsset(assetName, out string bundleName))
			{
				LogError(Core.Str.Build("LoadAssetAsyncSlowFromCatalog() Could not find bundle name for asset ", assetName));
				return null;
			}
			return LoadAssetAsyncSlow<T>(bundleName, assetName, downloadPriority);
		}

		public BundleAssetHandle<T> LoadAssetAsync<T>(
			string assetBundleName,
			string assetName,
			string fallbackAssetName = "",
			DownloadPriority downloadPriority = DownloadPriority.Essential) where T : UnityEngine.Object
		{
			if (!IsInitialized())
			{
				LogError(Core.Str.Build("LoadAssetAsync() Loading \"", assetBundleName, "\" but ABM is not initialized"));
			}
			if (!BundleExists(assetBundleName))
			{
				LogError(Core.Str.Build("LoadAssetAsync() \"", assetName, "\" bundle \"", assetBundleName, "\" doesn't exist"));
			}
			int reference = DownloadBundleAndDependencies(assetBundleName, downloadPriority);
			BundleAssetHandle<T> handle = new BundleAssetHandle<T>(this, reference, assetBundleName, assetName, fallbackAssetName);
			m_LoadOperations.Add(handle);
			return handle;
		}

		public BundleAssetHandle<T> LoadAssetAsyncFromCatalog<T>(
			string assetName, 
			string fallbackAssetName = "",
			DownloadPriority downloadPriority = DownloadPriority.Essential) where T : UnityEngine.Object
		{
			if (!GetBundleNameForAsset(assetName, out string bundleName))
			{
				if (string.IsNullOrEmpty(fallbackAssetName) || !GetBundleNameForAsset(fallbackAssetName, out bundleName))
				{
					LogError(Core.Str.Build("LoadAssetAsyncFromCatalog() Could not find bundle name for asset ", assetName, " or fallback ", fallbackAssetName));
					return null;
				}
			}
			return LoadAssetAsync<T>(bundleName, assetName, fallbackAssetName, downloadPriority);
		}

		public BundleLevelHandle LoadSceneAsync(string assetBundleName, string sceneName, bool additive = true, bool setActive = false)
		{
			if (!IsInitialized())
			{
				LogError(Core.Str.Build("LoadSceneAsync() Loading \"", assetBundleName, "\" but ABM is not initialized"));
			}
			if (!BundleExists(assetBundleName))
			{
				LogError(Core.Str.Build("LoadSceneAsync() \"", sceneName, "\" bundle \"", assetBundleName, "\" doesn't exist"));
			}
			DownloadBundleAndDependencies(assetBundleName);
			BundleLevelHandle handle = new BundleLevelHandle(this, assetBundleName, sceneName, additive, setActive);
			m_LoadOperations.Add(handle);
			return handle;
		}

		public BundleLevelHandle LoadSceneAsyncFromCatalog(string sceneName, bool additive = true, bool setActive = false)
		{
			if (!GetBundleNameForAsset(sceneName, out string bundleName))
			{
				LogError(Core.Str.Build("LoadSceneAsyncFromCatalog() Could not find bundle name for scene ", sceneName));
				return null;
			}
			return LoadSceneAsync(bundleName, sceneName, additive, setActive);
		}
	}
}
