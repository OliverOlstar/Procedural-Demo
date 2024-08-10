using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ODev.PoseAnimator
{
	[System.Serializable]
	public struct PoseAnimation
	{
		public int ClipsStartIndex;
		public int ClipCount;
		public PoseAnimationType PlayType;

		public PoseAnimation(SOPoseAnimation pSource, int pStartIndex)
		{
			ClipsStartIndex = pStartIndex;
			ClipCount = pSource.Clips.Length;
			PlayType = pSource.PlayType;
		}
	}
}