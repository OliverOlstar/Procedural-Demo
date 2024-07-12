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
		public static void DevException<T>(T pException, UnityEngine.Object pContext, [CallerMemberName] string pMethodName = "") where T : Exception
		{
			LogError(pException.Message, pContext, pMethodName);
#if RELEASE
			UnityEngine.Debug.LogException(pException);
#else
			throw pException;
#endif
		}
		[Conditional("ENABLE_DEBUG_EXCEPTIONS"), HideInCallstack]
		public static void DevException<T>(T pException, Type pContext, [CallerMemberName] string pMethodName = "") where T : Exception
		{
			LogError(pException.Message, pContext, pMethodName);
#if RELEASE
			UnityEngine.Debug.LogException(pException);
#else
			throw pException;
#endif
		}

		[Conditional("ENABLE_DEBUG_EXCEPTIONS"), HideInCallstack]
		public static void DevException(string pMessage, UnityEngine.Object pContext, [CallerMemberName] string pMethodName = "")
		{
#if RELEASE
			UnityEngine.Debug.LogException(new InvalidOperationException(CreateLogMessage(pMessage, pMethodName, pContext)), pContext);
#else
			throw new InvalidOperationException(CreateLogMessage(pMessage, pMethodName, pContext));
#endif
		}
		[Conditional("ENABLE_DEBUG_EXCEPTIONS"), HideInCallstack]
		public static void DevException(string pMessage, Type pContext, [CallerMemberName] string pMethodName = "")
		{
#if RELEASE
			UnityEngine.Debug.LogException(new InvalidOperationException(CreateLogMessage(pMessage, pMethodName, pContext)));
#else
			throw new InvalidOperationException(CreateLogMessage(pMessage, pMethodName, pContext));
#endif
		}

		[Conditional("ENABLE_DEBUG_EXCEPTIONS"), HideInCallstack]
		public static void NotImplementedException()
		{
#if RELEASE
			UnityEngine.Debug.LogException(new NotImplementedException());
#else
			throw new InvalidOperationException();
#endif
		}

		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
		public static void LogBasic(string pMessage)
		{
			UnityEngine.Debug.Log(pMessage);
		}

		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
		public static void Log(string pMessage, UnityEngine.Object pContext, [CallerMemberName] string pMethodName = "")
		{
			UnityEngine.Debug.Log(CreateLogMessage(pMessage, pMethodName, pContext), pContext);
		}
		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
		public static void Log(string pMessage, Type pContext, [CallerMemberName] string pMethodName = "")
		{
			UnityEngine.Debug.Log(CreateLogMessage(pMessage, pMethodName, pContext));
		}

		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
		public static void LogWarning(string pMessage, UnityEngine.Object pContext, [CallerMemberName] string pMethodName = "")
		{
			UnityEngine.Debug.LogWarning(CreateLogMessage(pMessage, pMethodName, pContext), pContext);
		}
		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
		public static void LogWarning(string pMessage, Type pContext, [CallerMemberName] string pMethodName = "")
		{
			UnityEngine.Debug.LogWarning(CreateLogMessage(pMessage, pMethodName, pContext));
		}

		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
		public static void LogError(string pMessage, UnityEngine.Object pContext, [CallerMemberName] string pMethodName = "")
		{
			UnityEngine.Debug.LogError(CreateLogMessage(pMessage, pMethodName, pContext), pContext);
		}
		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
		public static void LogError(string pMessage, Type pContext, [CallerMemberName] string pMethodName = "")
		{
			UnityEngine.Debug.LogError(CreateLogMessage(pMessage, pMethodName, pContext));
		}

		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
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

		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
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

		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
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

		private static string CreateLogMessage(string pMessage, string pMethodName, UnityEngine.Object pContext)
		{
			if (pContext == null)
			{
				return $"[].{pMethodName}() {pMessage}";
			}
			return ColorString($"[{pContext.GetType().FullName}]") + ColorString($"[{pContext.name}]") + $".{pMethodName}() {pMessage}";
		}
		private static string CreateLogMessage(string pMessage, string pMethodName, Type pContext)
		{
			return ColorString($"[{pContext.FullName}]") + $".{pMethodName}() {pMessage}";
		}

		public static string ColorString(string pString)
		{
#if UNITY_EDITOR
			Color color;
			switch (pString.Length) // For saftely
			{
				case 0:
					return pString;
				case 1:
					color = new Color(0.8f, 0.8f, CharToFloat01(pString[0]));
					break;
				case 2:
					color = new Color(CharToFloat01(pString[0]), CharToFloat01(pString[1]), 0.8f);
					break;
				case 3:
					color = new Color(CharToFloat01(pString[2]), CharToFloat01(pString[1]), CharToFloat01(pString[0]));
					break;
				default:
					color = new Color(CharToFloat01(pString[1]), CharToFloat01(pString[^2]), CharToFloat01(pString[2]));
					break;
			}
			return ColorString(color, pString);
#else
			return pString;
#endif
		}
#if UNITY_EDITOR
		private static float CharToFloat01(char pChar)
		{
			return ((pChar - 32.0f) / 90.0f) % 1.0f;
		}
#endif

		public static string ColorString(Color pColor, string pString)
		{
#if UNITY_EDITOR
			s_StringBuilder.Clear();
			s_StringBuilder.Append("<color=#");
			s_StringBuilder.Append(ColorUtility.ToHtmlStringRGBA(pColor));
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
#if ENABLE_DEBUG_LOGGING
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

		#region Gizmos
		[Conditional("ENABLE_DEBUG_GIZMOS")]
		public static void GizmoCapsule(Vector3 pVectorA, Vector3 pVectorB, float pRadius)
		{
			Gizmos.DrawWireSphere(pVectorA, pRadius);
			Gizmos.DrawLine(pVectorA + (Vector3.forward * pRadius), pVectorB + (Vector3.forward * pRadius));
			Gizmos.DrawLine(pVectorA + (Vector3.left * pRadius), pVectorB + (Vector3.left * pRadius));
			Gizmos.DrawLine(pVectorA + (Vector3.right * pRadius), pVectorB + (Vector3.right * pRadius));
			Gizmos.DrawLine(pVectorA + (Vector3.back * pRadius), pVectorB + (Vector3.back * pRadius));
			Gizmos.DrawWireSphere(pVectorB, pRadius);
		}
		[Conditional("ENABLE_DEBUG_GIZMOS")]
		public static void GizmoCapsule(Vector3 pCenter, float pRadius, float pHeight)
		{
			pHeight -= pRadius * 2.0f;
			if (pHeight <= 0)
			{
				Gizmos.DrawWireSphere(pCenter, pRadius);
				return;
			}
			Vector3 top = pCenter + (0.5f * pHeight * Vector3.up);
			Vector3 bottem = pCenter + (0.5f * pHeight * Vector3.down);
			GizmoCapsule(top, bottem, pRadius);
		}
		[Conditional("ENABLE_DEBUG_GIZMOS")]
		public static void GizmoCapsule(Vector3 pVectorA, Vector3 pVectorB, float pRadius, Matrix4x4 pMatrix)
		{
			Gizmos.matrix = pMatrix;
			GizmoCapsule(pVectorA, pVectorB, pRadius);
			Gizmos.matrix = Matrix4x4.identity;
		}
		[Conditional("ENABLE_DEBUG_GIZMOS")]
		public static void GizmoCapsule(Vector3 pCenter, float pRadius, float pHeight, Matrix4x4 pMatrix)
		{
			Gizmos.matrix = pMatrix;
			GizmoCapsule(pCenter, pRadius, pHeight);
			Gizmos.matrix = Matrix4x4.identity;
		}
		#endregion Gizmos
	}
}
