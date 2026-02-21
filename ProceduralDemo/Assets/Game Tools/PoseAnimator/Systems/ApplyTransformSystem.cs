using Unity.Collections;
using UnityEngine.Jobs;

namespace ODev.PoseAnimator
{
    public struct ApplyTransformSystem : IJobParallelForTransform
	{
		[ReadOnly] public NativeArray<PoseKey> NextPose;

		public void Execute(int pIndex, TransformAccess pBone)
		{
			bool isPositionDifferent = pBone.localPosition != NextPose[pIndex].Position;
			bool isRotationDifferent = pBone.localRotation != NextPose[pIndex].Rotation;
			bool isScaleDifferent = pBone.localScale != NextPose[pIndex].Scale;

			if (isPositionDifferent)
			{
				if (isRotationDifferent)
				{
					pBone.SetLocalPositionAndRotation(NextPose[pIndex].Position, NextPose[pIndex].Rotation);
				}
				else
				{
					pBone.localPosition = NextPose[pIndex].Position;
				}
			}
			else if (isRotationDifferent)
			{
				pBone.localRotation = NextPose[pIndex].Rotation;
			}

			if (isScaleDifferent)
			{
				pBone.localScale = NextPose[pIndex].Scale;
			}
		}
	}
}
