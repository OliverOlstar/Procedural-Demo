using System.Diagnostics;
using UnityEngine;


namespace OCore
{
	public abstract class Singleton<T> where T : class, new()
	{
		private static T s_Instance = null;
		private static ISingleton s_InstanceInterface = null;

		public static T Instance
		{
			get
			{
				if (s_Instance == null)
				{
					s_Instance = new T();
					s_InstanceInterface = (s_Instance is ISingleton i) ? i : null;
				}
				s_InstanceInterface?.OnAccessed();
				return s_Instance;
			}
		}

		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
		protected static void Log(string pMessage, string pMethodName) => Util.Debug2.Log(pMessage, pMethodName, typeof(T));
		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
		protected static void LogWarning(string pMessage, string pMethodName) => Util.Debug2.LogWarning(pMessage, pMethodName, typeof(T));
		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
		protected static void LogError(string pMessage, string pMethodName) => Util.Debug2.LogError(pMessage, pMethodName, typeof(T));
		[Conditional("ENABLE_DEBUG_EXCEPTIONS"), HideInCallstack]
		protected static void LogExeception(string pMessage, string pMethodName) => Util.Debug2.DevException(pMessage, pMethodName, typeof(T));
	}
}