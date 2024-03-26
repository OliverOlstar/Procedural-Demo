using System;
using System.Diagnostics;
using UnityEngine;


namespace OliverLoescher
{
	public abstract class Singleton<T> where T : class, new()
	{
		private static T _Instance = null;
		private static ISingleton _InstanceInterface = null;

		public static T Instance
		{
			get
			{
				// Create
				if (_Instance == null)
				{
					_Instance = new T();
					_InstanceInterface = (_Instance is ISingleton i) ? i : null;
				}
				// Access
				if (_InstanceInterface != null)
				{
					_InstanceInterface.OnAccessed();
				}
				return _Instance;
			}
		}

		[Conditional("ENABLE_DEBUG_LOGGING")]
		protected static void Log(string pMessage, string pMethodName) => Util.Debug2.Log(pMessage, pMethodName, typeof(T));
		[Conditional("ENABLE_DEBUG_LOGGING")]
		protected static void LogWarning(string pMessage, string pMethodName) => Util.Debug2.LogWarning(pMessage, pMethodName, typeof(T));
		[Conditional("ENABLE_DEBUG_LOGGING")]
		protected static void LogError(string pMessage, string pMethodName) => Util.Debug2.LogError(pMessage, pMethodName, typeof(T));
		[Conditional("ENABLE_DEBUG_EXCEPTIONS")]
		protected static void LogExeception(string pMessage, string pMethodName) => Util.Debug2.DevException(pMessage, pMethodName, typeof(T));
	}
}