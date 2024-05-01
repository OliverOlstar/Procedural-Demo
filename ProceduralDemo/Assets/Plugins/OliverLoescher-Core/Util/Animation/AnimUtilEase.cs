using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher.Util
{
    public class AnimUtilEase : Anim.IAnimationInternal
    {
        Easing.EaseParams Ease;
		private readonly Anim.TickEvent OnTick;
		private readonly Anim.TickEvent OnComplete;
		
		private readonly float InverseSeconds;
		private float Progress01;
		
		public bool IsComplete => Progress01 >= 1.0f;

		public AnimUtilEase(Easing.EaseParams pEase, float pSeconds, Anim.TickEvent pOnTick, Anim.TickEvent pOnComplete, float pDelay = 0.0f)
		{
			if (pOnTick == null)
			{
				Debug2.DevException("pOnTick should never be null", "", typeof(AnimUtilEase));
			}

			Ease = pEase;
			OnTick = pOnTick;
			OnComplete = pOnComplete ?? pOnTick;
			
			InverseSeconds = 1.0f / pSeconds;
			Progress01 = -pDelay * InverseSeconds;
		}

		bool Anim.IAnimationInternal.Tick(float pDeltaTime)
		{
			if (IsComplete)
			{
				return true;
			}
			Progress01 += pDeltaTime * InverseSeconds;
			if (IsComplete)
			{
				OnComplete.Invoke(1.0f);
				return true;
			}
			if (Progress01 >= 0.0f)
			{
				OnTick.Invoke(Ease.Ease(Progress01));
			}
			return false;
		}

		public void Cancel()
		{
			if (!IsComplete)
			{
				Progress01 = 1.0f;
				OnComplete.Invoke(1.0f);
			}
		}
    }
}
