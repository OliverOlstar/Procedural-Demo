using System.Diagnostics;
using System.Runtime.CompilerServices;
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
		protected static void Log(string pMessage, [CallerMemberName] string pMethodName = "") => Util.Debug.Log(pMessage, typeof(T), pMethodName);
		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
		protected static void LogWarning(string pMessage, [CallerMemberName] string pMethodName = "") => Util.Debug.LogWarning(pMessage, typeof(T), pMethodName);
		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
		protected static void LogError(string pMessage, [CallerMemberName] string pMethodName = "") => Util.Debug.LogError(pMessage, typeof(T), pMethodName);
		[Conditional("ENABLE_DEBUG_EXCEPTIONS"), HideInCallstack]
		protected static void LogExeception(string pMessage, [CallerMemberName] string pMethodName = "") => Util.Debug.DevException(pMessage, typeof(T), pMethodName);
	}
}