using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	[CreateAssetMenu(fileName = ASSET_NAME, menuName = "Scriptable Objects/Asset Bundles Config")]
	public class AssetBundlesConfig : ScriptableObject
	{
		public const string ASSET_NAME = "AssetBundlesConfig";

		private static AssetBundlesConfig s_Singleton = null;

		private void Awake()
		{
			Initialize();
		}

		private void OnValidate()
		{
			Initialize();
			//foreach (PlatformBuildSize bs in m_SizeLimits)
			//{
			//	foreach (BundleBuildSize bndls in bs.MaxBundleSizes)
			//	{
			//		Debug.Log(bndls.GetDebugString());
			//	}
			//}
		}

		public static bool TryGet(out AssetBundlesConfig config)
		{
			if (s_Singleton == null)
			{
				s_Singleton = Resources.Load<AssetBundlesConfig>(ASSET_NAME);
			}
			config = s_Singleton;
			return config != null;
		}

		public static bool HasConfigs(AssetBundleUtil.Platform platform, string buildName)
		{
			if (TryGet(out AssetBundlesConfig config))
			{
				foreach (PlatformConfig platformConfig in config.m_Platforms)
				{
					if (platformConfig.IsAvailable(platform) && platformConfig.TryGet(buildName, out _))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool TryGetSizeLimit(out PlatformBuildSize size)
		{
			if (TryGet(out AssetBundlesConfig config))
			{
				foreach (PlatformBuildSize platformSize in config.m_SizeLimits)
				{
					if (platformSize.IsAvailable())
					{
						size = platformSize;
						return true;
					}
				}
			}
			size = null;
			return false;
		}

		public static bool TryGetIsPersistent(string assetBundleName, out ABM.PersistentType persistentType)
		{
			persistentType = ABM.PersistentType.NotPersistent;
			if (!TryGet(out AssetBundlesConfig config))
			{
				return false;
			}
			foreach (BundlePersistent persistentBundle in config.m_PersistentBundles)
			{
				if (assetBundleName.StartsWith(persistentBundle.Name))
				{
					persistentType = persistentBundle.PersistentType;
					return true;
				}
			}
			return false;
		}

		public static bool TryGetBundleDependancyValidation(string assetBundleName, out BundleDependancyValidation dependancyValidation)
		{
			dependancyValidation = null;
			if (!TryGet(out AssetBundlesConfig config))
			{
				return false;
			}
			foreach (BundleDependancyValidation validation in config.m_DependancyValidations)
			{
				if (!assetBundleName.StartsWith(validation.Name))
				{
					continue;
				}
				if (!validation.LimitDependancies && !validation.LimitDependants)
				{
					return false; // Early out
				}
				dependancyValidation = validation;
				return true;
			}
			return false;
		}

		public static IEnumerable<BuildConfig> GetPlatformConfigs(AssetBundleUtil.Platform platform)
		{
			if (!TryGet(out AssetBundlesConfig config))
			{
				yield break;
			}
			foreach (PlatformConfig platformConfig in config.m_Platforms)
			{
				if (!platformConfig.IsAvailable(platform))
				{
					continue;
				}
				foreach (BuildConfig buildConfig in platformConfig.Configs)
				{
					yield return buildConfig;
				}
			}
		}

		public static IEnumerable<BuildConfig> GetBuildConfigs(AssetBundleUtil.Platform platform, string buildName)
		{
			if (!TryGet(out AssetBundlesConfig config))
			{
				yield break;
			}
			foreach (PlatformConfig platformConfig in config.m_Platforms)
			{
				if (platformConfig.IsAvailable(platform) && platformConfig.TryGet(buildName, out BuildConfig buildConfig))
				{
					yield return buildConfig;
				}
			}
		}

		public static bool ContainsBundle(AssetBundleUtil.Platform platform, string buildName, string bundleName)
		{
			foreach (BuildConfig config in GetBuildConfigs(platform, buildName))
			{
				if (config.Contains(bundleName))
				{
					return true;
				}
			}
			return false;
		}

		[SerializeField]
		private List<PlatformConfig> m_Platforms = new List<PlatformConfig>();
		[SerializeField]
		private List<PlatformBuildSize> m_SizeLimits = new List<PlatformBuildSize>();
		[SerializeField]
		private List<BundlePersistent> m_PersistentBundles = new List<BundlePersistent>();
		[SerializeField]
		private List<BundleDependancyValidation> m_DependancyValidations = new List<BundleDependancyValidation>();

		public void Initialize()
		{
			foreach (PlatformConfig platform in m_Platforms)
			{
				platform.Initialize();
			}
		}

		[System.Serializable]
		public class PlatformBuildSize
		{
			[SerializeField, EnumMask]
			private AssetBundleUtil.Platform m_Platform = AssetBundleUtil.Platform.Windows;

			[SerializeField]
			private float m_MaxBuildSizeMB = 30.0f;
			public float MaxSizeMB => m_MaxBuildSizeMB;
			// Logs round MB to the nearest tenth ie. 10.1416...MB is displayed as 10.1MB
			// Adding 0.05MB means we can type in the number displayed in the logs and things will always work out
			public long MaxSizeBytes => UIUtil.MbToBytes(m_MaxBuildSizeMB) + UIUtil.MbToBytes(0.05f) + 1; // Round up

			[SerializeField]
			private BundleBuildSize[] m_MaxBundleSizes = { };

			public IEnumerable<BundleBuildSize> MaxBundleSizes => m_MaxBundleSizes;

			public bool IsAvailable() { return m_Platform.HasFlag(AssetBundleUtil.GetPlatform()); }
		}

		[System.Serializable]
		public class BundleBuildSize
		{
			[SerializeField]
			private string[] m_BundleNames = new string[1];
			public string[] BundleNames => m_BundleNames;
			public string Name
			{
				get
				{
					if (m_BundleNames.Length <= 1)
					{
						return m_BundleNames[0];
					}
					return Core.Str.BuildWithBetweens(", ", m_BundleNames);
				}
			}
			[SerializeField]
			private float m_MaxSizeMB = 1.0f;
			public float MaxSizeMB => m_MaxSizeMB;
			// Logs round MB to the nearest tenth ie. 10.1416...MB is displayed as 10.1MB
			// Adding 0.05MB means we can type in the number displayed in the logs and things will always work out
			public long MaxSizeBytes => UIUtil.MbToBytes(m_MaxSizeMB) + UIUtil.MbToBytes(0.05f) + 1; // Round up

			public string GetDebugString() => $"[{m_MaxSizeMB}({UIUtil.MbToBytes(m_MaxSizeMB)}) + 0.05({UIUtil.MbToBytes(0.05f)}) + 1 = {UIUtil.BytesToString((ulong)MaxSizeBytes)}({MaxSizeBytes})]";
		}

		[System.Serializable]
		public class BundlePersistent
		{
			[SerializeField]
			private string m_BundleName = string.Empty;
			public string Name => m_BundleName;
			[SerializeField, Core.EnumExcludeLast]
			private ABM.PersistentType m_PersistentType = ABM.PersistentType.NeverUnload;
			public ABM.PersistentType PersistentType => m_PersistentType;
			[SerializeField]
			private InspectorNotes m_Note = new InspectorNotes();
		}

		[System.Serializable]
		public class BundleDependancyValidation
		{
			[SerializeField]
			private string m_BundleName = string.Empty;
			public string Name => m_BundleName;

			[Space, SerializeField]
			private bool m_LimitDependancies = false;
			public bool LimitDependancies => m_LimitDependancies;
			[SerializeField]
			private string[] m_AllowedDependancies = new string[0];
			public string[] AllowedDependancies => m_AllowedDependancies;
			[SerializeField]
			private bool m_LimitDependants = false;
			public bool LimitDependants => m_LimitDependants;
			[SerializeField]
			private string[] m_AllowedDependants = new string[0];
			public string[] AllowedDependants => m_AllowedDependants;
			
			[SerializeField]
			private InspectorNotes m_Note = new InspectorNotes();
		}

		[System.Serializable]
		public class PlatformConfig
		{
			[SerializeField, EnumMask]
			private AssetBundleUtil.Platform m_Platform = AssetBundleUtil.Platform.Windows;

			[SerializeField]
			private List<BuildConfig> m_Configs = new List<BuildConfig>();
			public IEnumerable<BuildConfig> Configs => m_Configs;

			public bool IsAvailable(AssetBundleUtil.Platform platform) { return m_Platform.HasFlag(platform); }

			private Dictionary<string, BuildConfig> m_Dict = new Dictionary<string, BuildConfig>();
			public bool TryGet(string buildName, out BuildConfig config) { return m_Dict.TryGetValue(buildName, out config); }

			public void Initialize()
			{
				m_Dict.Clear();
				foreach (BuildConfig config in m_Configs)
				{
					config.Initialize();
					m_Dict.Add(config.BuildName, config);
				}
			}
		}

		[System.Serializable]
		public class BuildConfig
		{
			[SerializeField]
			private string m_BuildName = string.Empty;
			public string BuildName => m_BuildName;

			[SerializeField]
			private List<string> m_StreamingBundles = new List<string>();
			public IEnumerable<string> BundleNames => m_StreamingBundles;
			public bool AllBundles => m_StreamingBundles.Count == 0; // Note: Empty set means we include all bundles
			public int BundleCount => m_StreamingBundles.Count;

			private HashSet<string> m_Set = new HashSet<string>();
			public bool Contains(string bundle) { return m_Set.Count == 0 ? true : m_Set.Contains(bundle); }

			public void Initialize()
			{
				m_Set.Clear();
				foreach (string bundle in m_StreamingBundles)
				{
					m_Set.Add(bundle);
				}
			}
		}
	}
}