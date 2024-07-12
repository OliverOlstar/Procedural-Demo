using UnityEngine;

namespace ODev.PlayerPrefs
{
	public class PlayerPrefsVector2Int : PlayerPrefsValue<Vector2Int>
	{
		public PlayerPrefsVector2Int(string key, bool isGlobalPref = false)
			: base(key, Vector2Int.zero, isGlobalPref)
		{
		}

		public PlayerPrefsVector2Int(string key, Vector2Int defaultValue, bool isGlobalPref = false)
			: base(key, defaultValue, isGlobalPref)
		{
		}

		protected override Vector2Int Get(bool isGlobal)
		{
			Vector2Int value = Vector2Int.zero;
			if (isGlobal)
			{
				value.x = PlayerPrefs.GetGlobalInt($"{Key}_x", m_DefaultValue.x);
				value.y = PlayerPrefs.GetGlobalInt($"{Key}_y", m_DefaultValue.y);
			}
			else
			{
				value.x = PlayerPrefs.GetInt($"{Key}_x", m_DefaultValue.x);
				value.y = PlayerPrefs.GetInt($"{Key}_y", m_DefaultValue.y);
			}
			return value;
		}

		protected override void Set(Vector2Int value, bool isGlobal)
		{
			if (isGlobal)
			{
				PlayerPrefs.SetGlobalInt($"{Key}_x", value.x);
				PlayerPrefs.SetGlobalInt($"{Key}_y", value.y);
				return;
			}
			PlayerPrefs.SetInt($"{Key}_x", value.x);
			PlayerPrefs.SetInt($"{Key}_y", value.y);
		}
	}
}