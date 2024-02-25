using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core
{
	public partial class ABM : ABMBase
	{
		internal class References : HashSet<int> { }

		public enum PersistentType
		{
			NeverUnload = 0,
			OnlyManualUnload,
			NotPersistent
		}

		public static readonly int MAX_LOADS = 20;
		public static readonly int INVALID_REF = 0;
		private static int s_RefHandle = INVALID_REF;

		private Dictionary<string, LoadedAssetBundle> m_LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();
		private List<LoadedAssetBundle> m_UnloadingAssetBundles = new List<LoadedAssetBundle>();
		private bool IsUnloading() { return m_UnloadingAssetBundles.Count > 0; }
		private Dictionary<string, References> m_References = new Dictionary<string, References>();
		private Dictionary<PersistentType, HashSet<string>> m_Persistent = new Dictionary<PersistentType, HashSet<string>>();
		private Dictionary<string, AssetBundleDownload> m_LoadFromCacheOperations = new Dictionary<string, AssetBundleDownload>();
		private List<string> m_RemoveFromLoadFromCache = new List<string>();
		private List<BaseBundleHandle> m_LoadOperations = new List<BaseBundleHandle>();

		public override bool IsDownloadRequestStillValid(string assetBundleName) // Required by AssetBundleDownloader
		{
			return m_References.TryGetValue(assetBundleName, out References references) && references.Count > 0;
		}

		public bool IsPersistent(string bundleName, PersistentType type)
		{
			return m_Persistent.TryGetValue(type, out HashSet<string> bundles) && bundles.Contains(bundleName);
		}
		public bool IsPersistent(string bundleName, out PersistentType type)
		{
			foreach (KeyValuePair<PersistentType, HashSet<string>> bundles in m_Persistent)
			{
				if (bundles.Value.Contains(bundleName))
				{
					type = bundles.Key;
					return true;
				}
			}
			type = default;
			return false;
		}
		public bool IsPersistent(string bundleName) => IsPersistent(bundleName, out _);
		public static bool IsLowerPersistantLevel(ABM.PersistentType typeA, ABM.PersistentType typeB)
		{
			switch (typeB)
			{
				case ABM.PersistentType.NeverUnload:
					return typeA != ABM.PersistentType.NeverUnload;
				case ABM.PersistentType.OnlyManualUnload:
					return typeA == ABM.PersistentType.NotPersistent;
			}
			return false;
		}

		public void UnloadBundle(string assetBundleName, int referenceHandle)
		{
			References references = null;
			// Check that the bundle is ref counted and that our handle was one of the references
			if (!m_References.TryGetValue(assetBundleName, out references) || !references.Remove(referenceHandle))
			{
				return;
			}

			if (references.Count == 0 && !IsPersistent(assetBundleName, PersistentType.NeverUnload))
			{
				// Bundle might not have finished loading yet (or we're in simulation mode)
				if (m_LoadedAssetBundles.TryGetValue(assetBundleName, out LoadedAssetBundle loadedAssetBundle))
				{
					if (Core.DebugOptions.ABMLogs.IsSet()) Log(Core.Str.Build("UnloadBundle() Actually unload ", assetBundleName));
					m_LoadedAssetBundles.Remove(assetBundleName);
					m_UnloadingAssetBundles.Add(loadedAssetBundle);
				}
			}

			//Core.Str.Add(assetBundleName);

			// If we have dependencies we expect them to be ref counted properly
			string[] dependencies = m_Dependencies[assetBundleName];
			for (int i = 0; i < dependencies.Length; i++)
			{
				string dependency = dependencies[i];
				//Core.Str.Add(", ", dependency);
				if (!m_References.TryGetValue(dependency, out References dependencyReferences))
				{
					LogError(Core.Str.Build("UnloadBundle() Trying to unload bundle ", assetBundleName,
						" dependency ", dependency, " has no refcount"));
					continue;
				}
				if (!dependencyReferences.Contains(referenceHandle))
				{
					LogError(Core.Str.Build("UnloadBundle() Trying to unload bundle ", assetBundleName,
						" dependency ", dependency, " doesn't have handle ", referenceHandle.ToString()));
					continue;
				}
				dependencyReferences.Remove(referenceHandle);
				if (dependencyReferences.Count == 0 && !IsPersistent(dependency))
				{
					LoadedAssetBundle loadedDependency = null; // Dependency might not have finished loading yet (or we're in simulation mode)
					if (m_LoadedAssetBundles.TryGetValue(dependency, out loadedDependency))
					{
						//Log(Core.Str.Build("UnloadBundle() Actually unload dependency ", dependency));
						m_LoadedAssetBundles.Remove(dependency);
						m_UnloadingAssetBundles.Add(loadedDependency);
					}
				}
			}

			//Log(Core.Str.Build("UnloadBundle() Remove ref ", Core.Str.Finish(), " handle ", referenceHandle.ToString(), " ", Time.time.ToString(), "\n", ReferencesToString()));
		}

		private string ReferencesToString()
		{
			foreach (string key in m_References.Keys)
			{
				if (m_References[key].Count > 0)
				{
					Core.Str.Add(key, " - ");
					foreach (int handle in m_References[key])
					{
						Core.Str.Add(handle.ToString(), ", ");
					}
					Core.Str.Add("\n");
				}
			}
			return Core.Str.Finish();
		}

		public void Clear()
		{
			for (int i = 0; i < m_LoadOperations.Count; i++)
			{
				BaseBundleHandle handle = m_LoadOperations[i];
				if (!handle.IsPersistent())
				{
					m_LoadOperations.RemoveAt(i);
					i--;
				}
			}

			// Clear references
			Dictionary<string, References> persistentReferences = Core.DictionaryPool<string, References>.Request();
			foreach (KeyValuePair<string, References> references in m_References)
			{
				if (IsPersistent(references.Key))
				{
					persistentReferences.Add(references.Key, references.Value);
				}
			}
			m_References.Clear();
			m_References.AddRange(persistentReferences);
			Core.DictionaryPool<string, References>.Return(persistentReferences);

			// Get bundles to unload
			List<LoadedAssetBundle> persistentBundles = new List<LoadedAssetBundle>(m_Persistent.Count);
			foreach (LoadedAssetBundle bundle in m_LoadedAssetBundles.Values)
			{
				if (IsPersistent(bundle.GetBundleName()))
				{
					persistentBundles.Add(bundle);
				}
				else
				{
					m_UnloadingAssetBundles.Add(bundle);
				}
			}
			m_LoadedAssetBundles.Clear();
			// Preserve persistent bundles
			for (int i = 0; i < persistentBundles.Count; i++)
			{
				LoadedAssetBundle bundle = persistentBundles[i];
				m_LoadedAssetBundles.Add(bundle.GetBundleName(), bundle);
			}
		}

		public override void Reset()
		{
			base.Reset();

			m_References.Clear();
			m_Persistent.Clear();

			m_LoadOperations.Clear();
			foreach (LoadedAssetBundle bundle in m_LoadedAssetBundles.Values)
			{
				m_UnloadingAssetBundles.Add(bundle);
			}
			m_LoadedAssetBundles.Clear();

			foreach (AssetBundleDownload cacheload in m_LoadFromCacheOperations.Values)
			{
				cacheload.Abort();
			}
			m_LoadFromCacheOperations.Clear();
		}

		// This is exposed only for BundleHandles
		internal LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName)
		{
			m_LoadedAssetBundles.TryGetValue(assetBundleName, out LoadedAssetBundle bundle);
			if (bundle == null)
			{
				return null;
			}

			// Make sure all dependencies are loaded
			string[] dependencies = m_Dependencies[assetBundleName];
			for (int i = 0; i < dependencies.Length; i++)
			{
				string dependency = dependencies[i];
				m_LoadedAssetBundles.TryGetValue(dependency, out LoadedAssetBundle dependentBundle);
				if (dependentBundle == null)
				{
					if (!m_Downloader.ContainsDownloadOperation(dependency) && !m_LoadFromCacheOperations.ContainsKey(dependency))
					{
						LogError(Core.Str.Build("GetLoadedAssetBundle() No load request for ", assetBundleName, " dependency ", dependency));
					}
					return null;
				}
			}

			return bundle;
		}

		private int DownloadBundleAndDependencies(
			string assetBundleName,
			DownloadPriority downloadPriority = DownloadPriority.Essential,
			bool includeDependencies = true)
		{
			bool persistent = AssetBundlesConfig.TryGetIsPersistent(assetBundleName, out PersistentType persistentType);
			int referenceHandle = persistentType == PersistentType.NeverUnload ? INVALID_REF : ++s_RefHandle;
			if (IsSimulationMode())
			{
				return referenceHandle;
			}
			//Core.Str.Add(assetBundleName);
			EnsureBundleIsLoadedOrDownloading(assetBundleName, referenceHandle, downloadPriority);
			if (includeDependencies)
			{
				if (m_Dependencies.TryGetValue(assetBundleName, out string[] dependencies))
				{
					for (int i = 0; i < dependencies.Length; i++)
					{
						//Core.Str.Add(", ", dependencies[i]);
						EnsureBundleIsLoadedOrDownloading(dependencies[i], referenceHandle, downloadPriority, assetBundleName, persistentType);
					}
				}
				else
				{
					LogError($"Could not find dependencies for asset bundle {assetBundleName}");
				}
			}

			//Log(Core.Str.Build("DownloadBundleAndDependencies() Add ref ", Core.Str.Finish(), " handle ", referenceHandle.ToString(), "\n", ReferencesToString()));
			return referenceHandle;
		}

		private void EnsureBundleIsLoadedOrDownloading(string assetBundleName, int referenceHandle, DownloadPriority downloadPriority, string bundleParentName = null, PersistentType? bundleParentPersistentType = null)
		{
			TryAddToPersistentBundles(assetBundleName, bundleParentPersistentType);

			// Invalid handle means we're never unload persistent
			if (referenceHandle != INVALID_REF)
			{
				if (!m_References.TryGetValue(assetBundleName, out References references))
				{
					references = new References();
					m_References.Add(assetBundleName, references);
				}
				references.Add(s_RefHandle);
			}

			if (m_LoadedAssetBundles.ContainsKey(assetBundleName))
			{
				return;
			}

			// Check if the bundle is scheduled to be downloaded
			if (m_Downloader.ContainsDownloadOperation(assetBundleName))
			{
				m_Downloader.RequestDownload(assetBundleName, downloadPriority, bundleParentName);
			}
			else
			{
				if (!m_LoadFromCacheOperations.ContainsKey(assetBundleName))
				{
					// If the bundle is not loaded, not being downloaded and not being loaded from cache, then we need to create a load from cache operation
					AssetBundleDownload webOp = new AssetBundleDownload(
						assetBundleName,
						GetAssetBundleURL(assetBundleName),
						m_Manifest.GetAssetBundleHash(assetBundleName),
						0,
						false);
					m_LoadFromCacheOperations.Add(assetBundleName, webOp);
				}
			}
		}

		private bool TryAddToPersistentBundles(string assetBundleName, PersistentType? parentPersistentType = null)
		{
			AssetBundlesConfig.TryGetIsPersistent(assetBundleName, out PersistentType type);
			if (parentPersistentType.HasValue && IsLowerPersistantLevel(type, parentPersistentType.Value))
			{
				DevException($"Asset bundle is loading it's dependancy '{assetBundleName}' which has a lower level of persistence. This should never happen." + 
					$" To fix this remove this asset bundle's references to '{assetBundleName}' thus removing the depenancy or adjust the persistent configuration in the {nameof(AssetBundlesConfig)}.asset");
				m_Persistent[type].Remove(assetBundleName);
				type = parentPersistentType.Value; // Move up to parent's persistence to recover
			}
			if (type == PersistentType.NotPersistent)
			{
				return false;
			}
			if (!m_Persistent.ContainsKey(type))
			{
				m_Persistent.Add(type, new HashSet<string>());
			}
			return m_Persistent[type].Add(assetBundleName);
		}

		protected override void ProcessFinishedDownload(AssetBundleDownload download)
		{
			if (!download.IsDone())
			{
				LogError(Core.Str.Build("ProcessFinishedDownload() ", download.GetAssetBundleName(), " is not finished downloading"));
				return;
			}
			if (download.GetAssetBundle() == null)
			{
				LogError(Core.Str.Build("ProcessFinishedDownload() ", download.GetAssetBundleName(), " asset bundle is null"));
				return;
			}
			if (m_LoadedAssetBundles.ContainsKey(download.GetAssetBundleName()))
			{
				LogError(Core.Str.Build("ProcessFinishedDownload() ", download.GetAssetBundleName(), " is already loaded"));
				return;
			}
			// This would be the spot to unload the bundle if it's no longer ref counted
			// Even if we're not longer ref counted loading the header into memory is pretty small so why not 
			// leave the bundle loaded? some one might decide they want it
			//if (!IsRefCounted(download.GetAssetBundleName()))
			//{
			//	LogWarning("ProcessFinishedOperation() Finished downloading " + download.GetAssetBundleName() + " but is no longer ref counted");
			//}
			m_LoadedAssetBundles.Add(download.GetAssetBundleName(), new LoadedAssetBundle(download.GetAssetBundle()));
		}

		private void UpdateLoadedBundles()
		{
			bool canUnload = true;
			for (int i = 0; i < m_UnloadingAssetBundles.Count; i++)
			{
				LoadedAssetBundle bundle = m_UnloadingAssetBundles[i];
				bundle.Update();
				if (!bundle.CanUnload())
				{
					canUnload = false;
				}
			}

			// We need to wait for all bundle to be ready to unload, 
			// otherwise we might unload a dependency that an async load needs to finish
			if (canUnload)
			{
				for (int i = 0; i < m_UnloadingAssetBundles.Count; i++)
				{
					m_UnloadingAssetBundles[i].Unload();
				}
				m_UnloadingAssetBundles.Clear();
			}

			// Update loaded bundles
			foreach (LoadedAssetBundle bundle in m_LoadedAssetBundles.Values)
			{
				bundle.Update();
			}
		}

		private void UpdateLoadsFromCache()
		{
			foreach (AssetBundleDownload download in m_LoadFromCacheOperations.Values)
			{
				if (download != null)
				{
					download.Update();
					if (!download.IsDone())
					{
						continue;
					}
					ProcessFinishedDownload(download);
				}
				m_RemoveFromLoadFromCache.Add(download.GetAssetBundleName());
			}

			foreach (string key in m_RemoveFromLoadFromCache)
			{
				m_LoadFromCacheOperations.Remove(key);
			}
			m_RemoveFromLoadFromCache.Clear();
		}

		private void UpdateUnpacking()
		{
			int completed = 0;
			for (int i = 0; i < m_LoadOperations.Count; i++)
			{
				BaseBundleHandle load = m_LoadOperations[i];
				bool done = load.IsDone();
				load.Update();
				if (load.IsDone())
				{
					m_LoadOperations.RemoveAt(i);
					i--;
					if (!done) // Check if the load completed this update
					{
						completed++;
						// Cap the number of loads we complete each frame so we don't get big spikes from unpacking too many assets at once
						if (completed >= MAX_LOADS)
						{
							break;
						}
					}
				}
			}
		}

		protected override void OnUpdate()
		{
			if (!IsInitialized())
			{
				return;
			}
			UpdateLoadedBundles();
			if (IsUnloading())
			{
				// If we're unloading we don't want to be doing anything else
				// It's not safe to load an asset bundle that is in the process of being unloaded
				return;
			}
			//Log("Update() Downloads: " + m_DownloadOperations.Count + " Cacheloads: " + m_LoadFromCacheOperations.Count + " " + Time.time);
			base.OnUpdate();
			UpdateLoadsFromCache();
			UpdateUnpacking();
		}

		protected override void AddToDebugString()
		{
			Core.Str.AddLine("Unload bundles: ", m_UnloadingAssetBundles.Count.ToString());
			foreach (LoadedAssetBundle unloading in m_UnloadingAssetBundles)
			{
				Core.Str.AddLine("    ", unloading.GetBundleName(), " can unload: ", unloading.CanUnload().ToString());
			}
			Core.Str.AddLine("Loaded bundles: ", m_LoadedAssetBundles.Count.ToString());
			foreach (LoadedAssetBundle loaded in m_LoadedAssetBundles.Values)
			{
				References refCount = null;
				m_References.TryGetValue(loaded.GetBundleName(), out refCount);
				Core.Str.AddLine("    ", loaded.GetBundleName(),
					" ref count: ", refCount == null ? "0" : refCount.Count.ToString(),
					IsPersistent(loaded.GetBundleName(), out PersistentType type) ? $" {System.Enum.GetName(typeof(PersistentType), type)}" : "",
					" can unload: ", loaded.CanUnload().ToString());
				foreach (UnityEngine.Object obj in loaded.GetObjects())
				{
					Core.Str.AddLine("        ", obj != null ? obj.name : "null");
				}
			}
			Core.Str.AddLine("Unpacking assets: ", m_LoadOperations.Count.ToString());
			for (int i = 0; i < m_LoadOperations.Count; i++)
			{
				BaseBundleHandle unpack = m_LoadOperations[i];
				Core.Str.AddLine("    ", unpack.ToString());
			}
			Core.Str.AddLine("Load from cache bundles: ", m_LoadFromCacheOperations.Count.ToString());
			foreach (AssetBundleDownload download in m_LoadFromCacheOperations.Values)
			{
				Core.Str.AddLine("    ", download.GetAssetBundleName());
			}
		}
	}
}
