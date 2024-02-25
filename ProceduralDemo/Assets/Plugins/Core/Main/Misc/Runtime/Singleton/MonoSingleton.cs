using UnityEngine;

namespace Core
{
	public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static T s_Instance;
		private static bool s_ApplicationIsQuitting;

		public static T Instance
		{
			get
			{
				if (s_ApplicationIsQuitting)
				{
					return null;
				}
				if (s_Instance == null)
				{
					s_Instance = FindObjectOfType<T>();
					if (s_Instance == null)
					{
						s_Instance = new GameObject(typeof(T).Name, typeof(T)).GetComponent<T>();
						DontDestroyOnLoad(s_Instance);
					}
				}
				return s_Instance;
			}
		}

		public static bool Exists()
		{
			return s_Instance != null;
		}

		protected virtual void OnDestroy()
		{
			s_ApplicationIsQuitting = false;
			s_Instance = null;
		}

		private void OnApplicationQuit()
		{
			s_ApplicationIsQuitting = true;
			s_Instance = null;
		}
	}
}
