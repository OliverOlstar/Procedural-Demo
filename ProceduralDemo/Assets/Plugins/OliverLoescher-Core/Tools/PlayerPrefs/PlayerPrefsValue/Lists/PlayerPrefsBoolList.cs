namespace OCore.PlayerPrefs
{
	public sealed class PlayerPrefsBoolList : PlayerPrefsList<bool>
	{
		public PlayerPrefsBoolList(string key, bool isGlobalPref = false)
			: base(key, isGlobalPref)
		{
		}

		protected override bool GetIndex(int index, bool isGlobal)
		{
			if (isGlobal)
			{
				return PlayerPrefs.GetGlobalInt(Key + index, 0) == 1;
			}
			return PlayerPrefs.GetInt(Key + index, 0) == 1;
		}

		protected override void SetIndex(int index, bool value, bool isGlobal)
		{
			if (isGlobal)
			{
				PlayerPrefs.SetGlobalInt(Key + index, value ? 1 : 0);
				return;
			}
			PlayerPrefs.SetInt(Key + index, value ? 1 : 0);
		}
	}
}