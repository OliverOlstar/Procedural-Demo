using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace ODev.Util
{
	public static class Debug
	{
		#region Logs
		private static readonly StringBuilder s_StringBuilder = new();

		[Conditional("ENABLE_DEBUG_EXCEPTIONS"), HideInCallstack]
		public static void DevException<T>(this UnityEngine.Object pContext, T pException, [CallerMemberName] string pMethodName = "", [CallerLineNumber] int pLineNumber = -1) where T : Exception
		{
			LogAssertion(pContext, pException.Message, pMethodName, pLineNumber);
#if RELEASE
			UnityEngine.Debug.LogException(pException);
#else
			throw pException;
#endif
		}
		[Conditional("ENABLE_DEBUG_EXCEPTIONS"), HideInCallstack]
		public static void DevException<T>(this object pContext, T pException, [CallerMemberName] string pMethodName = "", [CallerLineNumber] int pLineNumber = -1) where T : Exception
		{
			LogAssertion(pContext, pException.Message, pMethodName, pLineNumber);
#if RELEASE
			UnityEngine.Debug.LogException(pException);
#else
			throw pException;
#endif
		}
		[Conditional("ENABLE_DEBUG_EXCEPTIONS"), HideInCallstack]
		public static void DevException<T>(this Type pContext, T pException, [CallerMemberName] string pMethodName = "", [CallerLineNumber] int pLineNumber = -1) where T : Exception
		{
			LogAssertion(pContext, pException.Message, pMethodName, pLineNumber);
#if RELEASE
			UnityEngine.Debug.LogException(pException);
#else
			throw pException;
#endif
		}

		[Conditional("ENABLE_DEBUG_EXCEPTIONS"), HideInCallstack]
		public static void DevException(this UnityEngine.Object pContext, string pMessage, [CallerMemberName] string pMethodName = "", [CallerLineNumber] int pLineNumber = -1)
		{
#if RELEASE
			UnityEngine.Debug.LogException(new InvalidOperationException(CreateLogMessage(pMessage, pMethodName, pLineNumber, pContext)), pContext);
#else
			throw new InvalidOperationException(CreateLogMessage(pMessage, pMethodName, pLineNumber, pContext));
#endif
		}
		[Conditional("ENABLE_DEBUG_EXCEPTIONS"), HideInCallstack]
		public static void DevException(this object pContext, string pMessage, [CallerMemberName] string pMethodName = "", [CallerLineNumber] int pLineNumber = -1)
		{
#if RELEASE
			UnityEngine.Debug.LogException(new InvalidOperationException(CreateLogMessage(pMessage, pMethodName, pLineNumber, pContext.GetType())));
#else
			throw new InvalidOperationException(CreateLogMessage(pMessage, pMethodName, pLineNumber, pContext.GetType()));
#endif
		}
		[Conditional("ENABLE_DEBUG_EXCEPTIONS"), HideInCallstack]
		public static void DevException(this Type pContext, string pMessage, [CallerMemberName] string pMethodName = "", [CallerLineNumber] int pLineNumber = -1)
		{
#if RELEASE
			UnityEngine.Debug.LogException(new InvalidOperationException(CreateLogMessage(pMessage, pMethodName, pLineNumber, pContext)));
#else
			throw new InvalidOperationException(CreateLogMessage(pMessage, pMethodName, pLineNumber, pContext));
#endif
		}

		[Conditional("ENABLE_DEBUG_LOGS"), HideInCallstack]
		public static void LogBasic(string pMessage)
		{
			UnityEngine.Debug.Log(pMessage);
		}

		[Conditional("ENABLE_DEBUG_LOGS"), HideInCallstack]
		public static void Log(this UnityEngine.Object pContext, string pMessage = "", [CallerMemberName] string pMethodName = "", [CallerLineNumber] int pLineNumber = -1)
		{
			UnityEngine.Debug.Log(CreateLogMessage(pMessage, pMethodName, pLineNumber, pContext), pContext);
		}
		[Conditional("ENABLE_DEBUG_LOGS"), HideInCallstack]
		public static void Log(this object pContext, string pMessage = "", [CallerMemberName] string pMethodName = "", [CallerLineNumber] int pLineNumber = -1)
		{
			UnityEngine.Debug.Log(CreateLogMessage(pMessage, pMethodName, pLineNumber, pContext.GetType()));
		}
		[Conditional("ENABLE_DEBUG_LOGS"), HideInCallstack]
		public static void Log(this Type pContext, string pMessage, [CallerMemberName] string pMethodName = "", [CallerLineNumber] int pLineNumber = -1)
		{
			UnityEngine.Debug.Log(CreateLogMessage(pMessage, pMethodName, pLineNumber, pContext));
		}

		[Conditional("ENABLE_DEBUG_WARNINGS"), HideInCallstack]
		public static void LogWarning(this UnityEngine.Object pContext, string pMessage, [CallerMemberName] string pMethodName = "", [CallerLineNumber] int pLineNumber = -1)
		{
			UnityEngine.Debug.LogWarning(CreateLogMessage(pMessage, pMethodName, pLineNumber, pContext), pContext);
		}
		[Conditional("ENABLE_DEBUG_WARNINGS"), HideInCallstack]
		public static void LogWarning(this object pContext, string pMessage, [CallerMemberName] string pMethodName = "", [CallerLineNumber] int pLineNumber = -1)
		{
			UnityEngine.Debug.LogWarning(CreateLogMessage(pMessage, pMethodName, pLineNumber, pContext.GetType()));
		}
		[Conditional("ENABLE_DEBUG_WARNINGS"), HideInCallstack]
		public static void LogWarning(this Type pContext, string pMessage, [CallerMemberName] string pMethodName = "", [CallerLineNumber] int pLineNumber = -1)
		{
			UnityEngine.Debug.LogWarning(CreateLogMessage(pMessage, pMethodName, pLineNumber, pContext));
		}

		[Conditional("ENABLE_DEBUG_ERRORS"), HideInCallstack]
		public static void LogError(this UnityEngine.Object pContext, string pMessage, [CallerMemberName] string pMethodName = "", [CallerLineNumber] int pLineNumber = -1)
		{
			UnityEngine.Debug.LogError(CreateLogMessage(pMessage, pMethodName, pLineNumber, pContext), pContext);
		}
		[Conditional("ENABLE_DEBUG_ERRORS"), HideInCallstack]
		public static void LogError(this object pContext, string pMessage, [CallerMemberName] string pMethodName = "", [CallerLineNumber] int pLineNumber = -1)
		{
			UnityEngine.Debug.LogError(CreateLogMessage(pMessage, pMethodName, pLineNumber, pContext.GetType()));
		}
		[Conditional("ENABLE_DEBUG_ERRORS"), HideInCallstack]
		public static void LogError(this Type pContext, string pMessage, [CallerMemberName] string pMethodName = "", [CallerLineNumber] int pLineNumber = -1)
		{
			UnityEngine.Debug.LogError(CreateLogMessage(pMessage, pMethodName, pLineNumber, pContext));
		}

		[Conditional("ENABLE_DEBUG_EXCEPTIONS"), HideInCallstack]
		public static void LogAssertion(this UnityEngine.Object pContext, string pMessage, [CallerMemberName] string pMethodName = "", [CallerLineNumber] int pLineNumber = -1)
		{
			UnityEngine.Debug.LogAssertion(CreateLogMessage(pMessage, pMethodName, pLineNumber, pContext), pContext);
		}
		[Conditional("ENABLE_DEBUG_EXCEPTIONS"), HideInCallstack]
		public static void LogAssertion(this object pContext, string pMessage, [CallerMemberName] string pMethodName = "", [CallerLineNumber] int pLineNumber = -1)
		{
			UnityEngine.Debug.LogAssertion(CreateLogMessage(pMessage, pMethodName, pLineNumber, pContext.GetType()));
		}
		[Conditional("ENABLE_DEBUG_EXCEPTIONS"), HideInCallstack]
		public static void LogAssertion(this Type pContext, string pMessage, [CallerMemberName] string pMethodName = "", [CallerLineNumber] int pLineNumber = -1)
		{
			UnityEngine.Debug.LogAssertion(CreateLogMessage(pMessage, pMethodName, pLineNumber, pContext));
		}

		[Conditional("ENABLE_DEBUG_LOGS"), HideInCallstack]
		public static void Log<TKey, TValue>(string pMessage, Dictionary<TKey, TValue> pDictionary)
		{
			s_StringBuilder.Clear();
			foreach (KeyValuePair<TKey, TValue> value in pDictionary)
			{
				s_StringBuilder.Append(value.Key);
				s_StringBuilder.Append(": ");
				s_StringBuilder.Append(value.Value);
				s_StringBuilder.Append(", ");
			}
			s_StringBuilder.Remove(s_StringBuilder.Length - 2, 2);
			UnityEngine.Debug.Log($"{pMessage} [{s_StringBuilder}]");
		}

		[Conditional("ENABLE_DEBUG_LOGS"), HideInCallstack]
		public static void Log<TValue>(string pMessage, IEnumerable<TValue> pValues)
		{
			s_StringBuilder.Clear();
			s_StringBuilder.Append(pMessage);
			s_StringBuilder.Append(" [");

			foreach (TValue value in pValues)
			{
				s_StringBuilder.Append(value);
				s_StringBuilder.Append(", ");
			}
			s_StringBuilder.Remove(s_StringBuilder.Length - 2, 2);

			s_StringBuilder.Append("]");
			UnityEngine.Debug.Log(s_StringBuilder.ToString());
		}

		[Conditional("ENABLE_DEBUG_LOGS"), HideInCallstack]
		public static void Log<TValue>(string pMessage, TValue[,] pValues)
		{
			s_StringBuilder.Clear();
			s_StringBuilder.Append(pMessage);
			s_StringBuilder.Append(" { ");

			int xLength = pValues.GetLength(0);
			int yLength = pValues.GetLength(1);
			for (int x = 0; x < xLength; x++)
			{
				s_StringBuilder.Append("{ ");
				for (int y = 0; y < yLength; y++)
				{
					if (y == yLength - 1)
					{
						s_StringBuilder.Append($"{pValues[x, y]}" + "}");
					}
					else
					{
						s_StringBuilder.Append($"{pValues[x, y]}, ");
					}
				}
				if (x < xLength - 1)
				{
					s_StringBuilder.Append(", ");
				}
			}

			s_StringBuilder.Append(" }");
			LogBasic(s_StringBuilder.ToString());
		}

		private static string CreateLogMessage(string pMessage, string pMethodName, int pLineNumber, UnityEngine.Object pContext)
		{
			if (pContext == null)
			{
				return $"[].{pMethodName}():{pLineNumber} {pMessage}";
			}
			string objectName = pContext.name;
			string typeName = pContext.GetType().Name;
			bool same = objectName.Equals(typeName);

			objectName = same ? string.Empty : ColourString($"({objectName})", GetAutoColour(objectName));
			typeName = ColourString($"{typeName}::{pMethodName}:{pLineNumber} ", GetAutoColour(typeName));
			return objectName + typeName + pMessage;
		}
		private static string CreateLogMessage(string pMessage, string pMethodName, int pLineNumber, Type pContext)
		{
			string typeName = pContext.Name;
			typeName = ColourString($"[{typeName}].{pMethodName}():{pLineNumber} ", GetAutoColour(typeName));
			return typeName + pMessage;
		}

		public static Color GetAutoColour(string pString)
		{
#if UNITY_EDITOR
			static float CharToFloat01(char pChar)
			{
				float value = (char.ToLower(pChar) - 32.0f) / 90.0f;
				return Mathf.Pow(value, 2);
			}

			Color color;
			switch (pString.Length) // For saftely
			{
				case 0:
					return default;
				case 1:
					color = new Color(0.8f, 0.8f, CharToFloat01(pString[0]));
					break;
				case 2:
					color = new Color(CharToFloat01(pString[0]), CharToFloat01(pString[1]), 0.8f);
					break;
				case 3:
					color = new Color(CharToFloat01(pString[0]), CharToFloat01(pString[1]), CharToFloat01(pString[2]));
					break;
				default:
					color = new Color(CharToFloat01(pString[0]), CharToFloat01(pString[^1]), CharToFloat01(pString[1]));
					break;
			}
			return color;
#else
			return default;
#endif
		}

		public static string AutoColourString(string pString)
		{
#if UNITY_EDITOR
			if (string.IsNullOrEmpty(pString))
			{
				return pString;
			}
			return ColourString(pString, GetAutoColour(pString));
#else
			return pString;
#endif
		}

		public static string ColourString(string pString, Color pColour)
		{
#if !RELEASE
			s_StringBuilder.Clear();
			s_StringBuilder.Append("<color=#");
			s_StringBuilder.Append(ColorUtility.ToHtmlStringRGBA(pColour));
			s_StringBuilder.Append(">");
			s_StringBuilder.Append(pString);
			s_StringBuilder.Append("</color>");
			return s_StringBuilder.ToString();
#else
			return pString;
#endif
		}
		#endregion Logs

		public static string GetPath(Transform transform)
		{
#if ENABLE_DEBUG_LOGS
			if (transform.parent == null)
			{
				return transform.name;
			}
			return $"{transform.name}/{GetPath(transform.parent)}";
#else
			return string.Empty;
#endif
		}

		public static string BuildWithBetweens(string between, params string[] list)
		{
			if (list.Length == 0)
			{
				return string.Empty;
			}
			s_StringBuilder.Clear();
			s_StringBuilder.Append(list[0]);
			for (int i = 1; i < list.Length; i++)
			{
				s_StringBuilder.Append(between);
				s_StringBuilder.Append(list[i]);
			}
			return s_StringBuilder.ToString();
		}
	}
}
