using UnityEngine;

namespace ODev.CheatMenu
{
	public static class CheatMenuStyles
	{
		private static GUIStyle m_SmallStyle = null;
		public static GUIStyle SmallLabel
		{
			get
			{
				if (m_SmallStyle == null)
				{
					m_SmallStyle = new GUIStyle();
					m_SmallStyle.fontSize = 10;
					m_SmallStyle.alignment = TextAnchor.MiddleLeft;
					m_SmallStyle.wordWrap = true;
					m_SmallStyle.normal.textColor = Color.white;
				}
				return m_SmallStyle;
			}
		}

		private static GUIStyle m_SmallButtonStyle = null;
		public static GUIStyle SmallButton
		{
			get
			{
				if (m_SmallButtonStyle == null)
				{
					m_SmallButtonStyle = GUI.skin.button;
					m_SmallButtonStyle.fontSize = 10;
					m_SmallButtonStyle.wordWrap = true;
					m_SmallButtonStyle.normal.textColor = Color.white;
				}
				return m_SmallButtonStyle;
			}
		}

		private static GUIStyle m_ExtraSmallButtonStyle = null;
		public static GUIStyle ExtraSmallButtonStyle
		{
			get
			{
				if (m_ExtraSmallButtonStyle == null)
				{
					m_ExtraSmallButtonStyle = SmallButton;
					m_ExtraSmallButtonStyle.fontSize = 8;
				}
				return m_ExtraSmallButtonStyle;
			}
		}
	}
}
