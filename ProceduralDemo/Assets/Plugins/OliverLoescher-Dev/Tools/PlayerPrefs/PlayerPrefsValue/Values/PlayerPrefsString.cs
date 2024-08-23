namespace ODev.PlayerPrefs
{
	public sealed class PlayerPrefsString : PlayerPrefsValue<string>
	{
		public PlayerPrefsString(string key, bool isGlobalPref = false)
			: base(key, string.Empty, isGlobalPref)
		{
		}

		public PlayerPrefsString(string key, string defaultValue, bool isGlobalPref = false)
			: base(key, defaultValue, isGlobalPref)
		{
		}

		protected override string Get(bool isGlobal)
		{
			if (isGlobal)
			{
				return PlayerPrefs.GetGlobalString(Key, m_DefaultValue);
			}
			return PlayerPrefs.GetString(Key, m_DefaultValue);
		}

		protected override void Set(string value, bool isGlobal)
		{
			if (isGlobal)
			{
				PlayerPrefs.SetGlobalString(Key, value);
				return;
			}
			PlayerPrefs.SetString(Key, value);
		}
	}
}