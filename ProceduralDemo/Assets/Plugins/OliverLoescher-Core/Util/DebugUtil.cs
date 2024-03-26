using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OliverLoescher.Util
{
	public static class Debug2
	{
		#region Logs
		private static StringBuilder StringBuilder = new StringBuilder();

		[Conditional("ENABLE_DEBUG_EXCEPTIONS")]
		public static void DevException<T>(T pException) where T : Exception
		{
#if RELEASE
			UnityEngine.Debug.LogException(pException);
#else
			throw pException;
#endif
		}

		[Conditional("ENABLE_DEBUG_EXCEPTIONS")]
		public static void DevException(string pMessage)
		{
#if RELEASE
			UnityEngine.Debug.LogException(new InvalidOperationException(pMessage));
#else
			throw new InvalidOperationException(pMessage);
#endif
		}

		[Conditional("ENABLE_DEBUG_EXCEPTIONS")]
		public static void DevException(string pMessage, string pMethodName, UnityEngine.Object pObject)
		{
#if RELEASE
			UnityEngine.Debug.LogException(new InvalidOperationException(CreateLogMessage(pMessage, pMethodName, pObject)), pObject);
#else
			throw new InvalidOperationException(CreateLogMessage(pMessage, pMethodName, pObject));
#endif
		}

		[Conditional("ENABLE_DEBUG_EXCEPTIONS")]
		public static void DevException(string pMessage, string pMethodName, Type pObject)
		{
#if RELEASE
			UnityEngine.Debug.LogException(new InvalidOperationException(CreateLogMessage(pMessage, pMethodName, pObject)));
#else
			throw new InvalidOperationException(CreateLogMessage(pMessage, pMethodName, pObject));
#endif
		}

		[Conditional("ENABLE_DEBUG_EXCEPTIONS")]
		public static void NotImplementedException()
		{
#if RELEASE
			UnityEngine.Debug.LogException(new NotImplementedException());
#else
			throw new InvalidOperationException();
#endif
		}

		[Conditional("ENABLE_DEBUG_LOGGING")]
		public static void LogBasic(string pMessage)
		{
			UnityEngine.Debug.Log(pMessage);
		}

		[Conditional("ENABLE_DEBUG_LOGGING")]
		public static void Log(string pMessage, string pMethodName, UnityEngine.Object pObject)
		{
			UnityEngine.Debug.Log(CreateLogMessage(pMessage, pMethodName, pObject), pObject);
		}
		[Conditional("ENABLE_DEBUG_LOGGING")]
		public static void Log(string pMessage, string pMethodName, Type pObject)
		{
			UnityEngine.Debug.Log(CreateLogMessage(pMessage, pMethodName, pObject));
		}

		[Conditional("ENABLE_DEBUG_LOGGING")]
		public static void LogWarning(string pMessage, string pMethodName, UnityEngine.Object pObject)
		{
			UnityEngine.Debug.LogWarning(CreateLogMessage(pMessage, pMethodName, pObject), pObject);
		}
		[Conditional("ENABLE_DEBUG_LOGGING")]
		public static void LogWarning(string pMessage, string pMethodName, Type pObject)
		{
			UnityEngine.Debug.LogWarning(CreateLogMessage(pMessage, pMethodName, pObject));
		}

		[Conditional("ENABLE_DEBUG_LOGGING")]
		public static void LogError(string pMessage, string pMethodName, UnityEngine.Object pObject)
		{
			UnityEngine.Debug.LogError(CreateLogMessage(pMessage, pMethodName, pObject), pObject);
		}
		[Conditional("ENABLE_DEBUG_LOGGING")]
		public static void LogError(string pMessage, string pMethodName, Type pObject)
		{
			UnityEngine.Debug.LogError(CreateLogMessage(pMessage, pMethodName, pObject));
		}

		[Conditional("ENABLE_DEBUG_LOGGING")]
		public static void Log<TKey, TValue>(string pMessage, Dictionary<TKey, TValue> pDictionary)
		{
			StringBuilder.Clear();
			foreach (KeyValuePair<TKey, TValue> value in pDictionary)
			{
				StringBuilder.Append(value.Key);
				StringBuilder.Append(": ");
				StringBuilder.Append(value.Value);
				StringBuilder.Append(", ");
			}
			StringBuilder.Remove(StringBuilder.Length - 2, 2);
			UnityEngine.Debug.Log($"{pMessage} [{StringBuilder}]");
		}

		[Conditional("ENABLE_DEBUG_LOGGING")]
		public static void Log<TValue>(string pMessage, IEnumerable<TValue> pValues)
		{
			StringBuilder.Clear();
			StringBuilder.Append(pMessage);
			StringBuilder.Append(" [");

			foreach (TValue value in pValues)
			{
				StringBuilder.Append(value);
				StringBuilder.Append(", ");
			}
			StringBuilder.Remove(StringBuilder.Length - 2, 2);

			StringBuilder.Append("]");
			UnityEngine.Debug.Log(StringBuilder.ToString());
		}

		[Conditional("ENABLE_DEBUG_LOGGING")]
		public static void Log<TValue>(string pMessage, TValue[,] pValues)
		{
			StringBuilder.Clear();
			StringBuilder.Append(pMessage);
			StringBuilder.Append(" { ");

			int xLength = pValues.GetLength(0);
			int yLength = pValues.GetLength(1);
			for (int x = 0; x < xLength; x++)
			{
				StringBuilder.Append("{ ");
				for (int y = 0; y < yLength; y++)
				{
					if (y == yLength - 1)
					{
						StringBuilder.Append($"{pValues[x, y]}" + "}");
					}
					else
					{
						StringBuilder.Append($"{pValues[x, y]}, ");
					}
				}
				if (x < xLength - 1)
				{
					StringBuilder.Append(", ");
				}
			}
			
			StringBuilder.Append(" }");
			LogBasic(StringBuilder.ToString());
		}

		private static string CreateLogMessage(string pMessage, string pMethodName, UnityEngine.Object pObject)
		{
			if (pObject == null)
			{
				return $"[].{pMethodName}() {pMessage}";
			}
			return ColorString($"[{pObject.GetType().FullName}]") + ColorString($"[{pObject.name}]") + $".{pMethodName}() {pMessage}";
		}
		private static string CreateLogMessage(string pMessage, string pMethodName, Type pObject)
		{
			return ColorString($"[{pObject.FullName}]") + $".{pMethodName}() {pMessage}";
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
					color = new Color(CharToFloat01(pString[1]), CharToFloat01(pString[pString.Length - 2]), CharToFloat01(pString[2]));
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
			return (((float)pChar - 32.0f) / 90.0f) % 1.0f;
		}
#endif
		
		public static string ColorString(Color pColor, string pString)
		{
#if UNITY_EDITOR
			StringBuilder.Clear();
			StringBuilder.Append("<color=#");
			StringBuilder.Append(ColorUtility.ToHtmlStringRGBA(pColor));
			StringBuilder.Append(">");
			StringBuilder.Append(pString);
			StringBuilder.Append("</color>");
			return StringBuilder.ToString();
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
			Vector3 top = pCenter + (Vector3.up * pHeight * 0.5f);
			Vector3 bottem = pCenter + (Vector3.down * pHeight * 0.5f);
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
