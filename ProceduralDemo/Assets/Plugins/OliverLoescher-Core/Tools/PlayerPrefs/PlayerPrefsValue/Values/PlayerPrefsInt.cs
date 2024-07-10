
public sealed class PlayerPrefsInt : PlayerPrefsValue<int>
{
	public PlayerPrefsInt(string key, bool isGlobalPref = false)
		: base(key, 0, isGlobalPref)
	{
	}

	public PlayerPrefsInt(string key, int defaultValue, bool isGlobalPref = false)
		: base(key, defaultValue, isGlobalPref)
	{
	}

	protected override int Get(bool isGlobal)
	{
		if (isGlobal)
		{
			return PlayerPrefs.GetGlobalInt(Key, m_DefaultValue);
		}
		return PlayerPrefs.GetInt(Key, m_DefaultValue);
	}

	protected override void Set(int value, bool isGlobal)
	{
		if (isGlobal)
		{
			PlayerPrefs.SetGlobalInt(Key, value);
			return;
		}
		PlayerPrefs.SetInt(Key, value);
	}
}
