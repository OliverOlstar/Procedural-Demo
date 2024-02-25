using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public class AssetBundleDownloader
	{
		public enum Priority
		{
			None = 0,
			RequestEssential,
			RequestNonEssential,
			BackgroundQueue,
			BackgroundRandom
		}

		private class DownloadQueue
		{
			private Queue<string> m_Queue = new Queue<string>(); // This data structure is duplicated to support different queries
			private HashSet<string> m_Set = new HashSet<string>(); // HashSet for quick contains checks

			public int Count => m_Queue.Count;
			public StructEnumerable.Queue<string> QueuedBundlesNames => m_Queue;

			public string Dequeue()
			{
				string bundle = m_Queue.Dequeue();
				m_Set.Remove(bundle);
				return bundle;
			}

			public bool TryAdd(string assetBundleName)
			{
				if (m_Set.Contains(assetBundleName))
				{
					return false;
				}
				m_Queue.Enqueue(assetBundleName);
				m_Set.Add(assetBundleName);
				return true;
			}

			public void Clear()
			{
				m_Queue.Clear();
				m_Set.Clear();
			}
		}

		public static readonly int DEFAULT_DOWNLOADS = 4;
		public static readonly int MAX_DOWNLOADS = 8;

		private Dictionary<string, AssetBundleDownload> m_DownloadOperations = new Dictionary<string, AssetBundleDownload>();
		public bool ContainsDownloadOperation(string bundleName) => m_DownloadOperations.ContainsKey(bundleName);
		public int GetDownloadCount() { return m_DownloadOperations.Count; }

		private Dictionary<Priority, DownloadQueue> m_RequestedDownloadQueues = new Dictionary<Priority, DownloadQueue>()
		{
			{ Priority.RequestEssential, new DownloadQueue() },
			{ Priority.RequestNonEssential, new DownloadQueue() },
		};

		private Queue<string> m_BackgroundDownloadQueue = new Queue<string>();

		private AssetBundleDownload[] m_DownloadSlots = new AssetBundleDownload[MAX_DOWNLOADS];

		private ulong m_DownloadedBytes = 0;
		private ABMBase m_ABM = null;
		private System.Action<AssetBundleDownload> m_OnDownloadComplete = null;
		private int m_DownloadSlotCount = DEFAULT_DOWNLOADS;
		private ABM.DownloadMode m_DownloadMode = ABMBase.DownloadMode.Allowed;
		private ABM.BackgroundDownloadMode m_BackgrounDownloadMode = ABMBase.BackgroundDownloadMode.Paused;

		public void Initialize(ABMBase abm, System.Action<AssetBundleDownload> onDownloadComplete)
		{
			m_ABM = abm;
			m_OnDownloadComplete = onDownloadComplete;
		}

		public void SetDownloadSlotCount(int count)
		{
			m_DownloadSlotCount = Mathf.Clamp(count, 1, m_DownloadSlots.Length);
			if (Core.DebugOptions.ABMLogs.IsSet()) m_ABM.Log(Core.Str.Build("SetDownloadSlotCount: ", m_DownloadSlotCount.ToString()));
		}

		public void SetDownloadMode(ABM.DownloadMode mode)
		{
			if (m_DownloadMode != mode)
			{
				if (Core.DebugOptions.ABMLogs.IsSet()) m_ABM.Log(Core.Str.Build("SetDownloadMode: ", mode.ToString()));
				m_DownloadMode = mode;
			}
		}

		public void SetBackgroundDownloadMode(ABM.BackgroundDownloadMode mode)
		{
			if (m_BackgrounDownloadMode != mode)
			{
				if (Core.DebugOptions.ABMLogs.IsSet()) m_ABM.Log(Core.Str.Build("SetBackgroundDownloadMode: ", mode.ToString()));
				m_BackgrounDownloadMode = mode;
			}
		}

		public bool IsDownloadingEssentialBundles()
		{
			// Check requests, download operations might not have been marked essential yet
			if (m_RequestedDownloadQueues[Priority.RequestEssential].Count > 0)
			{
				return true;
			}
			foreach (var dl in m_DownloadOperations.Values)
			{
				if (dl.DownloadPriority == Priority.RequestEssential)
				{
					return true;
				}
			}
			return false;
		}

		public void AddBundleToDownloadQueue(string assetBundleName)
		{
			if (m_ABM.IsSimulationMode())
			{
				return;
			}
			if (!m_ABM.BundleExists(assetBundleName))
			{
				m_ABM.LogError(Core.Str.Build("AddBundleToDownloadQueue() Bundle ", assetBundleName, " doesn't exist"));
				return;
			}
			if (m_DownloadOperations.ContainsKey(assetBundleName))
			{
				m_BackgroundDownloadQueue.Enqueue(assetBundleName);
			}
		}

		public void AddBundlesToDownloadQueue(IEnumerable<string> assetBundleNames)
		{
			if (m_ABM.IsSimulationMode())
			{
				return;
			}
			foreach (string assetBundleName in assetBundleNames)
			{
				AddBundleToDownloadQueue(assetBundleName);
			}
		}

		public void DownloadAssetBundle(
			string assetBundleName, 
			string assetBundleURL,
			Hash128 hash,
			ulong size,
			bool requiresInternet) // Note: This bundle could come from streaming assets
		{
			if (m_ABM.IsSimulationMode())
			{
				return;
			}
			AssetBundleDownload dl = new AssetBundleDownload(assetBundleName, assetBundleURL, hash, size, requiresInternet);
			m_DownloadOperations.Add(assetBundleName, dl);
		}

		// Note: Internal version does not handle dependencies
		public void RequestDownload(string assetBundleName, ABM.DownloadPriority abmPriority, string parentBundle = null)
		{
			if (m_ABM.IsSimulationMode())
			{
				return;
			}
			if (!m_DownloadOperations.ContainsKey(assetBundleName))
			{
				return; // Doesn't need to be downloaded
			}
			if (m_DownloadMode == ABMBase.DownloadMode.Restricted && !m_ABM.StreamingCatalog.AllowDownload(assetBundleName))
			{
				m_ABM.DevException(string.IsNullOrEmpty(parentBundle) ?
					$"RequestDownload Downloading is restricted and bundle {assetBundleName} is not found in {nameof(AssetBundlesConfig)}" :
					$"RequestDownload Downloading is restricted and bundle {parentBundle} is dependent on {assetBundleName} which cannot be found in {nameof(AssetBundlesConfig)}");
			}
			Priority priority = abmPriority == ABMBase.DownloadPriority.Essential ? Priority.RequestEssential : Priority.RequestNonEssential;
			DownloadQueue queue = m_RequestedDownloadQueues[priority];
			queue.TryAdd(assetBundleName);
		}

		private AssetBundleDownload GetNextDownload(int slotIndex)
		{
			// These bundles have handles waiting on them
			DownloadQueue downloadRequestQueue = m_RequestedDownloadQueues[Priority.RequestEssential];
			while (downloadRequestQueue.Count > 0)
			{
				string assetBundleName = downloadRequestQueue.Dequeue();
				if (m_DownloadOperations.TryGetValue(assetBundleName, out AssetBundleDownload download) && !download.Started())
				{
					download.SetDownloadPriority(Priority.RequestEssential);
					return download;
				}
			}
			if (InternetReachabilityDirector.InternetReachability == NetworkReachability.NotReachable) // Note: we Download essential bundles regardless of connection so we can detect when we're stuck waiting for them
			{
				return null;
			}
			// These bundles also have handles waiting on them, but have been marked nonessential so we know the client isn't required to go
			// online to download them in order to progress
			downloadRequestQueue = m_RequestedDownloadQueues[Priority.RequestNonEssential];
			while (downloadRequestQueue.Count > 0)
			{
				string assetBundleName = downloadRequestQueue.Dequeue();
				if (m_ABM.IsDownloadRequestStillValid(assetBundleName) && // Non-essential download may no longer be ref counted by the time we get to download them
					m_DownloadOperations.TryGetValue(assetBundleName, out AssetBundleDownload download) &&
					!download.Started())
				{
					download.SetDownloadPriority(Priority.RequestNonEssential);
					return download;
				}
			}
			if (m_BackgrounDownloadMode == ABM.BackgroundDownloadMode.Paused ||
				slotIndex != (m_DownloadSlotCount - 1) || // Queued downloads are only allowed in the last slot
				ABMs.IsDownloadingEssentialBundles()) // Don't waste bandwidth if an ABM has essential downloads
			{
				return null;
			}
			while (m_BackgroundDownloadQueue.Count > 0)
			{
				string assetBundleName = m_BackgroundDownloadQueue.Dequeue();
				// Make sure download hasn't finished or and isn't already in progress
				if (m_DownloadOperations.TryGetValue(assetBundleName, out AssetBundleDownload download) && !download.Started())
				{
					download.SetDownloadPriority(Priority.BackgroundQueue);
					return download;
				}
			}
			if (m_BackgrounDownloadMode != ABM.BackgroundDownloadMode.AllBundles)
			{
				return null;
			}
			// Once we've gone through all the priority bundles start downloading the rest in any orders
			foreach (AssetBundleDownload dl in m_DownloadOperations.Values)
			{
				if (!dl.Started())
				{
					return dl;
				}
			}
			return null;
		}

		public void UpdateDownloads()
		{
			for (int i = 0; i < m_DownloadSlots.Length; i++)
			{
				AssetBundleDownload download = m_DownloadSlots[i];
				if (download != null)
				{
					download.Update();
					if (!download.IsDone())
					{
						continue;
					}
					DownloadComplete(download);
					m_DownloadOperations.Remove(download.GetAssetBundleName());
					m_DownloadSlots[i] = null;
				}
				// If slot count is reduced we still need to finish updating the download in this slot, we just won't add a new one
				if (i >= m_DownloadSlotCount)
				{
					continue;
				}
				// Try to find a download for this slot
				AssetBundleDownload nextDownload = GetNextDownload(i);
				if (nextDownload == null)
				{
					continue;
				}
				m_DownloadSlots[i] = nextDownload;
				if (Core.DebugOptions.ABMLogs.IsSet())
				{
					for (int j = 0; j < m_DownloadSlots.Length; j++)
					{
						if (m_DownloadSlots[j] == null)
						{
							if (j < m_DownloadSlotCount)
							{
								Core.Str.Add("[]");
							}
						}
						else if (j == i)
						{
							Core.Str.Add("*", m_DownloadSlots[j].GetAssetBundleName(), "*");
						}
						else
						{
							Core.Str.Add("[", m_DownloadSlots[j].GetAssetBundleName(), "]");
						}
					}
					ulong size = nextDownload.GetTotalBytes();
					m_ABM.Log(Core.Str.Build("Starting ", nextDownload.DownloadPriority.ToString(), " download ", nextDownload.GetSourceURL(), " ",
						size.ToString(), " (", UIUtil.BytesToString(size), ")\n", Core.Str.Finish()));
				}
				i--; // Update this slot again, very important that newly added AssetBundleDownload immediately get a call to Update()
			}
		}

		private void DownloadComplete(AssetBundleDownload download)
		{
			ulong expectedSize = download.GetTotalBytes();
			ulong actualSize = download.GetDownloadedBytes();
			// If we downloaded an essential bundle add the bytes, we don't track bundles streaming in the background
			if (download.DownloadPriority == Priority.RequestEssential)
			{
				m_DownloadedBytes += expectedSize;
			}
			// Check if downloaded size matches our expected size
			if (expectedSize > 0 && expectedSize != actualSize)
			{
				m_ABM.LogWarning(Core.Str.Build("ProcessFinishedOperation() ", download.GetAssetBundleName(),
					" download size ", actualSize.ToString(),
					" is not expected size ", expectedSize.ToString()));
			}
			if (Core.DebugOptions.ABMLogs.IsSet())
			{
				m_ABM.Log(Core.Str.Build("Completed ",
					download.DownloadPriority == Priority.RequestEssential ? "essential request " :
					download.DownloadPriority == Priority.RequestNonEssential ? "non-essential request " :
					"background ",
					download.GetAssetBundleName(), " ",
					actualSize.ToString(), " (",
					UIUtil.BytesToString(actualSize), ") ",
					download.GetTimer().ToString("F2"), "s"));
			}
			m_OnDownloadComplete.Invoke(download);
		}

		public void Reset()
		{
			foreach (DownloadQueue queue in m_RequestedDownloadQueues.Values)
			{
				queue.Clear();
			}
			m_BackgroundDownloadQueue.Clear();

			foreach (AssetBundleDownload download in m_DownloadOperations.Values)
			{
				download.Abort();
			}
			m_DownloadOperations.Clear();
			for (int i = 0; i < m_DownloadSlots.Length; i++)
			{
				m_DownloadSlots[i] = null;
			}
			m_DownloadedBytes = 0;
		}

		public void GetBytes(out ulong current, out ulong total)
		{
			current = 0;
			total = 0;
			foreach (AssetBundleDownload download in m_DownloadSlots)
			{
				if (download != null && download.DownloadPriority == Priority.RequestEssential)
				{
					ulong bundleSize = download.GetTotalBytes();
					total += bundleSize;
					current += download.GetDownloadedBytes();
				}
			}
			current += m_DownloadedBytes;
			total += m_DownloadedBytes;
		}

		public void AddToDebugString()
		{
			foreach (KeyValuePair<Priority, DownloadQueue> pair in m_RequestedDownloadQueues)
			{
				Core.Str.AddLine(pair.Key.ToString(), " download requests: ", pair.Value.Count.ToString());
				foreach (string bundleName in pair.Value.QueuedBundlesNames)
				{
					Core.Str.AddLine("    ", bundleName);
				}
			}
			Core.Str.AddLine("Download slots: ", m_DownloadSlotCount.ToString());
			for (int i = 0; i < m_DownloadSlots.Length; i++)
			{
				AssetBundleDownload download = m_DownloadSlots[i];
				if (download != null)
				{
					Core.Str.AddLine("    ", download.GetAssetBundleName());
				}
				else if (i < m_DownloadSlotCount)
				{
					Core.Str.AddLine("    Empty");
				}
			}
			Core.Str.AddLine("Downloading bundles: ", m_DownloadOperations.Count.ToString());
			foreach (AssetBundleDownload download in m_DownloadOperations.Values)
			{
				Core.Str.AddLine("    ", download.GetAssetBundleName());
			}
		}
	}
}
