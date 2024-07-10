using UnityEngine;

namespace OCore.PlayerPrefs
{
	public class PlayerPrefsVector3 : PlayerPrefsValue<Vector3>
	{
		public PlayerPrefsVector3(string key, bool isGlobalPref = false)
			: base(key, Vector3.zero, isGlobalPref)
		{
		}

		public PlayerPrefsVector3(string key, Vector3 defaultValue, bool isGlobalPref = false)
			: base(key, defaultValue, isGlobalPref)
		{
		}

		protected override Vector3 Get(bool isGlobal)
		{
			Vector3 value = Vector3.zero;
			if (isGlobal)
			{
				value.x = PlayerPrefs.GetGlobalFloat($"{Key}_x", m_DefaultValue.x);
				value.y = PlayerPrefs.GetGlobalFloat($"{Key}_y", m_DefaultValue.y);
				value.z = PlayerPrefs.GetGlobalFloat($"{Key}_z", m_DefaultValue.z);
			}
			else
			{
				value.x = PlayerPrefs.GetFloat($"{Key}_x", m_DefaultValue.x);
				value.y = PlayerPrefs.GetFloat($"{Key}_y", m_DefaultValue.y);
				value.z = PlayerPrefs.GetFloat($"{Key}_z", m_DefaultValue.z);
			}
			return value;
		}

		protected override void Set(Vector3 value, bool isGlobal)
		{
			if (isGlobal)
			{
				PlayerPrefs.SetGlobalFloat($"{Key}_x", value.x);
				PlayerPrefs.SetGlobalFloat($"{Key}_y", value.y);
				PlayerPrefs.SetGlobalFloat($"{Key}_z", value.z);
				return;
			}
			PlayerPrefs.SetFloat($"{Key}_x", value.x);
			PlayerPrefs.SetFloat($"{Key}_y", value.y);
			PlayerPrefs.SetFloat($"{Key}_z", value.z);
		}
	}
}