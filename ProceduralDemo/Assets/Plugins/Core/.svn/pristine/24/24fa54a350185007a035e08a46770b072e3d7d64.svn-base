using System;
using UnityEngine;
using System.Collections;


public class SafeTimer
{
	private long m_LastUpdateTime = 0;
	private float m_RemainingTime = -1.0f;

	public SafeTimer() { }

	public SafeTimer(float duration)
	{
		m_RemainingTime = duration;
		m_LastUpdateTime = Core.Chrono.UtcNowTimestamp;
	}

	public void InitTimer(float duration)
	{
		m_RemainingTime = duration;
		m_LastUpdateTime = GetNow();
	}

	public void ResetTimer()
	{
		m_RemainingTime = 0;
	}

	public void Update()
	{
		if (m_RemainingTime < Core.Util.EPSILON)
		{
			return;
		}
		long now = GetNow();
		float delta = now - m_LastUpdateTime;
		m_RemainingTime -= delta;
		m_LastUpdateTime = now;
	}

	public float GetRemainingTime()
	{
		return Mathf.Max(m_RemainingTime, 0.0f);
	}

	public void ApplyDeltaTime(float deltaTime)
	{
		if (m_RemainingTime < Core.Util.EPSILON)
		{
			return;
		}
		m_RemainingTime += deltaTime;
	}

	public bool IsDone()
	{
		return m_RemainingTime < Core.Util.EPSILON;
	}

	protected virtual long GetNow()
	{
		return Core.Chrono.UtcNowTimestamp;
	}
}


public class SafeStopwatch
{
	private long m_LastUpdateTime = 0;
	private long m_DeltaTime = 0;
	public long DeltaTime { get { return m_DeltaTime; } }
	private long m_ElapsedTime;
	public long GetElapsedTime() { return m_ElapsedTime; }

	public long GetUpdatedElapsedTime()
	{
		Update();
		return m_ElapsedTime;
	}

	public long GetUpdatedDeltaTime()
	{
		Update();
		return m_DeltaTime;
	}

	public SafeStopwatch()
	{
		Reset();
	}

	public SafeStopwatch(long elapsedSeconds)
	{
        if (elapsedSeconds < 0)
        {
			throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        }

		m_ElapsedTime = elapsedSeconds;
		m_LastUpdateTime = GetNow();
	}

	public void Update()
	{
		long now = GetNow();
		m_DeltaTime = now - m_LastUpdateTime;
		m_ElapsedTime += m_DeltaTime;
		m_LastUpdateTime = now;
	}

	public void Reset()
	{
		m_LastUpdateTime = GetNow();
		m_DeltaTime = 0;
		m_ElapsedTime = 0;
	}
	
	public void Reset(long elapsedTime)
	{
		if (elapsedTime < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(elapsedTime));
		}
		
		m_LastUpdateTime = GetNow();
		m_DeltaTime = 0;
		m_ElapsedTime = elapsedTime;
	}
	
	protected virtual long GetNow()
	{
		return Core.Chrono.UtcNowTimestamp;
	}
}
