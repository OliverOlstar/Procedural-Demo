using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher.Util
{
	public class AnimUtilEase2D : Anim.IAnimation
	{
		Easing.EaseParams EaseX;
		Easing.EaseParams EaseY;
		private Anim.Tick2DEvent OnTick;
		private Anim.Tick2DEvent OnComplete;

		private float InverseSeconds;
		private float Progress01;

		public bool IsComplete => Progress01 >= 1.0f;

		public AnimUtilEase2D(Easing.EaseParams pEaseX, Easing.EaseParams pEaseY, float pSeconds, Anim.Tick2DEvent pOnTick, Anim.Tick2DEvent pOnComplete)
		{
			EaseX = pEaseX;
			EaseY = pEaseY;
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
				OnComplete.Invoke(Vector2.one);
				return true;
			}
			OnTick.Invoke(new Vector2(EaseX.Ease(Progress01), EaseY.Ease(Progress01)));
			return false;
		}

		public void Cancel()
		{
			if (!IsComplete)
			{
				Progress01 = 1.0f;
				OnComplete.Invoke(Vector2.one);
			}
		}
	}
}
