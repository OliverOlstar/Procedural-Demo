using System.Collections;
using System.Collections.Generic;
using ODev;
using UnityEngine;

public class CheatManager : UpdateableMonoBehaviour
{
	[SerializeField, Range(0.0f, 2.0f)]
	private float m_TimeScale = 1.0f;

	private float m_LastTimeScale = 1.0f;
	private int m_TimeScaleKey = -1;

	protected override void Tick(float pDeltaTime)
	{
		if (m_TimeScale == m_LastTimeScale)
		{
			return;
		}
		m_LastTimeScale = m_TimeScale;
		
		if (m_TimeScaleKey != -1)
		{
			TimeScaleManager.UpdateTimeEvent(m_TimeScaleKey, m_TimeScale, true);
		}
		else
		{
			m_TimeScaleKey = TimeScaleManager.StartTimeEvent(m_TimeScale, true);
		}
	}
}
