using System;

public class PlayerPrefsEnum<T> : PlayerPrefsValue<T> where T : struct, Enum
{
	public PlayerPrefsEnum(string key, bool isGlobalPref = false)
		: base(key, isGlobalPref)
	{
	}

	public PlayerPrefsEnum(string key, T defaultValue, bool isGlobalPref = false)
		: base(key, defaultValue, isGlobalPref)
	{
	}

	protected override T Get(bool isGlobal)
	{
		string value;
		if (isGlobal)
		{
			value = PlayerPrefs.GetGlobalString(Key, null);
		}
		else
		{
			value = PlayerPrefs.GetString(Key, null);
		}
		if (Enum.TryParse(value, out T result))
		{
			return result;
		}
		return m_DefaultValue;
	}

	protected override void Set(T value, bool isGlobal)
	{
		if (isGlobal)
		{
			PlayerPrefs.SetGlobalString(Key, value.ToString());
			return;
		}
		PlayerPrefs.SetString(Key, value.ToString());
	}
}
