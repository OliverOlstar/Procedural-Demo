using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace ODev.PoseAnimator
{
    internal static class PoseUtil
    {
		internal struct Bone
		{
			public Transform Transform { get; private set; }
			public int Depth { get; private set; }
			public int Index { get; private set; }

			public Bone(Transform pBone, int pDepth, int pIndex)
			{
				Transform = pBone;
				Depth = pDepth;
				Index = pIndex;
			}
		}

		private static readonly Stack<Bone> m_BoneStack = new();

		internal static IEnumerable<Bone> GetAllBones(Transform pRoot)
		{
			m_BoneStack.Clear();
			m_BoneStack.Push(new Bone(pRoot, 0, 0));

			int skeletonIndex = 0;
			while (m_BoneStack.Count > 0)
			{
				Bone bone = m_BoneStack.Pop();
				yield return bone;
				skeletonIndex++;
				for (int i = 0; i < bone.Transform.childCount; i++)
				{
					m_BoneStack.Push(new Bone(bone.Transform.GetChild(i), bone.Depth + 1, skeletonIndex));
				}
			}
		}

		internal static IEnumerable<Bone> GetAllBones(SOPoseSkeleton pSkeleton, Transform pRoot)
		{
			m_BoneStack.Clear();
			m_BoneStack.Push(new Bone(pRoot, 0, 0));

			int skeletonIndex = 0;
			while (m_BoneStack.Count > 0)
			{
				Bone bone = m_BoneStack.Pop();
				while (bone.Depth != pSkeleton.GetBone(skeletonIndex).Depth)
				{
					Util.Debug.LogError($"Skipping bone of depth {pSkeleton.GetBone(skeletonIndex).Depth}", typeof(PoseUtil));
					skeletonIndex++;
				}
				yield return bone;
				skeletonIndex++;
				for (int i = 0; i < bone.Transform.childCount; i++)
				{
					m_BoneStack.Push(new Bone(bone.Transform.GetChild(i), bone.Depth + 1, skeletonIndex));
				}
			}
		}

		internal static void CopySkeleton(NativeArray<PoseKey> pSkeleton, SOPoseSkeleton pSource)
		{
			if (pSkeleton.Length != pSource.BoneCount)
			{
				Util.Debug.DevException($"The nativeArray skeleton size does not match the source skeleton", typeof(PoseUtil));
				return;
			}

			for (int i = 0; i < pSource.BoneCount; i++)
			{
				pSkeleton[i] = pSource.GetBone(i).Key;
			}
		}
	}
}
