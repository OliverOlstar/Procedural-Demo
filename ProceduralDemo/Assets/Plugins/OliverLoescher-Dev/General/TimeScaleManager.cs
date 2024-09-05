using System;
using UnityEngine;

namespace ODev
{
	public class TimeScaleManager : MonoBehaviourSingletonAuto<TimeScaleManager>
	{
		public class TimeEvent
		{
			public float m_Timer;

			private readonly int m_Handle;
			private readonly float m_Scale;
			private readonly float m_Time;

			private readonly bool m_Timed;
			private readonly bool m_ScaleTimer;
			private readonly bool m_AffectsAudio;
			private readonly bool m_FadeOut;

			public TimeEvent(int handle, float scale, bool affectsAudio = false)
			{
				m_Handle = handle;
				m_Scale = scale;
				m_Timed = false;
				m_ScaleTimer = false;
				m_Timer = -1.0f;
				m_AffectsAudio = affectsAudio;
				m_FadeOut = false;
			}

			public TimeEvent(int handle, float scale, float duration, bool scaleTimer, bool affectsAudio = false, bool fadeOut = false)
			{
				m_Handle = handle;
				m_Scale = scale;
				m_Timed = true;
				m_ScaleTimer = scaleTimer;
				m_Time = m_Timer = duration;
				m_AffectsAudio = affectsAudio;
				m_FadeOut = fadeOut;
			}

			public int GetHandle()
			{
				return m_Handle;
			}

			public float GetScale()
			{
				if (m_FadeOut)
				{
					float delta = 1.0f - Mathf.Clamp01(m_Timer / m_Time);
					delta *= delta;
					delta = 1.0f - delta;
					return Mathf.Lerp(1.0f, m_Scale, delta);
				}
				return m_Scale;
			}

			public bool IsTimed()
			{
				return m_Timed;
			}

			public bool IsTimerScaled()
			{
				return m_ScaleTimer;
			}

			public bool ShouldAffectAudio()
			{
				return m_AffectsAudio;
			}
		}

		public static readonly int INVALID_HANDLE = -11;

		private readonly TimeEvent[] m_CurrentTimeEvents = new TimeEvent[10]; // Time scale at index 0 is the one currently active
		private int m_EndIndex = -1;
		private int m_FreeHandles = 0;
		private float m_PreviousRealTimeSinceStartup = 0.0f;
		private float m_RealDeltaTime = 0.0f;
		private float m_BaseTimeScale = 1.0f;
		private float m_BaseFixedTimeScale = 0.02f;
		private bool m_Paused = false;
		private bool m_FirstFramePaused = false;
		private bool m_AffectAudio = false;

		private int m_EditorSlowMo = 100;
		public int EditorSlowMo => m_EditorSlowMo;

		void Start()
		{
			m_BaseFixedTimeScale = Time.fixedDeltaTime;
			m_PreviousRealTimeSinceStartup = Time.realtimeSinceStartup;
		}

		static void PressedResume() => Instance.UnPause();
		static void PressedPause() => Instance.Pause();

		public void EditorInc()
		{
			if (m_EditorSlowMo >= 100)
			{
				m_EditorSlowMo += 100;
			}
			else if (m_EditorSlowMo >= 10)
			{
				m_EditorSlowMo += 10;
			}
			else if (m_EditorSlowMo >= 5)
			{
				m_EditorSlowMo += 5;
			}
			else if (m_EditorSlowMo >= 1)
			{
				m_EditorSlowMo += 1;
			}
		}

		public void EditorDec()
		{
			if (m_EditorSlowMo > 100)
			{
				m_EditorSlowMo -= 100;
			}
			else if (m_EditorSlowMo > 10)
			{
				m_EditorSlowMo -= 10;
			}
			else if (m_EditorSlowMo > 5)
			{
				m_EditorSlowMo -= 5;
			}
			else if (m_EditorSlowMo > 2)
			{
				m_EditorSlowMo -= 1;
			}
		}

		void Update()
		{
			m_FirstFramePaused = false;

			for (int i = 0; i <= m_EndIndex; i++)
			{
				if (!m_CurrentTimeEvents[i].IsTimed())
				{
					continue;
				}
				if (m_CurrentTimeEvents[i].IsTimerScaled())
				{
					m_CurrentTimeEvents[i].m_Timer -= Time.deltaTime;
				}
				else
				{
					m_CurrentTimeEvents[i].m_Timer -= RealDeltaTime();
				}
				if (m_CurrentTimeEvents[i].m_Timer <= 0.0f)
				{
					EndTimeEvent(m_CurrentTimeEvents[i].GetHandle());
				}
			}

			ResetTimeScale();

			m_RealDeltaTime = (Time.realtimeSinceStartup - m_PreviousRealTimeSinceStartup) * m_BaseTimeScale;
			m_PreviousRealTimeSinceStartup = Time.realtimeSinceStartup;
		}

		public float RealDeltaTime()
		{
			if (m_Paused && !m_FirstFramePaused)
			{
				return 0.0f;
			}
			return m_RealDeltaTime;
		}

		public static float GetRealDeltaTime()
		{
			if (!Exists)
			{
				return Time.deltaTime;
			}
			return Instance.RealDeltaTime();
		}

		public void Pause()
		{
			m_Paused = true;
			ActualPause();
		}

		public void UnPause()
		{
			m_Paused = false;
			ResetTimeScale();
		}

		void ActualPause()
		{
			m_FirstFramePaused = true;
			Time.timeScale = 0.0f;
		}

		void ResetTimeScale()
		{
			if (m_Paused)
			{
				m_AffectAudio = false;
				return;
			}

			float timeScale = m_EndIndex == -1 ? 1.0f : m_CurrentTimeEvents[0].GetScale();
			m_AffectAudio = m_EndIndex == -1 ? false : m_CurrentTimeEvents[0].ShouldAffectAudio();
			// Go with slowest time event
			for (int i = 1; i <= m_EndIndex; i++)
			{
				if (m_CurrentTimeEvents[i].GetScale() >= timeScale)
				{
					continue;
				}
				timeScale = m_CurrentTimeEvents[i].GetScale();
				m_AffectAudio = m_CurrentTimeEvents[i].ShouldAffectAudio();
			}

			timeScale *= m_BaseTimeScale;
			timeScale *= m_EditorSlowMo / 100.0f;
			Time.timeScale = timeScale;
			Time.fixedDeltaTime = m_BaseFixedTimeScale * timeScale;
		}

		public int GetHandle()
		{
			for (int i = 0; i < 10; i++)
			{
				int mask = 1 << i;
				if ((mask & m_FreeHandles) != 0)
				{
					continue;
				}
				m_FreeHandles = mask | m_FreeHandles;
				return -1 - i;
			}
			LogError("There are no free handles");
			return INVALID_HANDLE;
		}

		public static void SetBaseTimeScale(float timeScale)
		{
			if (timeScale < Util.Math.NEARZERO)
			{
				LogError($"Time scale {timeScale} is invalid. Base time scale cannot be <= 0.");
			}
			if (!Exists)
			{
				LogWarning("TimeScaleManager is null.");
				return;
			}
			TimeScaleManager instance = Instance;
			instance.m_BaseTimeScale = timeScale;
			instance.ResetTimeScale();
		}

		public static float GetBaseTimeScale()
		{
			if (!Exists)
			{
				LogWarning("TimeScaleManager is null.");
				return 1.0f;
			}
			return Instance.m_BaseTimeScale;
		}

		public static float GetAudioTimeScale()
		{
			if (!Exists)
			{
				LogWarning("TimeScaleManager is null.");
				return 1.0f;
			}
			TimeScaleManager tem = Instance;
			if (tem.m_AffectAudio)
			{
				return Time.timeScale;
			}
			else
			{
				return tem.m_BaseTimeScale * (tem.m_EditorSlowMo / 100.0f);
			}
		}

		public static int StartTimeEvent(float timeScale, bool affectsAudio = false)
		{
			int handle = Instance.GetHandle();
			TimeEvent timeEvent = new(handle, timeScale, affectsAudio);
			CreateTimeEvent(timeEvent);
			Log($"Starting time event: {handle}");
			return handle;
		}

		public static int StartTimeEvent(float timeScale, float duration, bool scaleTimer, bool affectsAudio = false, bool fadeOut = false)
		{
			int handle = Instance.GetHandle();
			TimeEvent timeEvent = new(handle, timeScale, duration, scaleTimer, affectsAudio, fadeOut);
			CreateTimeEvent(timeEvent);
			Log($"Starting time event: {handle}");
			return handle;
		}

		public static void UpdateTimeEvent(int handle, float newTimeScale, bool affectAudio = false)
		{
			TimeScaleManager instance = Instance;
			if (instance.m_EndIndex == -1)
			{
				return;
			}
			int index = -1;
			for (int i = 0; i <= instance.m_EndIndex; i++)
			{
				if (instance.m_CurrentTimeEvents[i].GetHandle() == handle)
				{
					index = i;
					break;
				}
			}
			if (index == -1)
			{
				return;
			}
			instance.m_CurrentTimeEvents[index] = new TimeEvent(handle, newTimeScale, affectAudio);
		}

		public static void EndTimeEvent(int handle)
		{
			TimeScaleManager instance = Instance;
			if (instance.m_EndIndex == -1)
			{
				return;
			}
			int index = -1;
			for (int i = 0; i <= instance.m_EndIndex; i++)
			{
				if (instance.m_CurrentTimeEvents[i].GetHandle() == handle)
				{
					index = i;
					break;
				}
			}
			if (index == -1)
			{
				return;
			}
			for (int i = index; i < instance.m_EndIndex; i++)
			{
				instance.m_CurrentTimeEvents[i] = instance.m_CurrentTimeEvents[i + 1];
			}
			instance.m_EndIndex--;

			if (handle < 0)
			{
				int mask = 1 << Mathf.Abs(handle + 1);
				instance.m_FreeHandles ^= mask;
			}

			instance.ResetTimeScale();
			// Log($"Ending time event: {handle}");
		}

		public static void EndAllTimeEvents()
		{
			if (!Exists)
			{
				LogWarning("TimeScaleManager is null.");
				return;
			}

			TimeScaleManager instance = Instance;
			if (instance.m_EndIndex == -1)
			{
				return;
			}
			for (int i = instance.m_EndIndex; i >= 0; i--)
			{
				EndTimeEvent(instance.m_CurrentTimeEvents[i].GetHandle());
			}
		}

		static void CreateTimeEvent(TimeEvent timeEvent)
		{
			TimeScaleManager instance = Instance;
			if (instance.m_EndIndex == instance.m_CurrentTimeEvents.Length - 1)
			{
				LogError($"Attempting to start new timeevent {timeEvent.GetHandle()} when max timeevents are active");
				return;
			}
			if (timeEvent == null)
			{
				LogError("Time event that is being started is null");
				return;
			}

			for (int i = instance.m_EndIndex; i >= 0; i--)
			{
				instance.m_CurrentTimeEvents[i + 1] = instance.m_CurrentTimeEvents[i];
			}

			instance.m_CurrentTimeEvents[0] = timeEvent;
			instance.m_EndIndex++;

			instance.ResetTimeScale();
		}
	}
}

