using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ODev.PlayerPrefs;

namespace ODev.CheatMenu.Pages
{
    public static class CheatMenuDebugLogHandler
	{
		public const int MAX_STRING_COUNT = 256;

		public static PlayerPrefsBool Enabled { get; private set; }
		public static PlayerPrefsBool CallStackEnabled { get; private set; }
		public static PlayerPrefsBool LogsFilterEnabled { get; private set; }
		public static PlayerPrefsString LogsFilter { get; private set; }
		public static PlayerPrefsBool BasicLogsEnabled { get; private set; }
		public static PlayerPrefsBool WarningsEnabled { get; private set; }
		public static PlayerPrefsBool ErrorsEnabled { get; private set; }

		private static string[] m_Filters = null;
		private static string m_SourceFilters = string.Empty;
		private static readonly char[] SPLIT = new char[] { ',' };
		private static string m_LastLog = string.Empty;
		private static int m_Duplicate = 0;

		private static readonly string[] m_Logs = new string[MAX_STRING_COUNT];
		private static int m_CurrentLogIndex = 0;
		private static bool m_FilledLogs = false;

		public static string[] Logs => m_Logs;
		public static int CurrentLogIndex => m_CurrentLogIndex;
		public static bool FilledLogs => m_FilledLogs;

		[RuntimeInitializeOnLoadMethod]
		public static void Initalize()
		{
			Util.Debug.Log("", typeof(CheatMenuDebugLogPage));
			Application.logMessageReceived += HandleLog;

			Enabled = new PlayerPrefsBool("CheatMenu.Logs.Enabled", false, true);
			CallStackEnabled = new PlayerPrefsBool("CheatMenu.Logs.CallStackEnabled", false, true);
			LogsFilterEnabled = new PlayerPrefsBool("CheatMenu.Logs.LogsFilterEnabled", false, true);
			LogsFilter = new PlayerPrefsString("CheatMenu.Logs.LogsFilter", string.Empty, true);
			BasicLogsEnabled = new PlayerPrefsBool("CheatMenu.Logs.BasicLogsEnabled", true, true);
			WarningsEnabled = new PlayerPrefsBool("CheatMenu.Logs.WarningsEnabled", true, true);
			ErrorsEnabled = new PlayerPrefsBool("CheatMenu.Logs.ErrorsEnabled", true, true);
		}

		public static void OnApplicationQuit()
		{
			Util.Debug.Log("", typeof(CheatMenuDebugLogPage));
			Application.logMessageReceived -= HandleLog;
		}

		private static void HandleLog(string pLogString, string pStackTrace, LogType pType)
		{
			if (!Enabled.Value)
			{
				return;
			}
			if (!IsFilterValid(pLogString, pType))
			{
				return;
			}
			if (!CallStackEnabled.Value)
			{
				pStackTrace = string.Empty;
			}
			if (string.Equals(pLogString, m_LastLog))
			{
				m_Duplicate++;
				return;
			}
			else if (m_Duplicate > 0)
			{
				string newLogString = $"[DUPLICATE] x{m_Duplicate} {m_LastLog}";
				AddData(newLogString);
				m_Duplicate = 0;
			}
			switch (pType)
			{
				case LogType.Log:
					break;
				case LogType.Warning:
					pLogString = Util.Debug.ColorString("[WARNING] ", Color.yellow) + pLogString;
					break;
				case LogType.Assert:
					pLogString = Util.Debug.ColorString("[ERROR] ", Color.red) + pLogString;
					break;
				case LogType.Error:
					pLogString = Util.Debug.ColorString("[ERROR] ", Color.red) + pLogString;
					break;
				case LogType.Exception:
					pLogString = Util.Debug.ColorString("[EXCEPTION] " + pLogString, Color.red);
					break;
			}
			m_LastLog = pLogString;
			AddData(pLogString, pStackTrace);
		}

		private static bool IsFilterValid(string pLogString, LogType pType)
		{
			if (pType == LogType.Exception || !LogsFilterEnabled.Value || string.IsNullOrEmpty(LogsFilter.Value))
			{
				return true;
			}

			if ( !string.Equals(m_SourceFilters, LogsFilter.Value)) // Filter value changed
			{
				m_Filters = LogsFilter.Value.Split(SPLIT, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < m_Filters.Length; i++)
				{
					m_Filters[i] = m_Filters[i].Trim();
				}
				m_SourceFilters = LogsFilter.Value;
			}

			for (int i = 0; i < m_Filters.Length; i++)
			{
				if (pLogString.Contains(m_Filters[i]))
				{
					return true;
				}
			}
			return false;
		}

		private static void AddData(string pLog, string pStackTrace = "")
		{
			m_CurrentLogIndex++;
			if (m_CurrentLogIndex >= MAX_STRING_COUNT)
			{
				m_CurrentLogIndex = 0;
				m_FilledLogs = true;
			}
			m_Logs[m_CurrentLogIndex] = pLog + pStackTrace;
		}

		public static void ClearData()
		{
			m_CurrentLogIndex = 0;
			m_FilledLogs = false;
		}
	}
}
