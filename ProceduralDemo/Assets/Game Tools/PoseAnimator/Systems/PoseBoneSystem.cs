using ODev.Util;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ODev.PoseAnimator
{
	public struct PoseBoneSystem : IJobParallelFor
	{
		[ReadOnly] public NativeArray<PoseKey> SkeletonKeys;
		[ReadOnly] public int SkeletonLength;
		[ReadOnly] public NativeArray<PoseAnimation> Animations;
		[ReadOnly] public NativeArray<PoseKey> PoseKeys;

		[ReadOnly] public NativeArray<bool> IsAnimating;
		[ReadOnly] public NativeArray<PoseWeight> Weights;

		[ReadOnly] public NativeArray<PoseAnimation> MontageAnimations;
		[ReadOnly] public NativeArray<PoseKey> MontagePoseKeys;
		[ReadOnly] public NativeArray<PoseWeight> MontageWeights;

		[NativeDisableParallelForRestriction] public NativeArray<PoseKey> NextPose;

		public void Execute(int pAnimatorIndex)
		{
			if (!IsAnimating[pAnimatorIndex])
			{
				return;
			}

			int firstAnimationIndex = 0;
			PoseMontageAnimator.AnimatorHandleToMontageIndexes(pAnimatorIndex, out int activeMontageIndex, out int previousMontageIndex);
			if (!MontageWeights[activeMontageIndex].Progress01.ApproximatelyOrGreaterThan(1.0f) && MontageWeights[activeMontageIndex].Weight01.ApproximatelyOrGreaterThan(1.0f))
			{
				firstAnimationIndex = Animations.Length;
			}
			else
			{
				for (int i = Animations.Length - 1; i >= 0; i--)
				{
					if (Weights[i].Weight01.ApproximatelyOrGreaterThan(1.0f)) // Weight is max, anything before it will be covered anyways so skip them
					{
						firstAnimationIndex = i;
						break;
					}
				}
			}

			for (int boneIndex = 0; boneIndex < SkeletonLength; boneIndex++)
			{
				ExecuteBone(boneIndex, pAnimatorIndex, firstAnimationIndex, activeMontageIndex, previousMontageIndex);
			}
		}

		private void ExecuteBone(int pBoneIndex, int pAnimatorIndex, int pFirstAnimationIndex, int pActiveMontageIndex, int pPreviousMontageIndex)
		{
			int localBoneIndex = (pAnimatorIndex * SkeletonLength) + pBoneIndex;
			Vector3 position = Vector3.zero;
			Quaternion rotation = Quaternion.identity;
			Vector3 scale = Vector3.zero;

			for (int i = pFirstAnimationIndex; i < Animations.Length; i++)
			{
				int localAnimationIndex = i + (pAnimatorIndex * Animations.Length);
				CalculateAnimationKey(pBoneIndex, i, localAnimationIndex, ref position, ref rotation, ref scale);
			}
			CalculateMontageKey(pBoneIndex, pPreviousMontageIndex, ref position, ref rotation, ref scale);
			CalculateMontageKey(pBoneIndex, pActiveMontageIndex, ref position, ref rotation, ref scale);

			ApplySkeletonKey(pBoneIndex, ref position, ref rotation, ref scale);
			NextPose[localBoneIndex] = NextPose[localBoneIndex].Set(position, rotation, scale);
		}

		private void ApplySkeletonKey(int pBoneIndex, ref Vector3 rPosition, ref Quaternion rRotation, ref Vector3 rScale)
		{
			rPosition += SkeletonKeys[pBoneIndex].Position;
			rRotation = SkeletonKeys[pBoneIndex].Rotation.Add(rRotation);
			rScale += SkeletonKeys[pBoneIndex].Scale;
		}

		private void CalculateAnimationKey(int pBoneIndex, int pAnimationIndex, int pLocalAnimationIndex, ref Vector3 rPosition, ref Quaternion rRotation, ref Vector3 rScale)
		{
			float weight01 = Weights[pLocalAnimationIndex].Weight01;
			if (weight01.IsNearZero())
			{
				return;
			}

			float progress01 = Weights[pLocalAnimationIndex].Progress01;
			progress01 = GetClips(progress01, Animations[pAnimationIndex], out int clipIndexA, out int clipIndexB);
			progress01 = Easing.Ease(Animations[pAnimationIndex].Easing, progress01);

			PoseKey keyA = PoseKeys[(clipIndexA * SkeletonLength) + pBoneIndex];
			PoseKey keyB = PoseKeys[(clipIndexB * SkeletonLength) + pBoneIndex];

			Vector3 position = Vector3.LerpUnclamped(keyA.Position, keyB.Position, progress01);
			Quaternion rotation = Quaternion.LerpUnclamped(keyA.Rotation, keyB.Rotation.normalized, progress01);
			Vector3 scale = Vector3.LerpUnclamped(keyA.Scale, keyB.Scale, progress01);

			rPosition = Vector3.LerpUnclamped(rPosition, position, weight01);
			rRotation = Quaternion.LerpUnclamped(rRotation, rotation, weight01);
			rScale = Vector3.LerpUnclamped(rScale, scale, weight01);
		}

		private void CalculateMontageKey(int pBoneIndex, int pMontageIndex, ref Vector3 rPosition, ref Quaternion rRotation, ref Vector3 rScale)
		{
			float weight01 = MontageWeights[pMontageIndex].Weight01;
			if (weight01.IsNearZero())
			{
				return;
			}

			float progress01 = MontageWeights[pMontageIndex].Progress01;
			progress01 = GetClips(progress01, MontageAnimations[pMontageIndex], out int clipIndexA, out int clipIndexB);
			progress01 = Easing.Ease(MontageAnimations[pMontageIndex].Easing, progress01);

			PoseKey keyA = MontagePoseKeys[(clipIndexA * SkeletonLength) + pBoneIndex];
			PoseKey keyB = MontagePoseKeys[(clipIndexB * SkeletonLength) + pBoneIndex];

			Vector3 position = Vector3.LerpUnclamped(keyA.Position, keyB.Position, progress01);
			Quaternion rotation = Quaternion.LerpUnclamped(keyA.Rotation, keyB.Rotation.normalized, progress01);
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
