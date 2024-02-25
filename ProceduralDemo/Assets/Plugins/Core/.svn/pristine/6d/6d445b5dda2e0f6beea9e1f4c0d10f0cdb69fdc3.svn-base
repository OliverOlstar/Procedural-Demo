
public sealed class PlayerPrefsStringList : PlayerPrefsList<string>
{
	public PlayerPrefsStringList(string key, bool isGlobalPref = false)
		: base(key, isGlobalPref)
	{
	}

	protected override string GetIndex(int index, bool isGlobal)
	{
		if (isGlobal)
		{
			return PlayerPrefs.GetGlobalString(Key + index, string.Empty);
		}
		return PlayerPrefs.GetString(Key + index, string.Empty);
	}

	protected override void SetIndex(int index, string value, bool isGlobal)
	{
		if (isGlobal)
		{
			PlayerPrefs.GetGlobalString(Key + index, value);
			return;
		}
		PlayerPrefs.GetString(Key + index, value);
	}
}
