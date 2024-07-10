namespace OCore.PlayerPrefs
{
	public sealed class PlayerPrefsFloatList : PlayerPrefsList<float>
	{
		public PlayerPrefsFloatList(string key, bool isGlobalPref = false)
			: base(key, isGlobalPref)
		{
		}

		protected override float GetIndex(int index, bool isGlobal)
		{
			if (isGlobal)
			{
				return PlayerPrefs.GetGlobalFloat(Key + index, 0);
			}
			return PlayerPrefs.GetFloat(Key + index, 0);
		}

		protected override void SetIndex(int index, float value, bool isGlobal)
		{
			if (isGlobal)
			{
				PlayerPrefs.SetGlobalFloat(Key + index, value);
				return;
			}
			PlayerPrefs.SetFloat(Key + index, value);
		}
	}
}