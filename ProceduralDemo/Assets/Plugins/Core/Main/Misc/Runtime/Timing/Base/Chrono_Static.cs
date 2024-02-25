using System;
using System.Globalization;
using UnityEngine;

namespace Core
{
	public sealed partial class Chrono : MonoBehaviour
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			new GameObject("Chrono").AddComponent<Core.Chrono>();
		}

		public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static DateTime UtcNow => DateTime.UtcNow.AddMilliseconds(s_OffsetMS);
		public static long UtcNowTimestamp => DateTimeToTimestamp(UtcNow);
		public static DateTime UtcStartTime { get; private set; }
		public static TimeSpan UtcTimeSinceStart => UtcNow - UtcStartTime;
		public static double SecondsSinceStart { get { return (UtcNow - UtcStartTime).TotalSeconds; } }
		public static double UtcNowMilliseconds => (UtcNow - Epoch).TotalMilliseconds;
		public static float ShaderTime => s_Instance.m_ShaderTimeSinceStart;

        public const int SecondsPerMinute = 60;
		public const int SecondsPerHour = SecondsPerMinute * HoursPerDay;
		public const int SecondsPerDay = SecondsPerHour * HoursPerDay;
		public const int MinutesPerHour = 60;
		public const int MinutesPerDay = MinutesPerHour * HoursPerDay;
		public const int HoursPerDay = 24;
		public const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";

		private static Chrono s_Instance;
		private static double s_OffsetMS;
		public static void SetTimeOffsetMS(double milliseconds) => s_OffsetMS = milliseconds;

		public static void Reset()
		{
			s_OffsetMS = 0.0;
			s_Instance.OnReset();
			UtcStartTime = UtcNow;
		}

		public static void ResetShaderTime()
		{
			s_Instance.SetShaderTime(0f);
		}

		public static DateTime TimestampToDateTime(double timestamp)
		{
			return Epoch.AddSeconds(timestamp);
		}

		public static string TimestampToString(double timestamp)
		{
			return TimestampToDateTime(timestamp).ToString(DateTimeFormat, CultureInfo.InvariantCulture);
		}

		public static long DateTimeToTimestamp(DateTime dateTime)
		{
			return (long)(dateTime - Epoch).TotalSeconds;
		}

		public static string DateTimeToString(DateTime dateTime)
		{
			return dateTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
		}

		public static DateTime StringToDateTime(string str)
		{
			return DateTime.ParseExact(str, DateTimeFormat, CultureInfo.InvariantCulture);
		}

		public static long StringToTimestamp(string str)
		{
			return DateTimeToTimestamp(StringToDateTime(str));
		}

		public static DateTime GetPreviousUtcAlignedTimeSlice(TimeSpan timeSlice, DateTime now)
		{
			long nowTimestamp = DateTimeToTimestamp(now);
			long timeToNow = nowTimestamp % SecondsPerDay;
			long midnight = nowTimestamp - timeToNow;
			long intervals = (long)(timeToNow / timeSlice.TotalSeconds);
			double timestamp = midnight + (intervals * timeSlice.TotalSeconds);
			return TimestampToDateTime(timestamp);
		}

		public static DateTime GetPreviousUtcAlignedTimeSlice(TimeSpan timeSlice)
		{
			return GetPreviousUtcAlignedTimeSlice(timeSlice, UtcNow);
		}

		public static DateTime GetNextUtcAlignedTimeSlice(TimeSpan timeSlice, DateTime now)
		{
			return GetPreviousUtcAlignedTimeSlice(timeSlice, now).Add(timeSlice);
		}

		public static DateTime GetNextUtcAlignedTimeSlice(TimeSpan timeSlice)
		{
			return GetNextUtcAlignedTimeSlice(timeSlice, UtcNow);
		}

		public static void RegisterEarly(IEarlyUpdatable updatable, int sorting = 0)
		{
			s_Instance.m_EarlyUpdatables.Register(updatable, sorting);
		}

		public static void DeregisterEarly(IEarlyUpdatable updatable)
		{
			s_Instance.m_EarlyUpdatables.Deregister(updatable);
		}

		public static void Register(IUpdatable updatable, int sorting = 0)
		{
			s_Instance.m_Updatables.Register(updatable, sorting);
		}

		public static void Deregister(IUpdatable updatable)
		{
			s_Instance.m_Updatables.Deregister(updatable);
		}

		public static void RegisterLate(ILateUpdatable updatable, int sorting = 0)
		{
			s_Instance.m_LateUpdatables.Register(updatable, sorting);
		}

		public static bool DeregisterLate(ILateUpdatable updatable)
		{
			return s_Instance.m_LateUpdatables.Deregister(updatable);
		}

		public static void RegisterFixed(IFixedUpdatable updatable, int sorting = 0)
		{
			s_Instance.m_FixedUpdatables.Register(updatable, sorting);
		}

		public static bool DeregisterFixed(IFixedUpdatable updatable)
		{
			return s_Instance.m_FixedUpdatables.Deregister(updatable);
		}
	}
}
