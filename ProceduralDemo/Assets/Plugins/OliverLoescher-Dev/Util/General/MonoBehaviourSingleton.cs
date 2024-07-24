using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ODev
{
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>, new()
    {
        private static T s_Instance = null;
        private static ISingleton s_InstanceInterface = null;
		
		protected virtual new bool DontDestroyOnLoad => false;

        public static T Instance
        {
            get
            {
				s_InstanceInterface?.OnAccessed();
                return s_Instance;
            }
        }

		public static bool Exists => s_Instance != null;

		protected virtual void Awake()
		{
			if (s_Instance != null)
			{
				LogExeception("There is already an instance of this, destroying self");
				Destroy(this);
				return;
			}
			s_Instance = this as T;
			if (s_Instance.DontDestroyOnLoad)
			{
				DontDestroyOnLoad(gameObject);
			}
		}

		protected virtual void OnDestroy()
		{
			if (s_Instance != this)
			{
				return;
			}
			s_Instance = null;
			s_InstanceInterface = null;
		}

		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
        protected static void Log(string pMessage, [CallerMemberName] string pMethodName = "") => Util.Debug.Log(pMessage, s_Instance, pMethodName);
		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
        protected static void LogWarning(string pMessage, [CallerMemberName] string pMethodName = "") => Util.Debug.LogWarning(pMessage, s_Instance, pMethodName);
		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
        protected static void LogError(string pMessage, [CallerMemberName] string pMethodName = "") => Util.Debug.LogError(pMessage, s_Instance, pMethodName);
		[Conditional("ENABLE_DEBUG_EXCEPTIONS"), HideInCallstack]
        protected static void LogExeception(string pMessage, [CallerMemberName] string pMethodName = "") => Util.Debug.DevException(pMessage, s_Instance, pMethodName);
    }
}