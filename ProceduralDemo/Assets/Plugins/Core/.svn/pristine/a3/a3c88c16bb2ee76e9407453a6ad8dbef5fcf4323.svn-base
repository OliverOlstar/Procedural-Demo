
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public partial class ABMBase
	{
		List<string> m_DebugStates = new List<string>();
		public List<string> GetDebugStates() { return m_DebugStates; }
		List<float> m_DebugTimeStamps = new List<float>();
		public List<float> GetDebugTimeStamps() { return m_DebugTimeStamps; }
		bool m_Debugging = false;
		public void SetDebugMode(bool debugging) { m_Debugging = debugging; }

		public string GetDebugString()
		{
			GetBytes(out ulong current, out ulong total);
			Core.Str.AddLine("Download size: ", UIUtil.BytesToString(total));
			m_Downloader.AddToDebugString();
			AddToDebugString();
			return Core.Str.Finish();
		}

		protected virtual void AddToDebugString() { }

		private void DebugUpdate()
		{
			if (!m_Debugging)
			{
				return;
			}
			string debug = GetDebugString();
			if (m_DebugStates.Count > 0)
			{
				string last = m_DebugStates[m_DebugStates.Count - 1];
				if (string.Equals(debug, last))
				{
					return;
				}
				if (m_DebugStates.Count >= 100)
				{
					m_DebugStates.RemoveAt(0);
					m_DebugTimeStamps.RemoveAt(0);
				}
			}
			m_DebugStates.Add(debug);
			m_DebugTimeStamps.Add(Time.realtimeSinceStartup);
		}

		internal void Log(string s)
		{
			Debug.Log(Str.Build("<color=green>[", m_DebugName, "]</color> ", s));
		}
		internal void LogWarning(string s)
		{
			Debug.LogWarning(Core.Str.Build("<color=green>[", m_DebugName, "]</color> ", s));
		}
		internal void LogError(string s)
		{
			Debug.LogError(Core.Str.Build("<color=green>[", m_DebugName, "]</color> ", s));
		}
		internal void DevException(string s)
		{
			Core.DebugUtil.DevException(Core.Str.Build("<color=green>[", m_DebugName, "]</color> ", s));
		}

		private void LogCacheInfo()
		{
			Core.Str.AddLine("Cache info: ");
			for (int x = 0; x < Caching.cacheCount; ++x)
			{
				Cache cache = Caching.GetCacheAt(x);
				AddCacheInfoToString("Cache number " + x, cache);
			}
			AddCacheInfoToString("Default cache", Caching.defaultCache);
			AddCacheInfoToString("Current cache", Caching.currentCacheForWriting);
			Core.Str.AddLine();
			Core.Str.AddLine("Bundle hashes: ");
			foreach (string bundleName in m_Dependencies.Keys)
			{
				AddCachedVersionsToString(bundleName);
			}
			Log(Core.Str.Finish());
		}

		private void AddCacheInfoToString(string cacheName, Cache cache)
		{
			Core.Str.AddLine(cacheName, " info");
			Core.Str.AddLine("path: ", cache.path);
			Core.Str.AddLine("used space: ", Core.UIUtil.BytesToString((ulong)cache.spaceOccupied));
			Core.Str.AddLine("free space: ", Core.UIUtil.BytesToString((ulong)cache.spaceFree));
			Core.Str.AddLine("max space: ", Core.UIUtil.BytesToString((ulong)cache.maximumAvailableStorageSpace));
			Core.Str.AddLine("expiration: ", cache.expirationDelay.ToString());
			Core.Str.AddLine("read only: ", cache.readOnly.ToString());
			Core.Str.AddLine("ready: ", cache.ready.ToString());
			Core.Str.AddLine("valid: ", cache.valid.ToString());
		}

		private void AddCachedVersionsToString(string bundleName)
		{
			List<Hash128> listOfCachedVersions = ListPool<Hash128>.Request();
			Caching.GetCachedVersions(bundleName, listOfCachedVersions);
			Core.Str.AddLine(bundleName);
			for (int x = 0; x < listOfCachedVersions.Count; x++)
			{
				Core.Str.AddLine("  ", listOfCachedVersions[x].ToString());
			}
			ListPool<Hash128>.Return(listOfCachedVersions);
		}
	}
}
