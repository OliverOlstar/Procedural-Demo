using UnityEngine;

namespace OCore.PlayerPrefs
{
	public class PlayerPrefsVector4 : PlayerPrefsValue<Vector4>
	{
		public PlayerPrefsVector4(string key, bool isGlobalPref = false)
			: base(key, Vector4.zero, isGlobalPref)
		{
		}

		public PlayerPrefsVector4(string key, Vector4 defaultValue, bool isGlobalPref = false)
			: base(key, defaultValue, isGlobalPref)
		{
		}

		protected override Vector4 Get(bool isGlobal)
		{
			Vector4 value = Vector4.zero;
			if (isGlobal)
			{
				value.x = PlayerPrefs.GetGlobalFloat($"{Key}_x", m_DefaultValue.x);
				value.y = PlayerPrefs.GetGlobalFloat($"{Key}_y", m_DefaultValue.y);
				value.z = PlayerPrefs.GetGlobalFloat($"{Key}_z", m_DefaultValue.z);
				value.w = PlayerPrefs.GetGlobalFloat($"{Key}_w", m_DefaultValue.w);
			}
			else
			{
				value.x = PlayerPrefs.GetFloat($"{Key}_x", m_DefaultValue.x);
				value.y = PlayerPrefs.GetFloat($"{Key}_y", m_DefaultValue.y);
				value.z = PlayerPrefs.GetFloat($"{Key}_z", m_DefaultValue.z);
				value.w = PlayerPrefs.GetFloat($"{Key}_w", m_DefaultValue.w);
			}
			return value;
		}

		protected override void Set(Vector4 value, bool isGlobal)
		{
			if (isGlobal)
			{
				PlayerPrefs.SetGlobalFloat($"{Key}_x", value.x);
				PlayerPrefs.SetGlobalFloat($"{Key}_y", value.y);
				PlayerPrefs.SetGlobalFloat($"{Key}_z", value.z);
				PlayerPrefs.SetGlobalFloat($"{Key}_w", value.w);
				return;
			}
			PlayerPrefs.SetFloat($"{Key}_x", value.x);
			PlayerPrefs.SetFloat($"{Key}_y", value.y);
			PlayerPrefs.SetFloat($"{Key}_z", value.z);
			PlayerPrefs.SetFloat($"{Key}_w", value.w);
		}
	}
}