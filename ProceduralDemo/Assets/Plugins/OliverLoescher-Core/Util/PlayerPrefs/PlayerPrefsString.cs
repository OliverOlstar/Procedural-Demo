using UnityEngine;

namespace OCore
{
	public struct PlayerPrefsString
	{
		private readonly string m_Key;
		private string m_Value;

		public PlayerPrefsString(string pKey, string pDefaultValue = null)
		{
			m_Key = pKey;
			m_Value = PlayerPrefs.GetString(m_Key, pDefaultValue);
		}

		public readonly string Get()
		{
			return m_Value;
		}

		public void Set(string pValue)
		{
			if (m_Value == pValue)
			{
				return;
			}
			m_Value = pValue;
			PlayerPrefs.SetString(m_Key, m_Value);
		}
	}
}
