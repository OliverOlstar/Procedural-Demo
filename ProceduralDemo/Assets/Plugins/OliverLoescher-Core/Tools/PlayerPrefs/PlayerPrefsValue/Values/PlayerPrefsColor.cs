using UnityEngine;

namespace OCore.PlayerPrefs
{
	public class PlayerPrefsColor : PlayerPrefsValue<Color>
	{
		public PlayerPrefsColor(string key, bool isGlobalPref = false)
			: base(key, Color.black, isGlobalPref)
		{
		}

		public PlayerPrefsColor(string key, Color defaultValue, bool isGlobalPref = false)
			: base(key, defaultValue, isGlobalPref)
		{
		}

		protected override Color Get(bool isGlobal)
		{
			Color value = Color.black;
			if (isGlobal)
			{
				value.r = PlayerPrefs.GetGlobalFloat($"{Key}_r", m_DefaultValue.r);
				value.g = PlayerPrefs.GetGlobalFloat($"{Key}_g", m_DefaultValue.g);
				value.b = PlayerPrefs.GetGlobalFloat($"{Key}_b", m_DefaultValue.b);
				value.a = PlayerPrefs.GetGlobalFloat($"{Key}_a", m_DefaultValue.a);
			}
			else
			{
				value.r = PlayerPrefs.GetFloat($"{Key}_r", m_DefaultValue.r);
				value.g = PlayerPrefs.GetFloat($"{Key}_g", m_DefaultValue.g);
				value.b = PlayerPrefs.GetFloat($"{Key}_b", m_DefaultValue.b);
				value.a = PlayerPrefs.GetFloat($"{Key}_a", m_DefaultValue.a);
			}
			return value;
		}

		protected override void Set(Color value, bool isGlobal)
		{
			if (isGlobal)
			{
				PlayerPrefs.SetGlobalFloat($"{Key}_r", value.r);
				PlayerPrefs.SetGlobalFloat($"{Key}_g", value.g);
				PlayerPrefs.SetGlobalFloat($"{Key}_b", value.b);
				PlayerPrefs.SetGlobalFloat($"{Key}_a", value.a);
				return;
			}
			PlayerPrefs.SetFloat($"{Key}_r", value.r);
			PlayerPrefs.SetFloat($"{Key}_g", value.g);
			PlayerPrefs.SetFloat($"{Key}_b", value.b);
			PlayerPrefs.SetFloat($"{Key}_a", value.a);
		}
	}
}