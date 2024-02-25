using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher.Util
{
    public class AnimUtilEase : Anim.IAnimation
    {
        Easing.EaseParams Ease;
		private Anim.TickEvent OnTick;
		private Anim.TickEvent OnComplete;
		
		private float InverseSeconds;
		private float Progress01;
		
		public bool IsComplete => Progress01 >= 1.0f;

		public AnimUtilEase(Easing.EaseParams pEase, float pSeconds, Anim.TickEvent pOnTick, Anim.TickEvent pOnComplete)
		{
			Ease = pEase;
			OnTick = pOnTick;
			OnComplete = pOnComplete == null ? pOnTick : pOnComplete;
			
			InverseSeconds = 1.0f / pSeconds;
			Progress01 = 0.0f;
		}

		bool Anim.IAnimation.Tick(float pDeltaTime)
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
			OnTick.Invoke(Ease.Ease(Progress01));
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
