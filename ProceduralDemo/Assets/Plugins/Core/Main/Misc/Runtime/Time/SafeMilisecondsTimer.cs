using UnityEngine;

public class SafeMilisecondsTimer
{
	private double m_EndTime = 0d;

	public SafeMilisecondsTimer() { }
	public SafeMilisecondsTimer(double duration) => InitTimer(duration);

	public void InitTimer(double duration)
	{
		m_EndTime = GetNow() + duration;
	}

	public void ResetTimer()
	{
		m_EndTime = 0d;
	}

	public float GetRemainingTime()
	{
		double remainingTime = m_EndTime - GetNow();
		return Mathf.Max((float)remainingTime, 0.0f);
	}

	public void ApplyDeltaTime(double deltaTime)
	{
		if (m_EndTime < Core.Util.EPSILON)
		{
			return;
		}
		m_EndTime += deltaTime;
	}

	public bool IsDone()
	{
		return GetRemainingTime() < Core.Util.EPSILON;
	}

	protected virtual double GetNow()
	{
		return Core.Chrono.UtcNowMilliseconds;
	}
}