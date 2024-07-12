using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ODev
{
    public abstract class MonoBehaviourSingletonAuto<T> : MonoBehaviour where T : MonoBehaviourSingletonAuto<T>, new()
    {
        private static T s_Instance = null;
        private static ISingleton s_InstanceInterface = null;
		
        public static T Instance
        {
            get
            {
				TryCreate();
				s_InstanceInterface?.OnAccessed();
                return s_Instance;
            }
        }

		public static bool Exists => s_Instance != null;

		protected static void TryCreate()
		{
			if (Util.Func.IsApplicationQuitting || s_Instance != null)
			{
				return;
			}

			s_Instance = new GameObject().AddComponent<T>();
			DontDestroyOnLoad(s_Instance.gameObject);
			s_InstanceInterface = (s_Instance is ISingleton i) ? i : null;
			if (!Util.Func.IsRelease())
			{
				s_Instance.gameObject.name = s_Instance.GetType().Name;
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