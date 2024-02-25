using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Core
{
    public class StreamingBundlesCatalog
    {
        private ABMBase m_ABM = null;
        private Dictionary<string, string> m_Catalog = new Dictionary<string, string>();

        public bool Contains(string bundleName) => m_Catalog.ContainsKey(bundleName);
        public bool TryGetStreamingPath(string bundleName, out string streamingBundlePath) => 
            m_Catalog.TryGetValue(bundleName, out streamingBundlePath);

        private HashSet<string> m_HotfixedBundles = new HashSet<string>();
        public IEnumerable<string> HotfixedBundles => m_HotfixedBundles;
        public bool AllowDownload(string bundleName) => !m_IsConfigured || m_HotfixedBundles.Contains(bundleName);
        public bool HasAnyBundleBeenHotfixed => m_HotfixedBundles.Count > 0;
        private bool m_IsConfigured = false;

        public IEnumerator Initialize(ABMBase abm)
        {
            m_ABM = abm;
            m_Catalog.Clear();
            AssetBundleUtil.Platform platform = AssetBundleUtil.GetPlatform();
            m_IsConfigured = AssetBundlesConfig.HasConfigs(platform, m_ABM.BuildName);
            if (!m_IsConfigured)
            {
                if (Core.DebugOptions.ABMLogs.IsSet()) m_ABM.Log("StreamingBundlesCatalog not configured for this platform " + platform);
                yield break;
            }
            string manifestName = AssetBundleUtil.GetManifestBundleName(platform, m_ABM.BuildName);
            string path = Core.Str.Build(
                Application.streamingAssetsPath,
                "/AssetBundles/",
                platform.ToString(), "/",
                manifestName, "/");
			if (!path.Contains("://"))
			{
				path = "file://" + path;
			}
            string manifestPath = Core.Str.Build(path, manifestName);
            if (Core.DebugOptions.ABMLogs.IsSet()) m_ABM.Log("StreamingBundlesCatalog Initialize " + manifestPath + "\n" + Application.streamingAssetsPath + "\n" + Application.dataPath);
            UnityWebRequest manifestRequest = UnityWebRequestAssetBundle.GetAssetBundle(manifestPath);
			manifestRequest.certificateHandler = new ForceAcceptAll();
			manifestRequest.disposeCertificateHandlerOnDispose = true;
			manifestRequest.SendWebRequest();
            while (!manifestRequest.isDone)
            {
                yield return null;
            }
			AssetBundleManifest streamingManifest = StreamingBundlesUtil.GetContentFromWebRequest<AssetBundleManifest>(
				manifestRequest,
				AssetBundleUtil.MANIFEST_ASSET_NAME,
				out StreamingBundlesUtil.ErrorType errorType,
				out string error);
			switch (errorType)
			{
				case StreamingBundlesUtil.ErrorType.Warning:
					m_ABM.LogWarning(error);
					break;
				case StreamingBundlesUtil.ErrorType.Error:
					m_ABM.LogError(error);
					break;
			}
            manifestRequest.Dispose();
            if (streamingManifest == null)
			{
                // In editor or local builds this can happen when Steaming Assets haven't been built.
                // If we mark them all has having been "hotfixed" then the system will accept using the downloaded versions
                m_ABM.LogWarning($"SteamingBundlesCatalog.Initialize() Failed to load streaming bundles manifest {AssetBundleUtil.MANIFEST_ASSET_NAME}");
                foreach (string assetBundleName in m_ABM.GetManifest().GetAllAssetBundles())
                {
                    if (AssetBundlesConfig.ContainsBundle(platform, m_ABM.BuildName, assetBundleName))
                    {
                        m_HotfixedBundles.Add(assetBundleName);
                    }
                }
                yield break; // CompleteWebrequest will make logs if something went wrong
            }
            AssetBundleManifest downloadManifest = m_ABM.GetManifest();
            foreach (string bundleName in streamingManifest.GetAllAssetBundles())
			{
                if (!AssetBundlesConfig.ContainsBundle(platform, m_ABM.BuildName, bundleName))
                {
                    continue;
                }
                if (Core.DebugOptions.ABMLogs.IsSet()) Core.Str.Add(bundleName, ".........");
                if (!m_ABM.BundleExists(bundleName))
				{
                    if (Core.DebugOptions.ABMLogs.IsSet()) Core.Str.AddLine("no longer required");
                    continue;
				}
                Hash128 streamingHash = streamingManifest.GetAssetBundleHash(bundleName);
                Hash128 downloadHash = downloadManifest.GetAssetBundleHash(bundleName);
                if (streamingHash != downloadHash)
				{
                    m_HotfixedBundles.Add(bundleName);
                    if (Core.DebugOptions.ABMLogs.IsSet()) Core.Str.AddLine("HOTFIXED, hash ", streamingHash.ToString(), " != ", downloadHash.ToString());
                    continue;
				}
                string bundlePath = Core.Str.Build(path, bundleName);
                if (Core.DebugOptions.ABMLogs.IsSet()) Core.Str.AddLine(bundlePath);
                m_Catalog.Add(bundleName, bundlePath);
            }
            if (Core.DebugOptions.ABMLogs.IsSet()) m_ABM.Log("StreamingBundlesCatalog Initialized with " + m_Catalog.Count + " bundles\n" + Core.Str.Finish());
            // We want to make sure these bundles are downloaded first
            if (m_HotfixedBundles.Count > 0)
			{
                if (Core.DebugOptions.ABMLogs.IsSet()) m_ABM.Log("StreamingBundlesCatalog Adding " + m_HotfixedBundles.Count + " hotfixed bundles to download queue");
                abm.AddBundlesToDownloadQueue(m_HotfixedBundles);
			}
        }
    }

	public static class StreamingBundlesUtil
	{
		public enum ErrorType
		{
			None = 0,
			Warning,
			Error
		}

		public static TAsset GetContentFromWebRequest<TAsset>(UnityWebRequest request, string assetName, out ErrorType errorType, out string error) 
			where TAsset : UnityEngine.Object
		{
			if (!request.isDone)
			{
				errorType = ErrorType.Error;
				error = "StreamingBundlesUtil.RequestAsset() " + request.url + " is not complete";
				return null;
			}
			if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				errorType = ErrorType.Error;
				error = "StreamingBundlesUtil.RequestAsset() " + request.url + " failed with error " + request.error;
				return null;
			}
			if (request.result == UnityWebRequest.Result.ProtocolError)
			{
				if (Application.isEditor && request.responseCode == 404)
				{
					// In editor this might be because StreamAssets hasn't been built
					errorType = ErrorType.Warning;
					error = "StreamingBundlesUtil.RequestAsset() " + request.url + " failed with http error " + request.responseCode + " did you build Streaming Assets?";
				}
				else
				{
					// On device this is defintely a problem
					errorType = ErrorType.Error;
					error = "StreamingBundlesUtil.RequestAsset() " + request.url + " failed with http error " + request.responseCode;
				}
				return null;
			}
			AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
			if (bundle == null)
			{
				errorType = ErrorType.Error;
				error = "StreamingBundlesUtil.RequestAsset() " + request.url + " failed bundle is null";
				return null;
			}
			TAsset asset = bundle.LoadAsset<TAsset>(assetName);
			bundle.Unload(false);
			if (asset == null)
			{
				errorType = ErrorType.Error;
				error = "StreamingBundlesUtil.RequestAsset() " + request.url + " failed " + assetName + " is null";
				return null;
			}
			errorType = ErrorType.None;
			error = null;
			return asset;
		}
	}
}
