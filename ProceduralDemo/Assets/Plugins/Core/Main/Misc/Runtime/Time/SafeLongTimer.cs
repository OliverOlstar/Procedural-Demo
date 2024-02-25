using System;

public class SafeLongTimer
{
	private long m_LastUpdateTime = 0;
	private long m_RemainingTime = -1;

	public SafeLongTimer() { }

	public SafeLongTimer(long duration)
	{
		m_RemainingTime = duration;
		m_LastUpdateTime = Core.Chrono.UtcNowTimestamp;
	}

	public void InitTimer(long duration)
	{
		m_RemainingTime = duration;
		m_LastUpdateTime = GetNow();
	}

	public void Update()
	{
		if (m_RemainingTime < Core.Util.EPSILON)
		{
			return;
		}
		long now = GetNow();
		long delta = now - m_LastUpdateTime;
		m_RemainingTime -= delta;
		m_LastUpdateTime = now;
	}

	public long GetRemainingTime()
	{
		return Math.Max(m_RemainingTime, 0);
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
