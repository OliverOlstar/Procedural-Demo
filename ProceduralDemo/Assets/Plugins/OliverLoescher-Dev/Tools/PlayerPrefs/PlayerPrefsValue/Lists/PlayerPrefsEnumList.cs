using System;

namespace ODev.PlayerPrefs
{
	public class PlayerPrefsEnumList<T> : PlayerPrefsList<T> where T : struct, Enum
	{
		public PlayerPrefsEnumList(string key, bool isGlobalPref = false)
			: base(key, isGlobalPref)
		{
		}

		protected override T GetIndex(int index, bool isGlobal)
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
			Enum.TryParse(value, out T result);
			return result;
		}

		protected override void SetIndex(int index, T value, bool isGlobal)
		{
			if (isGlobal)
			{
				PlayerPrefs.SetGlobalString(Key + index, value.ToString());
				return;
			}
			PlayerPrefs.SetString(Key + index, value.ToString());
		}
	}
}