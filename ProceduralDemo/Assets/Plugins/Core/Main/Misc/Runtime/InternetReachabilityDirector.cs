using UnityEngine;

namespace Core
{
	[AutoDirector, PersistentDirector]
	public class InternetReachabilityDirector : IEarlyUpdatable, IDirector
	{
		private static NetworkReachability m_InternetReachability = NetworkReachability.NotReachable;
		public static NetworkReachability InternetReachability
		{
			get
			{
				if (LastCheckedFrame != CurrFrame)
				{
					m_InternetReachability = Application.internetReachability;
					CurrFrame = LastCheckedFrame;
				}
				return m_InternetReachability;
			}
		}

		private static int CurrFrame = int.MinValue;
		private static int LastCheckedFrame = 0;

		public void OnCreate()
		{
			m_InternetReachability = Application.internetReachability;
			Chrono.RegisterEarly(this);
		}
		public void OnDestroy()
		{
			Chrono.DeregisterEarly(this);
		}

		public void OnEarlyUpdate(double deltaTime)
		{
			CurrFrame++;
			if (CurrFrame == int.MaxValue)
			{
				CurrFrame = int.MinValue;
			}
		}

		public void OnRegistered() { }
		public void OnDeregistered() { }
		public double DeltaTime { get{ return 0.0f; } }
	}
}