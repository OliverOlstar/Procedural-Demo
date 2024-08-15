using System.Collections;
using System.Collections.Generic;
using ODev;
using UnityEngine;

public class CheatManager : MonoBehaviour
{
	[SerializeField, Range(0.0f, 2.0f)]
	private float m_TimeScale = 1.0f;

	private int m_TimeScaleKey = -1;

    private void OnValidate()
	{
		if (!Application.isPlaying)
		{
			return;
		}
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
