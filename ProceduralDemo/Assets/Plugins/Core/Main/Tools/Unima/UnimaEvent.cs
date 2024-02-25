
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public abstract class UnimaEventBase : IUnimaPlayer, System.IComparable
{
	[SerializeField]
	private bool m_NoInvokeOnStop;
	[SerializeField]
	private float m_StartTime = 0.0f;
	public float StartTime => m_StartTime;

	private float m_OffsetStartTime = 0.0f;
	private float m_Timer = 0.0f;
	private bool m_Invoked = false;

	public IUnimaPlayer InstatiatePlayer(GameObject gameObject) => this;

	bool IUnimaPlayer.IsPlaying() => false;

	public abstract bool IsValid();

	protected abstract void Invoke();

	protected UnimaEventBase(float startTime)
	{
		m_StartTime = startTime;
	}

	void IUnimaPlayer.Play(IUnimaContext context, float offsetStartTime)
	{
		m_OffsetStartTime = offsetStartTime;
		float startTime = m_StartTime + m_OffsetStartTime;
		if (Core.Util.Approximately(startTime, 0.0f))
		{
			// Update the flag first. The event could disable us, causing Stop() to be
			// called, which would invoke our event again -- Josh M. 
			m_Invoked = true;
			Invoke();
		}
		else
		{
			m_Timer = 0.0f;
			m_Invoked = false;
		}
	}

	void IUnimaPlayer.UpdatePlaying(float deltaTime)
	{
		if (!m_Invoked && m_StartTime > 0.0f)
		{
			m_Timer += deltaTime;
			float startTime = m_StartTime + m_OffsetStartTime;
			if (m_Timer > startTime)
			{
				// Update the flag first. The event could disable us, causing Stop() to be
				// called, which would invoke our event again -- Josh M. 
				m_Invoked = true;
				Invoke();
			}
		}
	}

	void IUnimaPlayer.Stop()
	{
		if (!m_Invoked && !m_NoInvokeOnStop)
		{
			Invoke();
		}
	}

	public int CompareTo(object obj)
	{
		UnimaEventBase timedItem = obj as UnimaEventBase;
		if (timedItem == null)
		{
			return 0;
		}
		float thisTiming = m_StartTime < 0.0f ? Mathf.Infinity : m_StartTime;
		float otherTiming = timedItem.m_StartTime < 0.0f ? Mathf.Infinity : timedItem.m_StartTime;
		int comparison = thisTiming.CompareTo(otherTiming);
		return comparison;
	}
}

[System.Serializable]
public class UnimaEvent : UnimaEventBase
{
	[SerializeField]
	private UnityEvent m_Event = new UnityEvent();
	public UnityEvent Event => m_Event;

	protected UnimaEvent(float startTime) : base(startTime)
	{

	}

	public override bool IsValid() => true;

	protected override void Invoke()
	{
		m_Event.Invoke();
	}
}

public class UnimaAction : UnimaEventBase
{
	public UnimaAction(float startTime, System.Action action) : base(startTime)
	{
		m_RegisteredAction = action;
	}

	private System.Action m_RegisteredAction;
	public System.Action RegisteredAction => m_RegisteredAction;

	public override bool IsValid() => m_RegisteredAction != null;

	public void UnregisterAction(System.Action action)
	{
		if (m_RegisteredAction != action)
		{
			return;
		}
		m_RegisteredAction = null;
	}

	protected override void Invoke()
	{
		if (m_RegisteredAction != null)
		{
			m_RegisteredAction.Invoke();
		}
	}

	public override string ToString() => 
		$"UnimaEventRuntime({(m_RegisteredAction != null ? m_RegisteredAction.Method.Name : "INVALID")}, {StartTime})";
}
