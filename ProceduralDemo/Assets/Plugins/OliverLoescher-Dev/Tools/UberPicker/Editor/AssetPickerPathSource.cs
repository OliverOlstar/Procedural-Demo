using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Pool;
using ODev.Util;

public class AssetPickerPathSource : IAssetPickerPathSource
{
	private readonly string m_Title;
	private readonly Type[] m_TypesToLoad;
	private readonly string m_PathPrefix;
	private readonly bool m_CanBeNested;

	/// <summary>Utility function for property drawers to get the field type and clean it up in the case the drawer is applied to a list or array</summary>
	public static Type GetUnderlyingType(FieldInfo fieldInfo) => GetUnderlyingType(fieldInfo.FieldType);

	/// <summary>Utility function for property drawers to get the field type and clean it up in the case the drawer is applied to a list or array</summary>
	public static Type GetUnderlyingType(Type fieldType)
	{
		if (fieldType.IsArray) // Attribute could be attached to an Array element
		{
			fieldType = fieldType.GetElementType();
		}
		else if (fieldType.IsGenericType) // Attribute could be attached to a List element
		{
			fieldType = fieldType.GetGenericArguments()[0];
		}
		return fieldType;
	}

	public AssetPickerPathSource(Type[] types, string limitPathsByPrefix = null, bool canBeNested = true)
	{
		List<Type> actualTypes = ListPool<Type>.Get();
		string[] typesNames = new string[types.Length];
		for (int i = 0; i < types.Length; i++)
		{
			Type type = GetUnderlyingType(types[i]);
			typesNames[i] = type.Name;

			if (typeof(Component).IsAssignableFrom(type))
			{
				type = typeof(GameObject);
			}

			actualTypes.AddUniqueItem(type);
		}

		m_Title = ODev.Util.Debug.BuildWithBetweens(" & ", typesNames);
		m_TypesToLoad = actualTypes.ToArray();
		m_CanBeNested = canBeNested;
		m_PathPrefix = limitPathsByPrefix;

		ListPool<Type>.Release(actualTypes);
	}

	string IAssetPickerPathSource.GetSearchWindowTitle() => m_Title;
	char[] IAssetPickerPathSource.GetPathSperators() => new char[] { '/', '\\' };

	List<string> IAssetPickerPathSource.GetPaths()
	{
		List<string> paths = new();
		foreach (Type type in m_TypesToLoad)
		{
			paths.AddRange(AssetDatabase.Find(type, m_CanBeNested));
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

	bool IAssetPickerPathSource.TryGetUnityObjectType(out Type type) { type = m_TypesToLoad[0]; return true; }
}
