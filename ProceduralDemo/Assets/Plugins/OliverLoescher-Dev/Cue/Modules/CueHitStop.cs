using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

namespace ODev.Cue
{
	[System.Serializable]
    public class CueHitStop : CueModule
	{
		[SerializeField, Tooltip("Duration of the stop")]
		private float m_Seconds = 0.2f;
		[SerializeField, Tooltip("Value to set the time to")]
		private float m_TimeScale = 0.0f;
		[SerializeField, Tooltip("Do fade out of the stop at the end")]
		private bool m_FadeOut = false;

		protected override void PlayInternal(in CueContext pContext, in SOCue pParent)
		{
			TimeScaleManager.StartTimeEvent(m_TimeScale, m_Seconds, false, false, m_FadeOut);
		}
	}
}
