
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Core
{
	public static class AssetBundleBuildPipeline
	{
		private static List<string> s_Errors = new List<string>();

		public static string CreateAssetBundleDirectory(string buildName)
		{
			// Choose the output path according to the build target.
			string outputPath = Core.AssetBundleUtil.GetAssetBundleBuildPath(buildName);
			if (!Directory.Exists(outputPath))
			{
				Directory.CreateDirectory(outputPath);
			}
			return outputPath;
		}

		public static List<AssetBundleBuild> BuildAssetList(out HashSet<string> assetList, System.Func<string, bool> includeBundle = null)
		{
			assetList = new HashSet<string>();
			string[] names = AssetDatabase.GetAllAssetBundleNames();
			List<AssetBundleBuild> buildList = new List<AssetBundleBuild>(names.Length);
			foreach (string name in names)
			{
				if (includeBundle != null && !includeBundle.Invoke(name))
				{
					continue;
				}
				AssetBundleBuild bundleBuild = new AssetBundleBuild();
				bundleBuild.assetBundleName = name;
				bundleBuild.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(name);
				foreach (string asset in bundleBuild.assetNames)
				{
					assetList.Add(asset);
				}
				buildList.Add(bundleBuild);
			}
			return buildList;
		}

		public static Core.AssetBundleCatalogue BuildBundleCatalogue(List<AssetBundleBuild> buildList, out string debugOutput)
		{
			Core.AssetBundleCatalogue catalogue = new Core.AssetBundleCatalogue();
			foreach (AssetBundleBuild build in buildList)
			{
				List<string> sortAssetNames = new List<string>(build.assetNames);
				sortAssetNames.Sort();
				foreach (string assetPath in sortAssetNames)
				{
					string assetName = Path.GetFileNameWithoutExtension(assetPath);
					if (catalogue.Add(assetName, build.assetBundleName))
					{
						Core.Str.AddLine(assetName, " - ", build.assetBundleName, " (", assetPath, ")");
					}
					else
					{
						Core.Str.AddLine("[Duplicate] ", assetName, " - ", build.assetBundleName, " (", assetPath, ")");
					}
				}
			}
			debugOutput = Core.Str.Finish();
			return catalogue;
		}

		private static string SerializeBinaryObjectAndAddtoBuild(object obj, string name, List<AssetBundleBuild> buildList)
		{
			string fullPath = "Assets/" + name + ".bytes";
			IFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream(
				fullPath,
				FileMode.Create, FileAccess.Write, FileShare.None);
			formatter.Serialize(stream, obj);
			stream.Close();
			AssetDatabase.Refresh();
			AssetBundleBuild s = new AssetBundleBuild();
			s.assetBundleName = name;
			s.assetNames = new string[] { fullPath };
			buildList.Add(s);
			return fullPath;
		}

		public static Dictionary<string, ulong> BuildBundleSizes(AssetBundleManifest manifest, string outputPath, out string outStr)
		{
			// Calculate asset bundle sizes
			string[] bundleNames = manifest.GetAllAssetBundles();
			Dictionary<string, ulong> sizes = new Dictionary<string, ulong>(bundleNames.Length);
			ulong totalSize = 0;
			foreach (string bundleName in bundleNames)
			{
				string bundlePath = outputPath + "/" + bundleName;
				if (File.Exists(bundlePath))
				{
					ulong size = (ulong)new FileInfo(bundlePath).Length;
					sizes.Add(bundleName, size);
					totalSize += size;
					Core.Str.AddLine(bundleName, ": ", Core.UIUtil.BytesToString(size));
				}
				else
				{
					Debug.LogError("Can't find bundle: " + bundlePath);
				}
			}
			Core.Str.AddLine("total: ", Core.UIUtil.BytesToString(totalSize));
			outStr = Core.Str.Finish();
			return sizes;
		}

		public static void BuildAssetBundles(
			string buildName,
			System.Func<string, bool> includeBundle = null,
			System.Action<string, HashSet<string>, List<AssetBundleBuild>> customBuildStep = null,
			BuildAssetBundleOptions options = BuildAssetBundleOptions.None)
		{
			Debug.Log(Core.Str.Build("[", buildName, "] Start build"));
			s_Errors.Clear();

			string outputPath = CreateAssetBundleDirectory(buildName);
			Debug.Log(Core.Str.Build("[", buildName, "] Output path ", outputPath));

			List<AssetBundleBuild> buildList = BuildAssetList(out HashSet<string> assetList, includeBundle);
			if (customBuildStep != null)
			{
				customBuildStep.Invoke(buildName, assetList, buildList);
			}
			Core.Str.AddLine("[", buildName, "] Building ", buildList.Count.ToString(), " bundles");
			foreach (AssetBundleBuild build in buildList)
			{
				List<string> sortAssetNames = new List<string>(build.assetNames);
				sortAssetNames.Sort();
				foreach (string assetPath in sortAssetNames)
				{
					Core.Str.AddLine(build.assetBundleName, " - ", assetPath);
				}
			}
			Debug.Log(Core.Str.Finish());

			// Build asset bundle catalogue
			Core.AssetBundleCatalogue catalogue = BuildBundleCatalogue(buildList, out string catalogueOutput);
			Debug.Log(Core.Str.Build("[", buildName, "] Bundle catalogue:\n", catalogueOutput));
			string cataloguePath = SerializeBinaryObjectAndAddtoBuild(
				catalogue,
				Core.AssetBundleUtil.GetCatalogueAssetName(buildName),
				buildList);

			// First bundle build
			AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
				outputPath,
				buildList.ToArray(),
				BuildAssetBundleOptions.None,
				EditorUserBuildSettings.activeBuildTarget);
			if (manifest == null)
			{
				throw new System.ArgumentNullException("AssetBundleBuildPipeline.BuildAssetBundles() AssetBundleManifest is null, the bundle build failed for some reason. See the build log for details.");
			}

			// Calculate asset bundle sizes
			Dictionary<string, ulong> sizes = BuildBundleSizes(manifest, outputPath, out string debugOutput);
			Debug.Log(Core.Str.Build("[", buildName, "] Calculate bundles sizes:\n", debugOutput));
			string sizesPath = SerializeBinaryObjectAndAddtoBuild(
				sizes,
				Core.AssetBundleUtil.GetSizesAssetName(buildName),
				buildList);

			// Second bundle build with sizes included
			manifest = BuildPipeline.BuildAssetBundles(
				outputPath,
				buildList.ToArray(),
				BuildAssetBundleOptions.None,
				EditorUserBuildSettings.activeBuildTarget);

			// Validate bundles
			string[] bundles = manifest.GetAllAssetBundles();
			Core.Str.AddLine("[", buildName, "] Finished ", bundles.Length.ToString(), " bundles");
			foreach (string bundle in bundles)
			{
				bool persistent = AssetBundlesConfig.TryGetIsPersistent(bundle, out ABM.PersistentType type);
				Core.Str.AddLine(bundle, persistent ? $" ({System.Enum.GetName(typeof(ABM.PersistentType), type)})" : "", " dependencies: ");
				ValidateBundleDependancies(manifest, bundle, type);
				// Core.Str.Add("\n");
				// string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(bundle);
				// foreach (string asset in assets)
				// {
				// 	Core.Str.AddLine("    ", asset);
				// }
			}
			Debug.Log(Core.Str.Finish());

			// Remove the temporary files
			AssetDatabase.DeleteAsset(cataloguePath);
			AssetDatabase.DeleteAsset(sizesPath);
			AssetDatabase.Refresh();

			// Errors during bundle build
			if (s_Errors.Count > 0)
			{
				foreach (string error in s_Errors)
				{
					Str.AddNewLine(error);
				}
				throw new System.Exception($"The bundle build failed with errors:{Str.Finish()}");
			}
			s_Errors.Clear();
		}

		private static void ValidateBundleDependancies(AssetBundleManifest manifest, string bundle, ABM.PersistentType type)
		{
			string[] dependencies = manifest.GetAllDependencies(bundle);
			AssetBundlesConfig.TryGetBundleDependancyValidation(bundle, out AssetBundlesConfig.BundleDependancyValidation bundleValidation);
			foreach (string dependency in dependencies)
			{
				// Dependency Validation
				if (bundleValidation != null &&
					bundleValidation.LimitDependancies && !bundleValidation.AllowedDependancies.Contains(dependency))
				{
					Core.Str.AddLine("    ", dependency, " - ERROR dependancy is not allowed due to validation set for the bundle(s) '", bundleValidation.Name, "'");
					s_Errors.Add($"Asset bundle '{bundle}' can not have the dependancy of the bundle '{dependency}' due to validation set for the bundle(s) '{bundleValidation.Name}'" +
						$"\nTo fix this remove '{bundle}'s references to '{dependency}' to remove the depenancy or adjust the bundle validation configuration in the {nameof(AssetBundlesConfig)}.asset");
					continue;
				}
				if (AssetBundlesConfig.TryGetBundleDependancyValidation(dependency, out AssetBundlesConfig.BundleDependancyValidation dependancyValidation) &&
					dependancyValidation.LimitDependants && !dependancyValidation.AllowedDependants.Contains(bundle))
				{
					Core.Str.AddLine("    ", dependency, " - ERROR dependant is not allowed due to validation set for the bundle(s) '", dependancyValidation.Name, "'");
					s_Errors.Add($"Asset bundle '{bundle}' can not be a dependant for the bundle '{dependency}' due to validation set for the bundle(s) '{dependancyValidation.Name}'" +
						$"\nTo fix this remove '{bundle}'s references to '{dependency}' to remove the depenancy or adjust the bundle validation configuration in the {nameof(AssetBundlesConfig)}.asset");
					continue;
				}
				// Persistent Validation
				if (type != ABM.PersistentType.NotPersistent)
				{
					if (!AssetBundlesConfig.TryGetIsPersistent(dependency, out ABM.PersistentType depenancyType))
					{
						Core.Str.AddLine("    ", dependency, " - ERROR persistent bundle can not be dependant on a not persistent bundle");
						s_Errors.Add($"Asset bundle '{bundle}' is dependant on bundle '{dependency}'. A persistent bundle can not be dependant on a not persistent bundle." +
							$"\nTo fix this remove '{bundle}'s references to '{dependency}' to remove the depenancy or adjust the persistent configuration in the {nameof(AssetBundlesConfig)}.asset");
						continue;
					}
					if (ABM.IsLowerPersistantLevel(depenancyType, type))
					{
						string typeString = System.Enum.GetName(typeof(ABM.PersistentType), type);
						string dependancyTypeString = System.Enum.GetName(typeof(ABM.PersistentType), depenancyType);
						Core.Str.AddLine("    ", dependency, $" - ERROR A {typeString} persistent bundle can not be dependant on a {dependancyTypeString} persistent bundle");
						s_Errors.Add($"Asset bundle '{bundle}' is dependant on bundle '{dependency}'. A {typeString} persistent bundle can not be dependant on a {dependancyTypeString} persistent bundle." +
							$"\nTo fix this remove '{bundle}'s references to '{dependency}' to remove the depenancy or adjust the persistent configuration in the {nameof(AssetBundlesConfig)}.asset");
						continue;
					}
				}
				Core.Str.AddLine("    ", dependency);
			}
		}
	}
}
