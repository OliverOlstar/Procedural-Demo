using System.Diagnostics;
using UnityEngine;

namespace OCore
{
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>, new()
    {
        private static T s_Instance = null;
        private static ISingleton s_InstanceInterface = null;
		
        public static T Instance
        {
            get
            {
				// Create
				TryCreate();

				// Access
				s_InstanceInterface?.OnAccessed();
                return s_Instance;
            }
        }

		protected static void TryCreate()
		{
			if (!Util.Func.s_IsApplicationQuitting && s_Instance == null)
			{
				s_Instance = new GameObject().AddComponent<T>();
				s_Instance.gameObject.name = s_Instance.GetType().Name;
				DontDestroyOnLoad(s_Instance.gameObject);
				s_InstanceInterface = (s_Instance is ISingleton i) ? i : null;
			}
		}

		protected virtual void OnDestroy()
		{
			if (s_Instance == this)
			{
				s_Instance = null;
				s_InstanceInterface = null;
			}
		}

		private static GameObject LogContext => s_Instance?.gameObject;
		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
        protected static void Log(string pMessage, string pMethodName) => Util.Debug2.Log(pMessage, pMethodName, LogContext);
		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
        protected static void LogWarning(string pMessage, string pMethodName) => Util.Debug2.LogWarning(pMessage, pMethodName, LogContext);
		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
        protected static void LogError(string pMessage, string pMethodName) => Util.Debug2.LogError(pMessage, pMethodName, LogContext);
		[Conditional("ENABLE_DEBUG_EXCEPTIONS"), HideInCallstack]
        protected static void LogExeception(string pMessage, string pMethodName) => Util.Debug2.DevException(pMessage, pMethodName, LogContext);
    }
}