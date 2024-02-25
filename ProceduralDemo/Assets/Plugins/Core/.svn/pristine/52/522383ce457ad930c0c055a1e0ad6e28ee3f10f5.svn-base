
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Static Data Config")]
public class StaticDataConfig : ScriptableObject
{
#if UNITY_EDITOR
	private static StaticDataConfig s_Singleton = null;
#endif

	public static bool TryGet(out StaticDataConfig config)
	{
#if UNITY_EDITOR
		if (s_Singleton != null)
		{
			config = s_Singleton;
			return true;
		}
		string[] guids = UnityEditor.AssetDatabase.FindAssets("t:StaticDataConfig");
		if (guids.Length == 0)
		{
			config = null;
			return false;
		}
		string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
		config = s_Singleton = UnityEditor.AssetDatabase.LoadAssetAtPath<StaticDataConfig>(path);
		return config != null;
#else
		config = null;
		return false;
#endif
	}

	public enum RootFolder
	{
		Source,
		Client,
		Server
	}

	[SerializeField, HideInInspector]
	private string[] m_ServerDataSheetNames = { };
	public string[] ServerDataSheetNames => m_ServerDataSheetNames;

	/// <summary>
	/// Find all variants and build a dictionary
	/// </summary>
	public Dictionary<string, DataVariant> BuildVariantsDictionary(bool includeBase = false)
	{
		Dictionary<string, DataVariant> dataVariants = new Dictionary<string, DataVariant>();
		if (includeBase)
		{
			dataVariants.Add(DataVariant.Base.Name, DataVariant.Base);
		}
		string[] variantPath;
		string variantName;
		string variantParent;
		foreach (string variantFolder in GetAllVariantPaths())
		{
			variantPath = variantFolder.Split('\\', '/');
			variantName = variantPath[variantPath.Length - 1];
			if (dataVariants.ContainsKey(variantName))
			{
				Core.DebugUtil.DevException($"[ImportData] BuildVariantsDictionary() The variant '{variantName}' is trying to be added twice. Variants can not share names. Please ensure no variant folders at the path '{GetRootPath(RootFolder.Source)}' have matching names.");
				continue;
			}
			variantParent = variantPath[variantPath.Length - 2];
			if (variantParent == RootFolder.Source.ToString())
			{
				variantParent = DataVariant.Base.Name;
			}
			dataVariants.Add(variantName, new DataVariant(variantName, variantParent));
		}
		return dataVariants;
	}

	/// <summary>
	/// Find a specific variant and build a dictionary with it and it's parent variants
	/// </summary>
	public Dictionary<string, DataVariant> BuildVariantsDictionary(string variant)
	{
		Dictionary<string, DataVariant> dataVariants = new Dictionary<string, DataVariant>();
		foreach (string variantFolder in GetAllVariantPaths())
		{
			if (!variantFolder.EndsWith(variant))
			{
				continue;
			}
			string rootPath = GetRootPath(RootFolder.Source);
			string[] variantPath = variantFolder.Remove(0, rootPath.Length + 1).Split('\\', '/');
			for (int i = 0; i < variantPath.Length; i++)
			{
				string variantParent = i < 1 ? DataVariant.Base.Name : variantPath[i - 1];
				dataVariants.Add(variantPath[i], new DataVariant(variantPath[i], variantParent));
			}
			break;
		}
		return dataVariants;
	}

	public IEnumerable<string> GetAllVariantPaths()
	{
		string rootPath = GetRootPath(RootFolder.Source);
		string[] directories = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories);
		foreach (string variantFolder in directories)
		{
			if (variantFolder.Contains("/_") || variantFolder.Contains("\\_"))
			{
				continue; // Skip if a folder in path starts with an '_'
			}
			yield return variantFolder;
		}
	}

	public const string DATA_BUNDLE_PATH = "data2/";
	public const string VARIANTS_FOLDER = "Variants"; // Move Variants into seperate folder so we can SVN ignore all those files using that folder

	public static string GetVariantAssetBundleName(DataVariant variant) => $"{DATA_BUNDLE_PATH}{variant.ToString().ToLower()}";
	public static string GetVariantAssetBundleName(string variant) => $"{DATA_BUNDLE_PATH}{variant.ToString().ToLower()}";

	public static string GetVariantDBMasterName(DataVariant variant) => GetVariantDBMasterName(variant.Name);
	public static string GetVariantDBMasterName(string variant) => $"_{variant}DBMaster";
	public string GetClientAssetPath(DataVariant set, System.Type assetType) =>
		$"{GetPath(RootFolder.Client, set)}/{assetType}.asset";
	public string GetMasterAssetPath(DataVariant set) =>
		$"{GetPath(RootFolder.Client, set)}/{GetMasterAssetName(set)}";
	public string GetMasterAssetName(DataVariant set) =>
		$"{GetVariantDBMasterName(set)}.asset";
	public string GetMasterAssetName(string set) =>
		$"{GetVariantDBMasterName(set)}.asset";
	public string GetServerAssetPath(DataVariant set, string serverSheetName) =>
		$"{GetPath(RootFolder.Server, set)}/{serverSheetName}.json";
	public string GetVariantsPath(RootFolder root)
		=> $"{GetRootPath(root)}/{VARIANTS_FOLDER}";
	public string GetPath(RootFolder root, DataVariant set)
		=> $"{GetRootPath(root)}/{GetShortVariantsPath(set)}";

	public string GetRootPath(RootFolder root)
	{
#if UNITY_EDITOR
		string fullPath = UnityEditor.AssetDatabase.GetAssetPath(this);
		string rootPath = System.IO.Path.GetDirectoryName(fullPath);
		string path = $"{rootPath}/{root}";
		System.IO.Directory.CreateDirectory(path);
		return path;
#else
		return string.Empty;
#endif
	}

	public static string GetShortVariantsPath(DataVariant set)
	{
		if (set == DataVariant.Base)
		{
			return set.ToString();
		}
		return $"{VARIANTS_FOLDER}/{set}";
	}

	public string GetDataVariantPath(DataVariant set, in Dictionary<string, DataVariant> variants)
	{
		string path = set.ToString(); // Build path using variant parents
		string rootPath = GetRootPath(RootFolder.Source);
		while (set.Parent != DataVariant.Base.Name)
		{
			if (!variants.TryGetValue(set.Parent, out set))
			{
				Debug.LogError($"[{nameof(StaticDataConfig)}] GetDataVariantPath() Data variant path {path} could not complete because varient.parent does not exist.");
				return rootPath;
			}
			path = $"{set}/{path}";
		}
		return $"{rootPath}/{path}";
	}
}

public class DataVariant
{
	public static readonly DataVariant Base = new DataVariant("Base", null);

	private string m_Name = string.Empty;
	public string Name => m_Name;
	private string m_Parent = string.Empty;
	public string Parent => m_Parent;

	public DataVariant(string name, string parent)
	{
		m_Name = name;
		m_Parent = parent;
	}

	public override string ToString() => Name;
}
