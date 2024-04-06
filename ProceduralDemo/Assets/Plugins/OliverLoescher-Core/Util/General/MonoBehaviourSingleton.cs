using System.Diagnostics;
using UnityEngine;

namespace OliverLoescher
{
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>, new()
    {
        private static T _Instance = null;
        private static ISingleton _InstanceInterface = null;
        public static T Instance
        {
            get
            {
				// Create
				TryCreate();

				// Access
				if (_InstanceInterface != null)
                {
                    _InstanceInterface.OnAccessed();
                }
                return _Instance;
            }
        }

		protected static void TryCreate()
		{
			if (!Util.Func.IsApplicationQuitting && _Instance == null)
			{
				_Instance = new GameObject().AddComponent<T>();
				_Instance.gameObject.name = _Instance.GetType().Name;
				DontDestroyOnLoad(_Instance.gameObject);
				_InstanceInterface = (_Instance is ISingleton i) ? i : null;
			}
		}

		protected virtual void OnDestroy()
		{
			if (_Instance == this)
			{
				_Instance = null;
				_InstanceInterface = null;
			}
		}

		private static GameObject LogContext => _Instance != null ? _Instance.gameObject : null;
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