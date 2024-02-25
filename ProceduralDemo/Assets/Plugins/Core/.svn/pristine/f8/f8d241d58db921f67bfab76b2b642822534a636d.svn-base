using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Core
{
	public class StreamingAssetBundleBuild
	{
		private static List<string> s_Logs = new List<string>();
		private static List<string> s_Errors = new List<string>();
		private static Dictionary<string, long> s_BundleSizes = new Dictionary<string, long>();

		public static void DoStreamingAssetsBuildAndroid()
		{
			Initialize();
			DeleteStreamingAssetDirectories();
			string path = GetArg("-builtAssetBundlePath");
			CopyBuiltBundlesToStreamingAssetsDirectory(path, AssetBundleUtil.Platform.Android);
			OutputSizeInfo();
			Exit();
		}

		public static bool DoLocalStreamingAssetsBuild(out string error)
		{
			Initialize();
			DeleteStreamingAssetDirectories();
			AssetBundleUtil.Platform platform = AssetBundleUtil.GetPlatform();
			CopyBuiltBundlesToStreamingAssetsDirectory(AssetBundleUtil.LocalBundlePath, platform);
			OutputSizeInfo();
			bool result = GetBuildResult(out error);
			AssetDatabase.Refresh();
			return result;
		}

		public static void DoStreamingAssetsBuildIos()
		{
			Initialize();
			DeleteStreamingAssetDirectories();
			string path = GetArg("-builtAssetBundlePath");
			CopyBuiltBundlesToStreamingAssetsDirectory(path, AssetBundleUtil.Platform.iOS);
			OutputSizeInfo();
			Exit();
		}

		private static void Initialize()
		{
			s_Logs.Clear();
			s_Errors.Clear();
			s_BundleSizes.Clear();
		}

		private static void CopyBuiltBundlesToStreamingAssetsDirectory(string builtBundleDirectory, AssetBundleUtil.Platform platform)
		{
			Log($"Build configs for {platform} and path {builtBundleDirectory}");
			foreach (AssetBundlesConfig.BuildConfig buildConfig in AssetBundlesConfig.GetPlatformConfigs(platform))
			{
				Log($"Building config {buildConfig.BuildName} ({(buildConfig.AllBundles ? "all" : buildConfig.BundleCount.ToString())} bundles)");
				string buildName = AssetBundleUtil.GetManifestBundleName(platform, buildConfig.BuildName);
				string builtManifestFilePath = GetBuiltBundleFilePath(AssetBundleUtil.LocalBundlePath, platform, buildName, buildName);
				AssetBundle manifestBundle = AssetBundle.LoadFromFile(builtManifestFilePath);
				AssetBundleManifest manifest = manifestBundle.LoadAsset<AssetBundleManifest>(AssetBundleUtil.MANIFEST_ASSET_NAME);

				CopyBuiltBundleFileToStreamingAssets(builtBundleDirectory, platform, buildName, buildName);
				string[] manifestBundleNames = manifest.GetAllAssetBundles();
				HashSet<string> copied = new HashSet<string>();
				Dictionary<string, string> allDependencies = new Dictionary<string, string>();
				foreach (string bundleName in manifestBundleNames)
				{
					if (!buildConfig.Contains(bundleName))
					{
						continue;
					}
					CopyBuiltBundleFileToStreamingAssets(builtBundleDirectory, platform, buildName, bundleName);
					copied.Add(bundleName);
					string[] dependencies = manifest.GetAllDependencies(bundleName);
					foreach (string dependency in dependencies)
					{
						if (!allDependencies.ContainsKey(dependency))
						{
							allDependencies.Add(dependency, bundleName);
						}
					}
				}
				foreach (string bundleName in buildConfig.BundleNames)
				{
					if (!copied.Contains(bundleName))
					{
						EnqueueError("Building config {0} ({1} bundles) couldn't find bundle to copy named '{2}' in manifest {3}", 
							buildConfig.BuildName,
							buildConfig.AllBundles ? "all" : buildConfig.BundleCount.ToString(),
							bundleName, 
							buildName);
					}
				}
				foreach (KeyValuePair<string, string> dependency in allDependencies)
				{
					if (!copied.Contains(dependency.Key))
					{
						EnqueueError($"{dependency.Value} depends on {dependency.Key} which is not included in streaming assets, all dependencies must also be included");
					}
				}
				manifestBundle.Unload(true);
			}
		}

		private static void CopyBuiltBundleFileToStreamingAssets(string builtBundleDirectory, AssetBundleUtil.Platform platform, string manager, string bundle)
		{
			string builtBundleFilePath = GetBuiltBundleFilePath(builtBundleDirectory, platform, manager, bundle);
			if (!File.Exists(builtBundleFilePath))
			{
				EnqueueError("Could not find built AssetBundle at path: {0}", builtBundleFilePath);
				return;
			}
			string builtManifestFilePath = GetBuiltManifestFilePath(builtBundleDirectory, platform, manager, bundle);
			if (!File.Exists(builtManifestFilePath))
			{
				EnqueueError("Could not find build AssetBundleManifest at path: {0}", builtManifestFilePath);
				return;
			}
			string streamingBundleFilePath = GetStreamingBundleFilePathForPlatform(platform, manager, bundle);
			string streamingManifestFilePath = GetStreamingManifestFilePathForPlatform(platform, manager, bundle);
			Log("Copying built AssetBundle {0}\n    from {1}\n    to {2}", bundle, builtBundleFilePath, streamingBundleFilePath);
			string destinationDirectory = Path.GetDirectoryName(streamingBundleFilePath);
			try
			{
				if (!Directory.Exists(destinationDirectory))
				{
					Log("Creating directory {0}", destinationDirectory);
					Directory.CreateDirectory(destinationDirectory);
				}
			}
			catch (Exception exception)
			{
				EnqueueError("Caught exception while creating directory '{0}': {1}", destinationDirectory, exception);
			}
			try
			{
				File.Copy(builtBundleFilePath, streamingBundleFilePath, true);
			}
			catch (Exception exception)
			{
				EnqueueError("Caught exception while copying bundle '{0}' for platform '{1}': {2}", bundle, platform, exception);
			}
			try
			{
				File.Copy(builtManifestFilePath, streamingManifestFilePath, true);
			}
			catch (Exception exception)
			{
				EnqueueError("Caught exception while copying manifest '{0}' for platform '{1}': {2}", bundle, platform, exception);
			}
			try
			{
				FileInfo info = new FileInfo(builtBundleFilePath);
				s_BundleSizes[bundle] = info.Length;
			}
			catch (Exception exception)
			{
				EnqueueError("Caught exception while calculating bundle size {0}: {1}", bundle, exception);
			}
		}

		private static void DeleteStreamingAssetDirectories()
		{
			foreach (string platform in AssetBundleUtil.AllPlatformNames)
			{
				string fullPath = GetStreamingAssetDirectoryPathForPlatform(platform);
				Log("Deleting StreamingAssets at path {0}", fullPath);
				try
				{
					if (Directory.Exists(fullPath))
					{
						Directory.Delete(fullPath, true);
					}
				}
				catch (Exception exception)
				{
					EnqueueError("Caught exception while trying to delete StreamingAssets directory at path '{0}': {1}", fullPath, exception);
				}
			}
		}

		private static void OutputSizeInfo()
		{
			if (s_BundleSizes == null)
			{
				EnqueueError("s_BundleSizes is null!");
				return;
			}
			if (!AssetBundlesConfig.TryGetSizeLimit(out AssetBundlesConfig.PlatformBuildSize platformSize))
			{
				Log("Streaming bundles not configured"); // This shouldn't really happen
				return;
			}

			long total = 0;
			foreach (KeyValuePair<string, long> kvp in s_BundleSizes)
			{
				total += kvp.Value;
				Str.AddLine(kvp.Key, " bundle size: ", UIUtil.BytesToString((ulong)kvp.Value));
			}
			Log($"\nTotal copied bundle size: {UIUtil.BytesToString((ulong)total)}/{platformSize.MaxSizeMB}MB\n{Str.Finish()}");
			long max = platformSize.MaxSizeBytes;
			if (total > max)
			{
				EnqueueError("Total bundle size {0}({1}) exceeded max bundle build size {2}MB({3})", 
					UIUtil.BytesToString((ulong)total),
					total,
					platformSize.MaxSizeMB,
					max);
			}
			Log("\n Bundles with size limits:");
			foreach (AssetBundlesConfig.BundleBuildSize bundleMaxSize in platformSize.MaxBundleSizes)
			{
				long bundleMax = bundleMaxSize.MaxSizeBytes;
				long totalBundlesSize = 0L;
				foreach (string bundleName in bundleMaxSize.BundleNames)
				{
					if (!s_BundleSizes.TryGetValue(bundleName, out long bundleSize))
					{
						EnqueueError($"Bundle '{bundleMaxSize.Name}' size ({UIUtil.BytesToString((ulong)bundleSize)}) not found");
						continue;
					}
					totalBundlesSize += bundleSize;
				}
				Log($"{bundleMaxSize.Name} {UIUtil.BytesToMB((ulong)totalBundlesSize)}MB/{bundleMaxSize.MaxSizeMB}MB");
				if (totalBundlesSize > bundleMax)
				{
					EnqueueError($"Bundle {bundleMaxSize.Name} size {UIUtil.BytesToString((ulong)totalBundlesSize)}({totalBundlesSize}) " +
						$"exceeded max bundle size {bundleMaxSize.MaxSizeMB}MB({bundleMax}) {bundleMaxSize.GetDebugString()}");
				}
			}
		}

		private static string GetBuiltBundleFilePath(string builtBundleDirectory, AssetBundleUtil.Platform platform, string manager, string bundle)
		{
			string path = string.Format("{0}/{1}/{2}/{3}", builtBundleDirectory, platform, manager, bundle);
			return Path.GetFullPath(path);
		}

		private static string GetBuiltManifestFilePath(string builtBundleDirectory, AssetBundleUtil.Platform platform, string manager, string bundle)
		{
			return string.Format("{0}.manifest", GetBuiltBundleFilePath(builtBundleDirectory, platform, manager, bundle));
		}

		private static string GetStreamingBundleFilePathForPlatform(AssetBundleUtil.Platform platform, string manager, string bundle)
		{
			return Path.GetFullPath(GetStreamingAssetDirectoryPathForPlatform(platform.ToString()) + "/" + manager + "/" + bundle);
		}

		private static string GetStreamingManifestFilePathForPlatform(AssetBundleUtil.Platform platform, string manager, string bundle)
		{
			return Path.GetFullPath(GetStreamingBundleFilePathForPlatform(platform, manager, bundle) + ".manifest");
		}

		private static string GetStreamingAssetDirectoryPathForPlatform(string platform)
		{
			return Path.GetFullPath(GetStreamingAssetsDirectoryPath() + platform);
		}

		private static string GetStreamingAssetsDirectoryPath()
		{
			return Path.GetFullPath(Application.streamingAssetsPath + "/AssetBundles/");
		}

		private static void Log(string message, params object[] parameters)
		{
			string formattedMessage = string.Format(message, parameters);
			s_Logs.Add(string.Format("{0}", formattedMessage));
		}

		private static void EnqueueError(string error, params object[] parameters)
		{
			string formattedError = string.Format(error, parameters);
			s_Errors.Add(string.Format("[ERROR] {0}", formattedError));
		}

		private static string GetArg(string name)
		{
			string[] args = Environment.GetCommandLineArgs();
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] == name && args.Length > i + 1)
				{
					return args[i + 1];
				}
			}
			return null;
		}

		private static bool GetBuildResult(out string error)
		{
			Core.Str.Add("[StreamingAssetBundleBuild] Build output:");
			foreach (string s in s_Logs)
			{
				Core.Str.AddNewLine(s);
			}
			Debug.Log(Core.Str.Finish());
			if (s_Errors.Count == 0)
			{
				Debug.Log("[StreamingAssetBundleBuild] Finished with 0 errors.");
				error = null;
				return true;
			}
			else
			{
				Core.Str.Add("[StreamingAssetBundleBuild] Finished with ", s_Errors.Count.ToString(), " error(s).");
				foreach (string s in s_Errors)
				{
					Core.Str.AddNewLine(s);
				}
				error = Core.Str.Finish();
				Debug.LogError(error);
				return false;
			}
		}

		private static void Exit()
		{
			bool result = GetBuildResult(out _);
			EditorApplication.Exit(result ? 0 : 1);
		}
	}
}
