using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data.UberPicker
{
	public class DataIDAttribute : PropertyAttribute, IAssetPickerAttribute
	{
		private System.Type m_DBType;
		public System.Type DBType => m_DBType;
		private bool m_AllowNone;
		private bool m_Flatten;
		private string m_OverrideNoneString;

		string IAssetPickerAttribute.OverrideFirstName => m_OverrideNoneString;
		bool IAssetPickerAttribute.ForceFlatten => m_Flatten;
		bool IAssetPickerAttribute.AllowNull => m_AllowNone;

		public DataIDAttribute(System.Type assetType, bool allowEmpty = false, bool flatten = true, string overrideNoneString = null)
		{
			m_DBType = assetType;
			m_AllowNone = allowEmpty;
			m_Flatten = flatten;
			m_OverrideNoneString = overrideNoneString;
		}
	}

	public class DataVariantAttribute : PropertyAttribute, IAssetPickerAttribute
	{
		private bool m_AllowNone;
		private bool m_Flatten;
		private string m_OverrideNoneString;

		string IAssetPickerAttribute.OverrideFirstName => m_OverrideNoneString;
		bool IAssetPickerAttribute.ForceFlatten => m_Flatten;
		bool IAssetPickerAttribute.AllowNull => m_AllowNone;

		public DataVariantAttribute(bool allowEmpty = false, bool flatten = true, string overrideNoneString = null)
		{
			m_AllowNone = allowEmpty;
			m_Flatten = flatten;
			m_OverrideNoneString = overrideNoneString;
		}
	}
}