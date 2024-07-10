using UnityEngine;

namespace OCore.Util
{
	public class AnimUtilEase2D : Anim.IAnimationInternal
	{
		private Easing.EaseParams m_EaseX;
		private Easing.EaseParams m_EaseY;
		private readonly Anim.Tick2DEvent m_OnTick;
		private readonly Anim.Tick2DEvent m_OnComplete;

		private readonly float m_InverseSeconds;
		private float m_Progress01;

		public bool IsComplete => m_Progress01 >= 1.0f;

		public AnimUtilEase2D(Easing.EaseParams pEaseX, Easing.EaseParams pEaseY, float pSeconds, Anim.Tick2DEvent pOnTick, Anim.Tick2DEvent pOnComplete, float pDelay)
		{
			if (pOnTick == null)
			{
				Debug.DevException("pOnTick should never be null", typeof(AnimUtilEase));
			}
			
			m_EaseX = pEaseX;
			m_EaseY = pEaseY;
			m_OnTick = pOnTick;
			m_OnComplete = pOnComplete ?? pOnTick;

			m_InverseSeconds = 1.0f / pSeconds;
			m_Progress01 = -pDelay * m_InverseSeconds;
		}

		bool Anim.IAnimationInternal.Tick(float pDeltaTime)
		{
			if (IsComplete)
			{
				return true;
			}
			m_Progress01 += pDeltaTime * m_InverseSeconds;
			if (IsComplete)
			{
				m_OnComplete.Invoke(Vector2.one);
				return true;
			}
			if (m_Progress01 >= 0.0f)
			{
				m_OnTick.Invoke(new Vector2(m_EaseX.Ease(m_Progress01), m_EaseY.Ease(m_Progress01)));
			}
			return false;
		}

		public void Cancel()
		{
			if (IsComplete)
			{
				return;
			}
			m_Progress01 = 1.0f;
			m_OnComplete.Invoke(Vector2.one);
		}
	}
}
