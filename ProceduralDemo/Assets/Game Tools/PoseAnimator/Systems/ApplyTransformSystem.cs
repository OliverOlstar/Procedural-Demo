using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Jobs;

namespace ODev.PoseAnimator
{
    public struct ApplyTransformSystem : IJobParallelForTransform
	{
		[ReadOnly]
		public NativeArray<PoseKey> NextPose;

		public void Execute(int pIndex, TransformAccess pBone)
		{
			pBone.SetLocalPositionAndRotation(NextPose[pIndex].Position, NextPose[pIndex].Rotation);
			pBone.localScale = NextPose[pIndex].Scale;
		}
	}
}
