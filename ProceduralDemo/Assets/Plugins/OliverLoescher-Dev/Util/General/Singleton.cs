using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace ODev
{
	public abstract class Singleton<T> where T : class, new()
	{
		private static T s_Instance = null;
		private static ISingleton s_InstanceInterface = null;

		public static T Instance
		{
			get
			{
				if (Util.Func.IsApplicationQuitting)
				{
					return null;
				}
				if (s_Instance == null)
				{
					s_Instance = new T();
					if (s_Instance is Singleton<T> instance)
					{
						instance.OnStart();
					}
					s_InstanceInterface = (s_Instance is ISingleton i) ? i : null;
					Application.quitting += Destroy;
				}
				s_InstanceInterface?.OnAccessed();
				return s_Instance;
			}
		}

		private static void Destroy()
		{
			if (s_Instance == null)
			{
				return;
			}
			if (s_Instance is Singleton<T> instance)
			{
				instance.OnDestroy();
			}
			s_Instance = null;
			s_InstanceInterface = null;
			Application.quitting -= Destroy;
		}

		protected virtual void OnStart() { }
		protected virtual void OnDestroy() { }

		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
		protected static void Log(string pMessage, [CallerMemberName] string pMethodName = "") => Util.Debug.Log(pMessage, typeof(T), pMethodName);
		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
		protected static void LogWarning(string pMessage, [CallerMemberName] string pMethodName = "") => Util.Debug.LogWarning(pMessage, typeof(T), pMethodName);
		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
		protected static void LogError(string pMessage, [CallerMemberName] string pMethodName = "") => Util.Debug.LogError(pMessage, typeof(T), pMethodName);
		[Conditional("ENABLE_DEBUG_EXCEPTIONS"), HideInCallstack]
		protected static void DevException(string pMessage, [CallerMemberName] string pMethodName = "") => Util.Debug.DevException(pMessage, typeof(T), pMethodName);
		[Conditional("ENABLE_DEBUG_EXCEPTIONS"), HideInCallstack]
		protected static void DevException<TException>(TException pException, [CallerMemberName] string pMethodName = "") where TException : Exception => Util.Debug.DevException<TException>(pException, typeof(T), pMethodName);
	}
}