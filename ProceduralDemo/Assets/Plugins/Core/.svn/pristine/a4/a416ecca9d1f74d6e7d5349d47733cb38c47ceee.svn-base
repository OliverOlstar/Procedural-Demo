using System.Collections.Generic;
using UnityEngine;

public class AssetPickerPathSource : IAssetPickerPathSource
{
	private string m_Title;
	private System.Type[] m_TypesToLoad;
	private string m_PathPrefix;
	private readonly bool m_CanBeNested;

	public AssetPickerPathSource(System.Type[] types, string limitPathsByPrefix = null, bool canBeNested = true)
	{
		List<System.Type> actualTypes = Core.ListPool<System.Type>.Request();
		string[] typesNames = new string[types.Length];
		for (int i = 0; i < types.Length; i++)
		{
			System.Type type = Core.PropertyDrawerUtil.GetUnderlyingType(types[i]);
			typesNames[i] = type.Name;

			bool isComponent = typeof(Component).IsAssignableFrom(type);
			if (isComponent)
			{
				type = typeof(GameObject);
			}

			actualTypes.AddUniqueItem(type);
		}

		m_Title = Core.Str.BuildWithBetweens(" & ", typesNames);
		m_TypesToLoad = actualTypes.ToArray();
		m_CanBeNested = canBeNested;
		m_PathPrefix = limitPathsByPrefix;

		Core.ListPool<System.Type>.Return(actualTypes);
	}

	string IAssetPickerPathSource.GetSearchWindowTitle() => m_Title;
	char[] IAssetPickerPathSource.GetPathSperators() => new char[] { '/', '\\' };

	List<string> IAssetPickerPathSource.GetPaths()
	{
		List<string> paths = new List<string>();
		foreach (System.Type type in m_TypesToLoad)
		{
			paths.AddRange(Core.AssetDatabaseUtil.Find(type, m_CanBeNested));
		}
		if (!string.IsNullOrEmpty(m_PathPrefix))
		{
			for (int i = paths.Count - 1; i > 0; i--)
			{
				if (!paths[i].StartsWith(m_PathPrefix))
				{
					paths.RemoveAt(i);
				}
			}
		}
		return paths;
	}

	bool IAssetPickerPathSource.TryGetUnityObjectType(out System.Type type) { type = m_TypesToLoad[0]; return true; }
}
