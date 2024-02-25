
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core
{
	public partial class ABMBase : MonoBehaviour
	{
		public enum DownloadPriority
		{
			Essential = 0,
			NonEssential,
		}
		public enum DownloadMode
		{
			Allowed = 0,
			Restricted
		}
		public enum BackgroundDownloadMode
		{
			Paused = 0,
			OnlyQueuedBundles,
			AllBundles,
		}

		protected Dictionary<string, string[]> m_Dependencies = null;
		public int GetBundleCount() { return m_Dependencies == null ? 0 : m_Dependencies.Count; }
		public bool TryGetDependencies(string bundleName, out string[] dependencies) => m_Dependencies.TryGetValue(bundleName, out dependencies);
		protected AssetBundleDownloader m_Downloader = new AssetBundleDownloader();
		protected AssetBundleManifest m_Manifest = null;
		public AssetBundleManifest GetManifest() { return m_Manifest; }

		private StreamingBundlesCatalog m_StreamingCatalog = new StreamingBundlesCatalog();
		public StreamingBundlesCatalog StreamingCatalog => m_StreamingCatalog;
		private AssetBundleDownload m_SizesDownload = null;
		private Dictionary<string, ulong> m_Sizes = null;
		private AssetBundleDownload m_CatalogueDownload = null;
		private AssetBundleCatalogue m_Catalogue = null;

		private Coroutine m_InitializeRoutine = null;
		private AssetBundleDownload m_ManifestDownload = null;

		private string m_BuildName = Core.Str.EMPTY;
		public string BuildName => m_BuildName;
		private string m_DebugName = Core.Str.EMPTY;
		public string DebugName => m_DebugName;
		private string m_BaseDownloadingURL = Core.Str.EMPTY;
		public string GetAssetBundleURL(string bundleName) => m_StreamingCatalog.TryGetStreamingPath(bundleName, out string streamingPath) ?
			streamingPath : Core.Str.Build(m_BaseDownloadingURL, bundleName);

		private ulong m_TotalSize = 0;
		public ulong GetTotalSize() { return m_TotalSize; } // Size of all bundles in the manifest
		private ulong m_TotalDownloadSize = 0;
		public ulong GetTotalDownloadSize() { return m_TotalDownloadSize; } // Size of all bundles that aren't downloaded

		private int m_Version = 0;
		public int Version => m_Version;

		private bool m_Initialized = false;
		public bool IsInitialized() { return m_Initialized; }
		private bool m_AllowSimulation = true;
		public bool IsSimulationMode() => m_AllowSimulation ? AssetBundleUtil.IsSimMode() : false;
		private bool m_UsingStreamingAssets = false;

		public ulong GetBundleSize(string assetBundleName)
		{
			ulong size = 0;
			if (m_Sizes != null && !m_Sizes.TryGetValue(assetBundleName, out size))
			{
				LogWarning(Core.Str.Build("GetBundleSize() Couldn't find size for bundle ", assetBundleName));
			}
			return size;
		}

		public int GetDownloadCount() => m_Downloader.GetDownloadCount();

		public void SetDownloadSlotCount(int count) => m_Downloader.SetDownloadSlotCount(count);

		public void SetDownloadMode(DownloadMode mode) => m_Downloader.SetDownloadMode(mode);

		public void SetBackgroundDownloadMode(BackgroundDownloadMode mode) => m_Downloader.SetBackgroundDownloadMode(mode);

		public void GetBytes(out ulong current, out ulong total) => m_Downloader.GetBytes(out current, out total);

		public void AddBundlesToDownloadQueue(IEnumerable<string> assetBundleNames) => m_Downloader.AddBundlesToDownloadQueue(assetBundleNames);

		public bool IsDownloadingEssentialBundles() => m_Downloader.IsDownloadingEssentialBundles();

		public void AddBundleToDownloadQueue(string assetBundleName)
		{
			m_Downloader.AddBundleToDownloadQueue(assetBundleName);
			// Queue dependencies
			string[] dependencies = m_Dependencies[assetBundleName];
			int dependencyCount = dependencies.Length;
			for (int i = 0; i < dependencyCount; i++)
			{
				string dependency = dependencies[i];
				m_Downloader.AddBundleToDownloadQueue(dependency);
			}
		}

		public void SetBundleRequired(string assetBundleName)
		{
			if (!IsInitialized())
			{
				LogError(Core.Str.Build("SetBundleRequired() Loading \"", assetBundleName, "\" but ABM is not initialized"));
			}
			if (!BundleExists(assetBundleName))
			{
				LogError(Core.Str.Build("SetBundleRequired() bundle \"", assetBundleName, "\" doesn't exist"));
			}
			m_Downloader.RequestDownload(assetBundleName, DownloadPriority.Essential);
			if (!m_Dependencies.TryGetValue(assetBundleName, out string[] dependencies))
			{
				LogError("Could not find dependencies for asset bundle " + assetBundleName);
				return;
			}
			int dependencyCount = dependencies.Length;
			for (int i = 0; i < dependencyCount; i++)
			{
				m_Downloader.RequestDownload(dependencies[i], DownloadPriority.Essential, assetBundleName);
			}
		}

		public void SetBundlesRequired(List<string> assetBundleNames)
		{
			foreach (string assetBundleName in assetBundleNames)
			{
				SetBundleRequired(assetBundleName);
			}
		}

		public bool BundleExists(string bundleName)
		{
			if (m_Dependencies == null)
			{
				LogError(Core.Str.Build("BundleExists() ABM is not initialized"));
				return false;
			}
			return m_Dependencies.ContainsKey(bundleName);
		}

		public bool GetBundleNameForAsset(string assetName, out string bundleName)
		{
			if (!IsInitialized())
			{
				LogError(Core.Str.Build("GetBundleNameForAsset() Not initialized"));
				bundleName = Core.Str.EMPTY;
				return false;
			}
			return m_Catalogue.TryGetBundleName(assetName, out bundleName);
		}
		public bool CatalogHasAsset(string assetName) => GetBundleNameForAsset(assetName, out _);

		public bool GetAssetNamesInBundle(string bundleName, out IReadOnlyList<string> names)
		{
			if (!IsInitialized())
			{
				LogError(Core.Str.Build("GetAssetNamesInBundle() Not initialized"));
				names = default;
				return false;
			}
			return m_Catalogue.TryGetAssetNames(bundleName, out names);
		}

		public void Initialize(string buildName, string assetBundleURL, bool allowSimulation)
		{
			if (m_InitializeRoutine != null)
			{
				Core.DebugUtil.DevException("ABM.Intialize() Initialization already in progress");
				return;
			}
			if (m_Manifest != null || m_ManifestDownload != null)
			{
				LogError(Core.Str.Build("Initialize() Is already initialized"));
			}
#if UNITY_EDITOR
			if (AssetBundleUtil.UseLocalBundlePath)
			{
				assetBundleURL = "file://" + AssetBundleUtil.LocalBundlePath;
				if (Core.DebugOptions.ABMLogs.IsSet()) Log(Str.Build("Using local bundles:  ", assetBundleURL));
			}
#endif
			string manifestAssetBundleName = AssetBundleUtil.GetManifestBundleName(buildName);

			m_BuildName = buildName;
			m_DebugName = Core.Str.Build(buildName, ".", GetType().Name);
			m_Version = AssetBundleUtil.GetBundleVersionFromURL(assetBundleURL);
			m_BaseDownloadingURL = AssetBundleUtil.BuildBundleBaseURL(assetBundleURL, manifestAssetBundleName);
			// Note: Seems like the prefix could be http:// or https://
			m_UsingStreamingAssets = !m_BaseDownloadingURL.StartsWith("http");
			m_AllowSimulation = allowSimulation;
			m_Downloader.Initialize(this, ProcessFinishedDownload);

			string manifestUrl = GetAssetBundleURL(manifestAssetBundleName);
			manifestAssetBundleName = manifestAssetBundleName.ToLower();
			if (Core.DebugOptions.ABMLogs.IsSet()) Log(Core.Str.Build("Initialize ", manifestUrl));
#if UNITY_EDITOR
			if (IsSimulationMode())
			{
				m_Initialized = true;
				m_Catalogue = new AssetBundleCatalogue();
				m_Catalogue.EditorInitialize(); // If we're running in simulation mode we want to build a new up to date asset catalogue
				string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
				m_Dependencies = new Dictionary<string, string[]>(bundleNames.Length);
				for (int i = 0; i < bundleNames.Length; i++)
				{
					m_Dependencies.Add(bundleNames[i], new string[] { });
				}
				return;
			}
#endif
			if (InternetReachabilityDirector.InternetReachability == NetworkReachability.NotReachable)
			{
				List<Hash128> hashes = ListPool<Hash128>.Request();
				Caching.GetCachedVersions(manifestAssetBundleName.ToLower(), hashes);
				if (hashes.Count > 0)
				{
					m_ManifestDownload = new AssetBundleDownload(
						manifestAssetBundleName,
						manifestUrl,
						hashes[0],
						0,
						false);
				}
				ListPool<Hash128>.Return(hashes);
			}
			if (m_ManifestDownload == null)
			{
				Hash128 manifestHash = Hash128.Compute(manifestAssetBundleName + Chrono.UtcNowMilliseconds); // Generate some unique hash
				m_ManifestDownload = new AssetBundleDownload(
					manifestAssetBundleName,
					manifestUrl,
					manifestHash,
					0,
					!m_UsingStreamingAssets);
			}
			m_InitializeRoutine = StartCoroutine(InitializeRoutine());
		}
		private IEnumerator InitializeRoutine()
		{
			while (!Caching.currentCacheForWriting.ready)
			{
				yield return null;
			}
			while (!m_ManifestDownload.IsDone())
			{
				m_ManifestDownload.Update();
				yield return null;
			}
			OnManifestDownloadComplete();
			yield return m_StreamingCatalog.Initialize(this);
			InitializeSizesAndCatalog();
			while (!SizesAndCatalogComplete())
			{
				yield return null;
			}
			OnInitializationComplete();
			m_Initialized = true;
			m_InitializeRoutine = null;
		}

		private void OnManifestDownloadComplete()
		{
			Caching.ClearOtherCachedVersions(m_ManifestDownload.GetAssetBundleName(), m_ManifestDownload.GetHash());

			AssetBundle manifestBundle = m_ManifestDownload.GetAssetBundle();
			m_Manifest = manifestBundle.LoadAsset<AssetBundleManifest>(AssetBundleUtil.MANIFEST_ASSET_NAME);
			// Note: We have to unload the manifest because if the project has multiple ABMs the manifest bundles will
			// have the same names which causes duplicate asset bundle loading issues
			// We might leak manifest asset when Reset() is called?
			manifestBundle.Unload(false);
			m_ManifestDownload = null;

			string[] bundles = m_Manifest.GetAllAssetBundles();
			m_Dependencies = new Dictionary<string, string[]>(bundles.Length);
			//Core.Str.Add("MANIFEST ", bundles.Length.ToString(), "\n");
			for (int i = 0; i < bundles.Length; i++)
			{
				string bundleName = bundles[i];
				//try to remove any outdated versions of the bundles on device
				Caching.ClearOtherCachedVersions(bundleName, m_Manifest.GetAssetBundleHash(bundleName));
				string[] dependencies = m_Manifest.GetAllDependencies(bundleName);
				m_Dependencies.Add(bundleName, dependencies);
			}
			if (DebugOptions.ABMLogs.IsSet()) LogCacheInfo();
		}

		private void InitializeSizesAndCatalog()
		{
			string sizesName = AssetBundleUtil.GetSizesBundleName(m_BuildName);
			if (m_Dependencies.ContainsKey(sizesName))
			{
				Hash128 sizesHash = m_Manifest.GetAssetBundleHash(sizesName);
				bool requiresDownload = !m_UsingStreamingAssets && !m_StreamingCatalog.Contains(sizesName) && !Caching.IsVersionCached(new CachedAssetBundle(sizesName, sizesHash));
				m_SizesDownload = new AssetBundleDownload(sizesName, GetAssetBundleURL(sizesName), sizesHash, 0, requiresDownload);
			}
			string catalogName = AssetBundleUtil.GetCatalogueBundleName(m_BuildName);
			if (m_Dependencies.ContainsKey(catalogName))
			{
				Hash128 catalogHash = m_Manifest.GetAssetBundleHash(catalogName);
				bool requiresDownload = !m_UsingStreamingAssets && !m_StreamingCatalog.Contains(catalogName) && !Caching.IsVersionCached(new CachedAssetBundle(catalogName, catalogHash));
				m_CatalogueDownload = new AssetBundleDownload(catalogName, GetAssetBundleURL(catalogName), catalogHash, 0, requiresDownload);
			}
		}

		private bool SizesAndCatalogComplete()
		{
			bool sizesDone = true;
			if (m_SizesDownload != null && !m_SizesDownload.IsDone())
			{
				m_SizesDownload.Update();
				sizesDone = m_SizesDownload.IsDone();
			}
			bool catalogDone = true;
			if (m_CatalogueDownload != null && !m_CatalogueDownload.IsDone())
			{
				m_CatalogueDownload.Update();
				catalogDone = m_CatalogueDownload.IsDone();
			}
			return sizesDone && catalogDone;
		}

		private T LoadBinAssetFromBundle<T>(AssetBundle bundle, string assetName) where T : class
		{
			if (bundle == null)
			{
				LogWarning("LoadBinAssetFromBundle() Can't load asset " + assetName + " bundle is null");
				return null;
			}
			TextAsset text = bundle.LoadAsset<TextAsset>(assetName);
			if (text == null)
			{
				LogWarning("LoadBinAssetFromBundle() " + bundle.name + " doesn't contain TextAsset " + assetName);
				return null;
			}
			T asset = AssetBundleUtil.Deserialize<T>(text);
			if (asset == null)
			{
				LogWarning("LoadBinAssetFromBundle() Couldn't deserialize TextAsset " + assetName);
				return null;
			}
			return asset;
		}

		private void OnInitializationComplete()
		{
			if (m_CatalogueDownload != null && m_CatalogueDownload.IsDone())
			{
				string catalogName = AssetBundleUtil.GetCatalogueAssetName(m_BuildName);
				AssetBundle catalogBundle = m_CatalogueDownload.GetAssetBundle();
				m_Catalogue = LoadBinAssetFromBundle<AssetBundleCatalogue>(catalogBundle, catalogName);
				catalogBundle.Unload(true);
			}
			if (m_SizesDownload != null && m_SizesDownload.IsDone())
			{
				string sizesName = AssetBundleUtil.GetSizesAssetName(m_BuildName);
				AssetBundle sizesBundle = m_SizesDownload.GetAssetBundle();
				m_Sizes = LoadBinAssetFromBundle<Dictionary<string, ulong>>(sizesBundle, sizesName);
				sizesBundle.Unload(true);
			}
			// Queue up all the bundles we need to download once bundle sizes is finished
			m_TotalSize = 0;
			m_TotalDownloadSize = 0;
			foreach (string bundleName in m_Dependencies.Keys)
			{
				ulong size = m_Sizes != null && m_Sizes.TryGetValue(bundleName, out size) ? size : 0;
				m_TotalSize += size;
				//Note: should avoid using "/" in an asset bundle name, as IsVersionedCached strips file paths.
				//Meaning that "sprites/bundle_name" and "bundle_name" will both be searched, but "sprites/bundle_name" 
				//will always return false, because they are both comparing their hash to the loaded "bundle_name" hash
				Hash128 hash = m_Manifest.GetAssetBundleHash(bundleName);
				if (Caching.IsVersionCached(new CachedAssetBundle(bundleName, hash)))
				{
					if (Core.DebugOptions.ABMLogs.IsSet()) Core.Str.AddLine(bundleName, " cached");
					continue;
				}
				if (m_StreamingCatalog.Contains(bundleName))
				{
					if (Core.DebugOptions.ABMLogs.IsSet()) Core.Str.AddLine(bundleName, " streaming");
					continue;
				}
				m_Downloader.DownloadAssetBundle(bundleName, GetAssetBundleURL(bundleName), hash, size, !m_UsingStreamingAssets);
				m_TotalDownloadSize += size;
				if (Core.DebugOptions.ABMLogs.IsSet()) Core.Str.AddLine(bundleName, " ", UIUtil.BytesToString(size));
			}
			if (Core.DebugOptions.ABMLogs.IsSet()) Log(Core.Str.Build("Initialized, downloading ",
				UIUtil.BytesToString(m_TotalDownloadSize), "/", UIUtil.BytesToString(m_TotalSize), "\n", Core.Str.Finish()));
		}

		protected virtual void OnUpdate()
		{
			m_Downloader.UpdateDownloads();
		}

		private void Update()
		{
			DebugUpdate();
			if (!IsInitialized())
			{
				return;
			}
			OnUpdate();
			DebugUpdate();
		}

		private void OnDestroy()
		{
			// Need to clear static references when we're destroyed to prevent memory leaks
			// For now when one ABM gets destroyed we're going to assume all of the others do too
			ABMs.OnDestroy();
		}

		public virtual void Reset()
		{
			m_Initialized = false;
			if (m_InitializeRoutine != null)
			{
				StopCoroutine(m_InitializeRoutine);
				m_InitializeRoutine = null;
			}

			if (m_ManifestDownload != null)
			{
				m_ManifestDownload.Abort();
				m_ManifestDownload = null;
			}
			m_Manifest = null;
			m_Dependencies = null;

			if (m_CatalogueDownload != null)
			{
				m_CatalogueDownload.Abort();
				m_CatalogueDownload = null;
			}
			m_Catalogue = null;

			if (m_SizesDownload != null)
			{
				m_SizesDownload.Abort();
				m_SizesDownload = null;
			}
			m_Sizes = null;

			m_Downloader.Reset();
		}

		public bool IsInternetRequired()
		{
			if (m_UsingStreamingAssets)
			{
				return false;
			}
			if (m_ManifestDownload != null &&
				!m_ManifestDownload.IsDone() &&
				m_ManifestDownload.RequiresDownload())
			{
				return true;
			}
			if (m_CatalogueDownload != null &&
				!m_CatalogueDownload.IsDone() &&
				m_CatalogueDownload.RequiresDownload())
			{
				return true;
			}
			if (m_SizesDownload != null &&
				!m_SizesDownload.IsDone() &&
				m_SizesDownload.RequiresDownload())
			{
				return true;
			}
			return m_Downloader.IsDownloadingEssentialBundles();
		}

		public virtual bool IsLoading() => IsDownloadingEssentialBundles();

		public virtual bool IsDownloadRequestStillValid(string assetBundleName) => true;

		protected virtual void ProcessFinishedDownload(AssetBundleDownload download) { }
	}
}
