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
		public NativeArray<PoseKey> PoseKeys;

		public NativeArray<PoseKey> NextPose;

		public void Execute(int pIndex)
		{
			float progress01 = Animations[0].Progress;
			progress01 = GetClips(progress01, 3, out int clipAIndex, out int clipBIndex);
			PoseKey keyA = PoseKeys[(clipAIndex * SkeletonLength) + pIndex];
			PoseKey keyB = PoseKeys[(clipBIndex * SkeletonLength) + pIndex];

			Vector3 position = Vector3.LerpUnclamped(keyA.Position, keyB.Position, progress01);
			Quaternion rotation = Quaternion.LerpUnclamped(keyA.Rotation, keyB.Rotation, progress01);
			Vector3 scale = Vector3.LerpUnclamped(keyA.Scale, keyB.Scale, progress01);

			position += SkeletonKeys[pIndex].Position;
			rotation = SkeletonKeys[pIndex].Rotation * rotation;
			scale += SkeletonKeys[pIndex].Scale;

			NextPose[pIndex] = NextPose[pIndex].Set(position, rotation, scale);
		}

		private readonly float GetClips(float pProgress01, int pClipCount, out int oClipA, out int oClipB)
		{
			float scaledProgress = pProgress01 * (pClipCount - 1);
			oClipA = Mathf.FloorToInt(scaledProgress);
			oClipA = Mathf.Clamp(oClipA, 0, pClipCount - 2);
			oClipB = oClipA + 1;
			return scaledProgress - oClipA;
		}
	}
}
