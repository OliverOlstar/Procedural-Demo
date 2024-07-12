using UnityEngine;

namespace ODev.PlayerPrefs
{
	public class PlayerPrefsVector2 : PlayerPrefsValue<Vector2>
	{
		public PlayerPrefsVector2(string key, bool isGlobalPref = false)
			: base(key, Vector2.zero, isGlobalPref)
		{
		}

		public PlayerPrefsVector2(string key, Vector2 defaultValue, bool isGlobalPref = false)
			: base(key, defaultValue, isGlobalPref)
		{
		}

		protected override Vector2 Get(bool isGlobal)
		{
			Vector2 value = Vector2.zero;
			if (isGlobal)
			{
				value.x = PlayerPrefs.GetGlobalFloat($"{Key}_x", m_DefaultValue.x);
				value.y = PlayerPrefs.GetGlobalFloat($"{Key}_y", m_DefaultValue.y);
			}
			else
			{
				value.x = PlayerPrefs.GetFloat($"{Key}_x", m_DefaultValue.x);
				value.y = PlayerPrefs.GetFloat($"{Key}_y", m_DefaultValue.y);
			}
			return value;
		}

		protected override void Set(Vector2 value, bool isGlobal)
		{
			if (isGlobal)
			{
				PlayerPrefs.SetGlobalFloat($"{Key}_x", value.x);
				PlayerPrefs.SetGlobalFloat($"{Key}_y", value.y);
				return;
			}
			PlayerPrefs.SetFloat($"{Key}_x", value.x);
			PlayerPrefs.SetFloat($"{Key}_y", value.y);
		}
	}
}