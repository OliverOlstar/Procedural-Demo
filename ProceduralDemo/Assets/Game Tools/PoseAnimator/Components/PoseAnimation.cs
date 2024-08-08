using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ODev.PoseAnimator
{
	[System.Serializable]
	public struct PoseAnimation
	{
		public int ClipIndexA;
		public int ClipIndexB;
		public int ClipIndexC;
		public int ClipIndexD;
		public int ClipIndexE;

		public int WeightIndex;
		// public AnimationPlayType Type = AnimationPlayType.Linear;

		public float Progress; // TODO be set from outside, should be it's own component?

		public PoseAnimation ModifyProgress(float pDelta)
		{
			Progress += pDelta;
			return this;
		}

		public PoseAnimation SetProgress(float pProgress)
		{
			Progress = pProgress;
			return this;
		}
	}
}
