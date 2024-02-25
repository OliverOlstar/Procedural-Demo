using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Core
{
	[Serializable]
	public class StreamingBundlesManifest
	{
		public const string MANIFEST_NAME = "intro_bundle_manifest.json";
		public const float DEFAULT_MAX_ANDROID_SIZE = 50f;
		public const float DEFAULT_MAX_IOS_SIZE = 60f;

		public static StreamingBundlesManifest FromJson(string json)
		{
			StreamingBundlesManifest manifest = JsonConvert.DeserializeObject<StreamingBundlesManifest>(json);
			return manifest;
		}

		[JsonProperty("iOS")]
		private Dictionary<string, string[]> m_iOS = null;
		[JsonProperty("Android")]
		private Dictionary<string, string[]> m_Android = null;
		[JsonProperty("Windows")]
		private Dictionary<string, string[]> m_Windows = null;
#pragma warning disable
		[JsonProperty("maxIosSize")]
		private float m_MaxIosSize = DEFAULT_MAX_IOS_SIZE;
		[JsonProperty("maxAndroidSize")]
		private float m_MaxAndroidSize = DEFAULT_MAX_ANDROID_SIZE;
#pragma warning restore
		[JsonProperty("failOversizeBuilds")]
		private bool m_FailOversizeBuilds = false;

		public float MaxBundleBuildSize
		{
			get
			{
#if UNITY_ANDROID
				return m_MaxAndroidSize;
#elif UNITY_IOS
				return m_MaxIosSize;
#else
				return float.MaxValue;
#endif
			}
		}
		public bool FailOversizeBuilds { get { return m_FailOversizeBuilds; } }

		public IEnumerable<string> GetBundleNames()
		{
			Dictionary<string, string[]> bundles = GetCurrentPlatformBundles();
			foreach (KeyValuePair<string, string[]> kvp in bundles)
			{
				for (int i = 0; i < kvp.Value.Length; ++i)
				{
					yield return kvp.Value[i];
				}
			}
		}

		public Dictionary<string, string[]> GetBundlesByManager()
		{
			return GetCurrentPlatformBundles();
		}

		public Dictionary<string, string[]> GetBundlesByManagerForPlatform(string platform)
		{
			return GetPlatformBundles(platform);
		}

		private Dictionary<string, string[]> GetCurrentPlatformBundles()
		{
#if UNITY_ANDROID
			return m_Android;
#elif UNITY_IOS
			return m_iOS;
#else
			return m_Windows;
#endif
		}

		private Dictionary<string, string[]> GetPlatformBundles(string platform)
		{
			switch (platform)
			{
				case "Android":
					return m_Android;
				case "iOS":
					return m_iOS;
				default:
					return m_Windows;
			}
		}
	}
}
