namespace OCore.PlayerPrefs
{
	public sealed class PlayerPrefsIntList : PlayerPrefsList<int>
	{
		public PlayerPrefsIntList(string key, bool isGlobalPref = false)
			: base(key, isGlobalPref)
		{
		}

		protected override int GetIndex(int index, bool isGlobal)
		{
			if (isGlobal)
			{
				return PlayerPrefs.GetGlobalInt(Key + index, 0);
			}
			return PlayerPrefs.GetInt(Key + index, 0);
		}

		protected override void SetIndex(int index, int value, bool isGlobal)
		{
			if (isGlobal)
			{
				PlayerPrefs.SetGlobalInt(Key + index, value);
				return;
			}
			PlayerPrefs.SetInt(Key + index, value);
		}
	}
}