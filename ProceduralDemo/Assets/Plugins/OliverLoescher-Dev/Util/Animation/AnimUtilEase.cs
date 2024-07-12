namespace ODev.Util
{
	public class AnimUtilEase : Anim.IAnimationInternal
    {
		private Easing.EaseParams m_Ease;
		private readonly Anim.TickEvent m_OnTick;
		private readonly Anim.TickEvent m_OnComplete;
		
		private readonly float m_InverseSeconds;
		private float m_Progress01;
		
		public bool IsComplete => m_Progress01 >= 1.0f;

		public AnimUtilEase(Easing.EaseParams pEase, float pSeconds, Anim.TickEvent pOnTick, Anim.TickEvent pOnComplete, float pDelay = 0.0f)
		{
			if (pOnTick == null)
			{
				Debug.DevException("pOnTick should never be null", typeof(AnimUtilEase));
			}

			m_Ease = pEase;
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
				m_OnComplete.Invoke(1.0f);
				return true;
			}
			if (m_Progress01 >= 0.0f)
			{
				m_OnTick.Invoke(m_Ease.Ease(m_Progress01));
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
			m_OnComplete.Invoke(1.0f);
		}
	}
}
