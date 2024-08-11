using ODev.Util;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ODev.PoseAnimator
{
	public struct PoseBoneSystem : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<PoseKey> SkeletonKeys;
		[ReadOnly]
		public int SkeletonLength;
		[ReadOnly]
		public NativeArray<PoseAnimation> Animations;
		[ReadOnly]
		public NativeArray<PoseWeight> Weights;
		[ReadOnly]
		public NativeArray<PoseKey> PoseKeys;

		public NativeArray<PoseKey> NextPose;

		public void Execute(int pIndex)
		{
			Vector3 position = Vector3.zero;
			Quaternion rotation = Quaternion.identity;
			Vector3 scale = Vector3.zero;
			
			int firstAnimationIndex = 0;
			for (int i = Animations.Length - 1; i >= 0; i--)
			{
				if (Weights[i].Weight01.ApproximatelyOrGreaterThan(1.0f)) // Weight is max, anything before it will be covered anyways so skip them
				{
					firstAnimationIndex = i;
					break;
				}
			}
			
			for (int i = firstAnimationIndex; i < Animations.Length; i++)
			{
				CalculateAnimationKey(pIndex, i, ref position, ref rotation, ref scale);
			}

			ApplySkeletonKey(pIndex, ref position, ref rotation, ref scale);
			NextPose[pIndex] = NextPose[pIndex].Set(position, rotation, scale);
		}

		private void ApplySkeletonKey(int pIndex, ref Vector3 rPosition, ref Quaternion rRotation, ref Vector3 rScale)
		{
			rPosition += SkeletonKeys[pIndex].Position;
			rRotation = SkeletonKeys[pIndex].Rotation.Add(rRotation);
			rScale += SkeletonKeys[pIndex].Scale;
		}

		private void CalculateAnimationKey(int pIndex, int pAnimationIndex, ref Vector3 rPosition, ref Quaternion rRotation, ref Vector3 rScale)
		{
			float weight01 = Weights[pAnimationIndex].Weight01;
			if (weight01.IsNearZero())
			{
				return;
			}

			float progress01 = Weights[pAnimationIndex].Progress01;
			progress01 = GetClips(progress01, Animations[pAnimationIndex], out int clipIndexA, out int clipIndexB);
			PoseKey keyA = PoseKeys[(clipIndexA * SkeletonLength) + pIndex];
			PoseKey keyB = PoseKeys[(clipIndexB * SkeletonLength) + pIndex];

			Vector3 position = Vector3.LerpUnclamped(keyA.Position, keyB.Position, progress01);
			Quaternion rotation = Quaternion.LerpUnclamped(keyA.Rotation, keyB.Rotation, progress01);
			Vector3 scale = Vector3.LerpUnclamped(keyA.Scale, keyB.Scale, progress01);

			rPosition = Vector3.LerpUnclamped(rPosition, position, weight01);
			rRotation = Quaternion.LerpUnclamped(rRotation, rotation, weight01);
			rScale = Vector3.LerpUnclamped(rScale, scale, weight01);
		}

		private readonly float GetClips(float pProgress01, PoseAnimation pAnimation, out int oClipA, out int oClipB)
		{
			float scaledProgress;
			switch (pAnimation.PlayType)
			{
				case PoseAnimationType.Linear:
					scaledProgress = pProgress01 * (pAnimation.ClipCount - 1);
					oClipA = Mathf.FloorToInt(scaledProgress);
					oClipA = Mathf.Clamp(oClipA, 0, pAnimation.ClipCount - 2);
					scaledProgress -= oClipA;
					oClipA += pAnimation.ClipsStartIndex;
					oClipB = oClipA + 1;
					return scaledProgress;

				case PoseAnimationType.Circular:
					scaledProgress = pProgress01 * pAnimation.ClipCount;
					oClipA = Mathf.FloorToInt(scaledProgress);
					oClipA = oClipA.Loop(pAnimation.ClipCount);
					scaledProgress = scaledProgress.Loop(1.0f);
					oClipA += pAnimation.ClipsStartIndex;
					oClipB = oClipA + 1;
					if (oClipB == pAnimation.ClipCount + pAnimation.ClipsStartIndex)
					{
						oClipB = pAnimation.ClipsStartIndex;
					}
					return scaledProgress;

				default:
					throw new System.NotImplementedException();
			}
		}
	}
}
