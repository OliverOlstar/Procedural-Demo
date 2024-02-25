
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	[System.Serializable]
	public class AssetBundleCatalogue
	{
		private Dictionary<string, string> m_AssetsToBundle = new Dictionary<string, string>();
		private Dictionary<string, List<string>> m_BundleToAssets = new Dictionary<string, List<string>>();

		public bool Add(string assetName, string bundleName)
		{
			if (m_AssetsToBundle.ContainsKey(assetName))
			{
				return false;
			}
			m_AssetsToBundle.Add(assetName, bundleName);
			if (!m_BundleToAssets.TryGetValue(bundleName, out List<string> assets))
			{
				assets = new List<string>();
				m_BundleToAssets.Add(bundleName, assets);
			}
			assets.Add(assetName);
			return true;
		}

		public bool ContainsAsset(string assetName)
		{
			return m_AssetsToBundle.ContainsKey(assetName);
		}

		public bool TryGetBundleName(string assetName, out string bundleName)
		{
			return m_AssetsToBundle.TryGetValue(assetName, out bundleName);
		}

		public bool TryGetAssetNames(string bundleName, out IReadOnlyList<string> assetNames)
		{
			if (m_BundleToAssets.TryGetValue(bundleName, out List<string> list))
			{
				assetNames = list;
				return true;
			}
			assetNames = default;
			return false;
		}

		public void Clear()
		{
			m_AssetsToBundle.Clear();
			m_BundleToAssets.Clear();
		}

		public void EditorInitialize()
		{
#if UNITY_EDITOR
			// If we're running in simulation mode we want to build a new up to date asset catalogue
			string[] bundleNames = UnityEditor.AssetDatabase.GetAllAssetBundleNames();
			foreach (string bundle in bundleNames)
			{
				string[] paths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(bundle);
				foreach (string path in paths)
				{
					string assetName = System.IO.Path.GetFileNameWithoutExtension(path);
					Add(assetName, bundle);
				}
			}
#endif
		}
	}
}
