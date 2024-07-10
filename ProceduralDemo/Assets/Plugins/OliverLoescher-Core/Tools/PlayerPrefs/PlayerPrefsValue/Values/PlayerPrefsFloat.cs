
public sealed class PlayerPrefsFloat : PlayerPrefsValue<float>
{
	public PlayerPrefsFloat(string key, bool isGlobalPref = false)
		: base(key, 0.0f, isGlobalPref)
	{
	}

	public PlayerPrefsFloat(string key, float defaultValue, bool isGlobalPref = false)
		: base(key, defaultValue, isGlobalPref)
	{
	}

	protected override float Get(bool isGlobal)
	{
		if (isGlobal)
		{
			return PlayerPrefs.GetGlobalFloat(Key, m_DefaultValue);
		}
		return PlayerPrefs.GetFloat(Key, m_DefaultValue);
	}

	protected override void Set(float value, bool isGlobal)
	{
		if (isGlobal)
		{
			PlayerPrefs.SetGlobalFloat(Key, value);
			return;
		}
		PlayerPrefs.SetFloat(Key, value);
	}
}
