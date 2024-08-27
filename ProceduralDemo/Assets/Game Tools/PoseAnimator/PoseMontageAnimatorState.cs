using ODev.Util;
using UnityEngine;

namespace ODev.PoseAnimator
{
    public struct PoseMontageAnimatorState
    {
		public int Handle;
		public SOPoseMontage Montage;
		public float Time;

		private float m_TimeWeightOffset;

		public readonly float WeightTime => Time + m_TimeWeightOffset;
		public readonly bool IsComplete => Montage == null || WeightTime > Montage.TotalSeconds;

		public PoseWeight GetPoseWeight()
		{
			return new PoseWeight()
			{
				Weight01 = Weight01(),
				Progress01 = Mathf.Clamp01((Time - Montage.StartSeconds) / Montage.Seconds)
			};
		}
		private readonly float Weight01()
		{
			float weightTime = WeightTime;
			if (weightTime < Montage.FadeInSeconds)
			{
				return (weightTime / Montage.FadeInSeconds).Clamp01();
			}
			if (weightTime > Montage.Seconds - Montage.FadeOutSeconds)
			{
				return ((Montage.TotalSeconds - weightTime) / Montage.FadeOutSeconds).Clamp01();
			}
			return 1.0f;
		}

		public void StartFadeOut()
		{
			m_TimeWeightOffset = Montage.TotalSeconds - (Time + Montage.FadeOutSeconds);
			m_TimeWeightOffset = Mathf.Max(0.0f, m_TimeWeightOffset);
		}

		public void Clear()
		{
			Handle = PoseMontageAnimator.NULL_HANDLE;
			Montage = null;
		}
	}

}
