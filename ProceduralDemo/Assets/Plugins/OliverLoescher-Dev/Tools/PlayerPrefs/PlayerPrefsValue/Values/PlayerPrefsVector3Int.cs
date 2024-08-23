using UnityEngine;

namespace ODev.PlayerPrefs
{
	public class PlayerPrefsVector3Int : PlayerPrefsValue<Vector3Int>
	{
		public PlayerPrefsVector3Int(string key, bool isGlobalPref = false)
			: base(key, Vector3Int.zero, isGlobalPref)
		{
		}

		public PlayerPrefsVector3Int(string key, Vector3Int defaultValue, bool isGlobalPref = false)
			: base(key, defaultValue, isGlobalPref)
		{
		}

		protected override Vector3Int Get(bool isGlobal)
		{
			Vector3Int value = Vector3Int.zero;
			if (isGlobal)
			{
				value.x = PlayerPrefs.GetGlobalInt($"{Key}_x", m_DefaultValue.x);
				value.y = PlayerPrefs.GetGlobalInt($"{Key}_y", m_DefaultValue.y);
				value.z = PlayerPrefs.GetGlobalInt($"{Key}_z", m_DefaultValue.z);
			}
			else
			{
				value.x = PlayerPrefs.GetInt($"{Key}_x", m_DefaultValue.x);
				value.y = PlayerPrefs.GetInt($"{Key}_y", m_DefaultValue.y);
				value.z = PlayerPrefs.GetInt($"{Key}_z", m_DefaultValue.z);
			}
			return value;
		}

		protected override void Set(Vector3Int value, bool isGlobal)
		{
			if (isGlobal)
			{
				PlayerPrefs.SetGlobalInt($"{Key}_x", value.x);
				PlayerPrefs.SetGlobalInt($"{Key}_y", value.y);
				PlayerPrefs.SetGlobalInt($"{Key}_z", value.z);
				return;
			}
			PlayerPrefs.SetInt($"{Key}_x", value.x);
			PlayerPrefs.SetInt($"{Key}_y", value.y);
			PlayerPrefs.SetInt($"{Key}_z", value.z);
		}
	}
}