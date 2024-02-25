
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public abstract class LockEditor<T> where T : LockEditor<T>
	{
		public enum ReportType
		{
			Summary,
			Full,
		}

		private static string EditorPrefsKey(ReportType reportType) => Core.Str.Build(Application.dataPath, typeof(T).Name, reportType.ToString());

		public static bool IsLocked() { return TryGetReport(out _, out _); }

		public static bool TryGetReport(out string summary, out string full)
		{
			summary = null;
			full = null;
#if UNITY_EDITOR
			summary = UnityEditor.EditorPrefs.GetString(EditorPrefsKey(ReportType.Summary), null);
			full = UnityEditor.EditorPrefs.GetString(EditorPrefsKey(ReportType.Full), null);
#endif
			return !string.IsNullOrEmpty(summary) && !string.IsNullOrEmpty(full);
		}
		private static void SetReport(ReportType reportType, string report)
		{
#if UNITY_EDITOR
			UnityEditor.EditorPrefs.SetString(EditorPrefsKey(reportType), report);
#endif
		}
		public static void SetLock(string summary, string full)
		{
			SetReport(ReportType.Summary, summary);
			SetReport(ReportType.Full, full);
		}
		private static void ClearReport(ReportType reportType)
		{
#if UNITY_EDITOR
			UnityEditor.EditorPrefs.DeleteKey(EditorPrefsKey(reportType));
#endif
		}
		public static void ClearLock()
		{
			ClearReport(ReportType.Summary);
			ClearReport(ReportType.Full);
		}

		public static bool ShowErrorReport()
		{
			if (!TryGetReport(out string summaryReport, out string fullReport))
			{
				return false;
			}
			Debug.LogError(fullReport);
#if UNITY_EDITOR
			UnityEditor.EditorUtility.DisplayDialog(
					"Editor Locked",
					summaryReport + "\n\nSee console for details...",
					"Ok");
			UnityEditor.EditorApplication.isPlaying = false;
#endif
			return true;
		}
	}
}
