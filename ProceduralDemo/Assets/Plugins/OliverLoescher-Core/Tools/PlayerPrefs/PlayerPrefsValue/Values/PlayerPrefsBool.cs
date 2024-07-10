
public sealed class PlayerPrefsBool : PlayerPrefsValue<bool>
{
	public PlayerPrefsBool(string key, bool isGlobalPref = false)
		: base(key, false, isGlobalPref)
	{
	}

	public PlayerPrefsBool(string key, bool defaultValue, bool isGlobalPref = false)
		: base(key, defaultValue, isGlobalPref)
	{
	}

	protected override bool Get(bool isGlobal)
	{
		if (isGlobal)
		{
			return PlayerPrefs.GetGlobalInt(Key, m_DefaultValue ? 1 : 0) == 1;
		}
		return PlayerPrefs.GetInt(Key, m_DefaultValue ? 1 : 0) == 1;
	}

	protected override void Set(bool value, bool isGlobal)
	{
		if (isGlobal)
		{
			PlayerPrefs.GetGlobalInt(Key, value ? 1 : 0);
			return;
		}
		PlayerPrefs.SetInt(Key, value ? 1 : 0);
	}
}
