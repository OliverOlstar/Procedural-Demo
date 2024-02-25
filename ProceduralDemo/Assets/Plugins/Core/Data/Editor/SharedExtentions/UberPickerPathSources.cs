using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Data;

public class DataIDPickerPathSource : IAssetPickerPathSource
{
	private System.Type m_DBType = null;

	public DataIDPickerPathSource(System.Type dbType)
	{
		m_DBType = dbType;
	}

	string IAssetPickerPathSource.GetSearchWindowTitle() => DBManager.TryGet(m_DBType, out DBBase dataBase) ? dataBase.SheetName : string.Empty;
	char[] IAssetPickerPathSource.GetPathSperators() => new char[] { '_' };
	List<string> IAssetPickerPathSource.GetPaths()
	{
		if (!DBManager.TryGet(m_DBType, out DBBase dataBase))
		{
			return new List<string>();
		}
		return new List<string>(dataBase.GetEditorPickerIDs());
	}
	bool IAssetPickerPathSource.TryGetUnityObjectType(out System.Type type) { type = null; return false; }
}

public class DataVariantPickerPathSource : IAssetPickerPathSource
{
	string IAssetPickerPathSource.GetSearchWindowTitle() => "Data Variants";
	char[] IAssetPickerPathSource.GetPathSperators() => new char[] { '/', '\\' };
	List<string> IAssetPickerPathSource.GetPaths()
	{
		if (!StaticDataConfig.TryGet(out StaticDataConfig dataConfig))
		{
			return new List<string>();
		}
		List<string> paths = new List<string>();
		paths.Add(DataVariant.Base.Name);
		paths.AddRange(dataConfig.GetAllVariantPaths());
		return paths;
	}
	bool IAssetPickerPathSource.TryGetUnityObjectType(out System.Type type) { type = null; return false; }
}
