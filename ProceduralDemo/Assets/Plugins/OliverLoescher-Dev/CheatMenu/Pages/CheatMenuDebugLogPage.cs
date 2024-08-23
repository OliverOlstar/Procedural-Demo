using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Handler = ODev.CheatMenu.Pages.CheatMenuDebugLogHandler;

namespace ODev.CheatMenu.Pages
{
	public class CheatMenuDebugLogPage : CheatMenuPage
	{
		public override CheatMenuGroup Group => CheatMenuODevGroups.Debug;
		public override string Name => "Logs";
		public override bool IsAvailable() => true;

		private bool m_IsInitalized = false;
		private readonly StringBuilder m_StringBuilder = new();

		private Texture2D m_EnabledIcon;
		private Texture2D m_DisabledIcon;
		private Texture2D m_LogIcon;
		private Texture2D m_LogIconDisabled;
		private Texture2D m_WarningIcon;
		private Texture2D m_WarningIconDisabled;
		private Texture2D m_ErrorIcon;
		private Texture2D m_ErrorIconDisabled;
		private Texture2D m_FilterIcon;
		private Texture2D m_ClearIcon;
		private Texture2D m_CopyIcon;
		private Texture2D m_CallStackIconDisabled;

		public override void OnBecameActive()
		{
			if (m_IsInitalized)
			{
				return;
			}
			m_IsInitalized = true;

			m_EnabledIcon = Resources.Load<Texture2D>("EnabledIcon");
			m_DisabledIcon = Resources.Load<Texture2D>("DisabledIcon");
			m_LogIcon = Resources.Load<Texture2D>("LogIconFull");
			m_LogIconDisabled = Resources.Load<Texture2D>("LogIconEmpty");
			m_WarningIcon = Resources.Load<Texture2D>("CautionIconFull");
			m_WarningIconDisabled = Resources.Load<Texture2D>("CautionIconEmpty");
			m_ErrorIcon = Resources.Load<Texture2D>("ErrorIconFull");
			m_ErrorIconDisabled = Resources.Load<Texture2D>("ErrorIconEmpty");
			m_FilterIcon = Resources.Load<Texture2D>("FilterIcon");
			m_ClearIcon = Resources.Load<Texture2D>("ClearIcon");
			m_CopyIcon = Resources.Load<Texture2D>("CopyIcon");
			m_CallStackIconDisabled = Resources.Load<Texture2D>("StackIconEmpty");
		}

		public override void DrawGUI()
		{
			using (Util.GUI.UsableHorizontal.Use())
			{
				Handler.Enabled.Value = ToggleButton(Handler.Enabled.Value, m_EnabledIcon, m_DisabledIcon, 64.0f);
				Handler.CallStackEnabled.Value = ToggleButton(Handler.CallStackEnabled.Value, m_CallStackIconDisabled, m_CallStackIconDisabled);
				Handler.BasicLogsEnabled.Value = ToggleButton(Handler.BasicLogsEnabled.Value, m_LogIcon, m_LogIconDisabled);
				Handler.WarningsEnabled.Value = ToggleButton(Handler.WarningsEnabled.Value, m_WarningIcon, m_WarningIconDisabled);
				Handler.ErrorsEnabled.Value = ToggleButton(Handler.ErrorsEnabled.Value, m_ErrorIcon, m_ErrorIconDisabled);
			}
			using (Util.GUI.UsableHorizontal.Use())
			{
				CopyButton();
				ClearButton();
				Handler.LogsFilterEnabled.Value = ToggleButton(Handler.LogsFilterEnabled.Value, m_FilterIcon, m_FilterIcon);
				if (Handler.LogsFilterEnabled.Value)
				{
					Handler.LogsFilter.Value = GUILayout.TextField(Handler.LogsFilter.Value);
				}
			}

			for (int i = Handler.CurrentLogIndex; i >= 0; i--)
			{
				GUILayout.Label(Handler.Logs[i]);
			}
			if (Handler.FilledLogs)
			{
				for (int i = Handler.MAX_STRING_COUNT - 1; i > Handler.CurrentLogIndex; i--)
				{
					GUILayout.Label(Handler.Logs[i]);
				}
			}
		}

		private void CopyButton()
		{
			if (GUILayout.Button(new GUIContent(m_CopyIcon), GUILayout.Width(32.0f)))
			{
				foreach (string member in Handler.Logs)
				{
					m_StringBuilder.AppendLine(member);
				}
				// UniPasteBoard.SetClipBoardString(m_StringBuilder.ToString());
				m_StringBuilder.Clear();
				Util.Debug.DevException(new NotImplementedException(), typeof(CheatMenuDebugLogPage));
			}
		}

		private void ClearButton()
		{
			if (GUILayout.Button(new GUIContent(m_ClearIcon), GUILayout.Width(32.0f)))
			{
				Handler.ClearData();
			}
		}

		private bool ToggleButton(bool pEnabled, Texture2D pEnabledTexture, Texture2D pDisabledTexture, float pWidth = 32.0f, Action pCallback = null)
		{
			GUI.backgroundColor = pEnabled ? Color.white : Color.black;
			Texture2D icon = pEnabled ? pEnabledTexture : pDisabledTexture;
			if (GUILayout.Button(new GUIContent(icon), GUILayout.Width(pWidth)))
			{
				pEnabled = !pEnabled;
				pCallback?.Invoke();
			}
			GUI.backgroundColor = Color.white;
			return pEnabled;
		}
	}
}