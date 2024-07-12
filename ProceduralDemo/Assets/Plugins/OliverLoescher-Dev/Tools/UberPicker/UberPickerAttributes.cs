using UnityEngine;

namespace UberPicker
{
	/// <summary>Some objects are too complicated to be viewed nicely in a foldout inspector or should only be viewed through a custom editor window. 
	/// Use this attribute to disable the UberPickers foldout inspector.</summary>
	public class NoFoldoutInspectorAttribute : System.Attribute { }

	public class AssetAttribute : PropertyAttribute, IAssetPickerAttribute
	{
		private bool m_AllowNone;
		private string m_Path;
		public string PathPrefix => m_Path;
		private string m_OverrideButtonName;
		private readonly bool m_CanBeNested;

		bool IAssetPickerAttribute.AllowNull => m_AllowNone;
		bool IAssetPickerAttribute.ForceFlatten => false;
		string IAssetPickerAttribute.OverrideFirstName => m_OverrideButtonName;

		public bool CanBeNested => m_CanBeNested;
		
		/// <param name="allowNull"></param>
		/// <param name="path"></param>
		/// <param name="overrideButtonName"></param>
		/// <param name="canBeNested">Set to false to avoid loading assets, strongly recommended for assets with SerializeReference attribute on any of their fields which can cause slow import time in the editor</param>
		public AssetAttribute(bool allowNull = true, string path = null, string overrideButtonName = null, bool canBeNested = true)
		{
			m_AllowNone = allowNull;
			m_Path = path;
			m_OverrideButtonName = overrideButtonName;
			m_CanBeNested = canBeNested;
		}
	}

	public class AssetNonNullAttribute : AssetAttribute
	{
		public AssetNonNullAttribute(string path = null, string overrideButtonName = null) : base(false, path, overrideButtonName)
		{

		}
	}

	public class AssetNameAttribute : PropertyAttribute, IAssetPickerAttribute
	{
		private System.Type[] m_Types;
		private bool m_AllowNone;
		private string m_Path;

		public System.Type[] Types => m_Types;
		public string PathPrefix => m_Path;
		
		bool IAssetPickerAttribute.AllowNull => m_AllowNone;
		bool IAssetPickerAttribute.ForceFlatten => false;
		string IAssetPickerAttribute.OverrideFirstName => null;

		public AssetNameAttribute(System.Type assetType, bool allowEmpty = false, string path = null)
		{
			m_Types = new System.Type[] { assetType };
			m_AllowNone = allowEmpty;
			m_Path = path;
		}

		public AssetNameAttribute(bool allowEmpty = false, string path = null, params System.Type[] assetTypes)
		{
			m_Types = assetTypes;
			m_AllowNone = allowEmpty;
			m_Path = path;
		}
	}
}
