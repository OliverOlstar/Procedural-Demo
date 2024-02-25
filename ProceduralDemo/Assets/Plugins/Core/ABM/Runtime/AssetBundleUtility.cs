
using UnityEngine;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core
{
	public class AssetBundleUtil
	{
		public static readonly string MANIFEST_ASSET_NAME = "AssetBundleManifest";

#if UNITY_EDITOR
		private static int s_SimulateAssetBundleInEditor = -1;
		private const string SIMULATE_BUNDLES_KEY = "SimulateAssetBundles";
		private const string USE_LOCAL_BUNDLES_KEY = "UseLocalAssetBundles";
		private const string LOCAL_BUNDLES_PATH_KEY = "LocalAssetBundlesPath";

		public static bool IsSimMode()
		{
			if (s_SimulateAssetBundleInEditor == -1)
			{
				s_SimulateAssetBundleInEditor = EditorPrefs.GetBool(SIMULATE_BUNDLES_KEY, true) ? 1 : 0;
			}
			return s_SimulateAssetBundleInEditor != 0;
		}

		public static void SetSimulationMode(bool simMode)
		{
			int newValue = simMode ? 1 : 0;
			if (newValue != s_SimulateAssetBundleInEditor)
			{
				s_SimulateAssetBundleInEditor = newValue;
				EditorPrefs.SetBool(SIMULATE_BUNDLES_KEY, simMode);
			}
		}

		public static bool UseLocalBundlePath
		{
			get
			{
				return EditorPrefs.GetBool(USE_LOCAL_BUNDLES_KEY, false);
			}
			set
			{
				EditorPrefs.SetBool(USE_LOCAL_BUNDLES_KEY, value);
			}
		}
		public static string LocalBundlePath
		{
			get
			{
				return EditorPrefs.GetBool(USE_LOCAL_BUNDLES_KEY, false) ?
					EditorPrefs.GetString(LOCAL_BUNDLES_PATH_KEY, GetDefaultLocalBundlesPath()) :
					GetDefaultLocalBundlesPath();
			}
			set
			{
				EditorPrefs.SetString(LOCAL_BUNDLES_PATH_KEY, value);
			}
		}

		public static void UseDefaultLocalBundlePath()
		{
			EditorPrefs.DeleteKey(LOCAL_BUNDLES_PATH_KEY);
		}

		private static string GetDefaultLocalBundlesPath()
		{
			return System.IO.Path.GetFullPath("./AssetBundles").Replace('\\', '/');
		}
#else
		public static bool IsSimMode()
		{
			return false;
		}

		public static bool UseLocalBundlePath
		{
			get { return false; }
		}
		public static string LocalBundlePath
		{
			get { return string.Empty; }
		}

#endif

		public const string AssetBundlesOutputPath = "AssetBundles";

		public static string GetManifestBundleName(Platform platform, string buildName)
		{
			return Core.Str.Build(platform.ToString(), buildName);
		}

		public static string GetManifestBundleName(string buildName)
		{
			return GetManifestBundleName(GetPlatform(), buildName);
		}

		public static string BuildBundleBaseURL(string assetBundleURL, string manifestAssetBundleName)
		{
			return Core.Str.Build(
				assetBundleURL,
				!assetBundleURL.EndsWith("/") ? "/" : Core.Str.EMPTY,
				GetPlatformName(), "/",
				manifestAssetBundleName, "/");
		}

		public static int GetBundleVersionFromURL(string url)
		{
			if (string.IsNullOrEmpty(url))
			{
				UnityEngine.Debug.LogWarning($"Couldn't parse AssetBundle version. AssetBundleLocation is null.");
				return -1;
			}
			if (url.Contains("latest"))
			{
				return 0;
			}
			const string delimiter = "/v";
			int index = url.IndexOf(delimiter) + delimiter.Length;
			if (index < 0)
			{
				UnityEngine.Debug.LogWarning($"Couldn't parse AssetBundle version. Missing delimiter: {delimiter}");
				return -1;
			}
			int version = 0;
			while (index < url.Length)
			{
				double value = char.GetNumericValue(url, index);
				if (value < 0d)
				{
					break;
				}
				version *= 10;
				version += (int)value;
				index++;
			}
			return version;
		}

		public static string GetSizesAssetName(string buildName)
		{
			return Core.Str.Build(GetPlatformName(), buildName, "Sizes");
		}
		public static string GetSizesBundleName(string buildName)
		{
			return GetSizesAssetName(buildName).ToLower();
		}

		public static string GetCatalogueAssetName(string buildName)
		{
			return Core.Str.Build(GetPlatformName(), buildName, "Catalogue");
		}
		public static string GetCatalogueBundleName(string buildName)
		{
			return GetCatalogueAssetName(buildName).ToLower();
		}

		public static string GetAssetBundlePlatformPath()
		{
			return Path.Combine(AssetBundlesOutputPath, GetPlatformName());
		}

		public static string GetAssetBundleBuildPath(string buildName)
		{
			return Path.Combine(
				GetAssetBundlePlatformPath(),
				GetManifestBundleName(buildName));
		}

		public static string GetAssetBundleBuildManifestPath(string buildName)
		{
			string manifestName = GetManifestBundleName(buildName);
			string path = Path.Combine(
				GetAssetBundlePlatformPath(),
				manifestName,
				Core.Str.Build(manifestName, ".manifest"));
			Debug.Log("MANIFEST PATH: " + path);
			return path;
		}

		public enum Platform
		{
			None = 0,
			Windows = 1 << 1,
			iOS = 1 << 2,
			Android = 1 << 3,
			OSX = 1 << 4, // This is just here for OSX Editor support
		}

		private static readonly string[] PLATFORM_NAMES = System.Enum.GetNames(typeof(Platform));
		public static IEnumerable<string> AllPlatformNames => PLATFORM_NAMES;

#if UNITY_EDITOR
		private static Platform GetPlatformForAssetBundles(BuildTarget target)
		{
			switch (target)
			{
				case BuildTarget.Android:
					return Platform.Android;
				case BuildTarget.iOS:
					return Platform.iOS;
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					return Platform.Windows;
				case BuildTarget.StandaloneOSX:
					return Platform.OSX;
				default:
					throw new System.ArgumentException("AssetBundleUtil.GetPlatformForAssetBundles() Unexpected BuildTarget " + target);
			}
		}
#endif

		private static Platform GetPlatformForAssetBundles(RuntimePlatform platform)
		{
			switch (platform)
			{
				case RuntimePlatform.Android:
					return Platform.Android;
				case RuntimePlatform.IPhonePlayer:
					return Platform.iOS;
				case RuntimePlatform.WindowsPlayer:
					return Platform.Windows;
				case RuntimePlatform.OSXPlayer:
					return Platform.OSX;
				default:
					throw new System.ArgumentException("AssetBundleUtil.GetPlatformForAssetBundles() Unexpected RuntimePlatform " + platform);
			}
		}

		public static Platform GetPlatform()
		{
#if UNITY_EDITOR
			return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
#else
			return GetPlatformForAssetBundles(Application.platform);
#endif
		}

		public static string GetPlatformName() { return GetPlatform().ToString(); }

		public static T Deserialize<T>(TextAsset text)
		{
			object obj = null;
			try
			{
				IFormatter formatter = new BinaryFormatter();
				Stream stream = new MemoryStream(text.bytes);
				obj = formatter.Deserialize(stream);
				stream.Close();
			}
			catch (System.Exception e)
			{
				Debug.LogError("AssetBundleUtil.Deserialize() Exception " + e + " trying to deserialize asset to type " + typeof(T));
				return default(T);
			}

			if (!typeof(T).IsAssignableFrom(obj.GetType()))
			{
				Debug.LogError("AssetBundleUtil.Deserialize() Cannot cast " + obj.GetType() + " to " + typeof(T));
				return default(T);
			}

			return (T)obj;
		}

#if UNITY_EDITOR
		public static T LoadAssetEditor<T>(string bundleName, string assetName, string fallbackAssetName = "") where T : UnityEngine.Object
		{
			string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName, assetName);
			if (assetPaths.Length == 0 && !Core.Str.IsEmpty(fallbackAssetName))
			{
				// Try to load fallback asset
				assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName, fallbackAssetName);
			}
			if (assetPaths.Length == 0)
			{
				Debug.LogError("AssetBundleUtil.LoadAssetEditor() There is no asset with name \"" +
					assetName + "\" or \"" + fallbackAssetName + "\" in " + bundleName);
				return null;
			}

			T asset = AssetDatabase.LoadAssetAtPath<T>(assetPaths[0]);
			if (asset == null)
			{
				Debug.LogError("AssetBundleUtil.LoadAssetEditor() There is no asset \"" + assetPaths[0] + "\" of type " + typeof(T));
			}
			return asset;
		}

		public static T[] LoadAssetWithSubAssets<T>(string bundleName, string assetName, string fallbackAssetName = "") where T : UnityEngine.Object
		{
			string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName, assetName);
			if (assetPaths.Length == 0 && !Core.Str.IsEmpty(fallbackAssetName))
			{
				// Try to load fallback asset
				assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName, fallbackAssetName);
			}
			if (assetPaths.Length == 0)
			{
				Debug.LogError("AssetBundleUtil.LoadAssetEditor() There is no asset with name \"" +
					assetName + "\" or \"" + fallbackAssetName + "\" in " + bundleName);
				return null;
			}

			Object[] arr = AssetDatabase.LoadAllAssetsAtPath(assetPaths[0]);
			var list = new System.Collections.Generic.List<T>();
			for (int i = 0; i < arr.Length; i++)
			{
				if (arr[i] is T el)
				{
					list.Add(el);
				}
			}
			T[] asset = list.ToArray();
			return asset;
		}

		public static T[] LoadAllAssetsEditor<T>(string bundleName) where T : UnityEngine.Object
		{
			string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
			if (assetPaths.Length == 0)
			{
				Debug.LogError("AssetBundleUtil.LoadAssetEditor() There are no assets in bundle " + bundleName);
				return null;
			}
			T[] assets = new T[assetPaths.Length];
			for (int i = 0; i < assetPaths.Length; i++)
			{
				assets[i] = AssetDatabase.LoadAssetAtPath<T>(assetPaths[i]);
			}
			return assets;
		}
#endif
	}
}