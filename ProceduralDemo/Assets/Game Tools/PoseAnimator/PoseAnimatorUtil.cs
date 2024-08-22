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
			public int Index;

			public Bone(Transform pBone, int pDepth, int pIndex)
			{
				Transform = pBone;
				Depth = pDepth;
				Index = pIndex;
			}

			public override string ToString()
			{
				return $"[{Transform.name}] Index {Index} - Depth {Depth}";
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
				bone.Index = skeletonIndex;
				
				yield return bone;

				for (int i = 0; i < bone.Transform.childCount; i++)
				{
					m_BoneStack.Push(new Bone(bone.Transform.GetChild(i), bone.Depth + 1, -1));
				}
				skeletonIndex++;
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
					Util.Debug.LogError(typeof(PoseUtil), $"Skipping bone of depth {pSkeleton.GetBone(skeletonIndex).Depth}");
					// skeletonIndex++;
				}
				bone.Index = skeletonIndex;

				yield return bone;
				
				for (int i = 0; i < bone.Transform.childCount; i++)
				{
					m_BoneStack.Push(new Bone(bone.Transform.GetChild(i), bone.Depth + 1, -1));
				}
				skeletonIndex++;
			}
		}

		internal static void CopySkeleton(NativeArray<PoseKey> pSkeleton, SOPoseSkeleton pSource)
		{
			if (pSkeleton.Length != pSource.BoneCount)
			{
				Util.Debug.DevException(typeof(PoseUtil), $"The nativeArray skeleton size does not match the source skeleton");
				return;
			}

			for (int i = 0; i < pSource.BoneCount; i++)
			{
				pSkeleton[i] = pSource.GetBone(i).Key;
			}
		}

		internal static void AppendNative<T>(ref NativeArray<T> pArray, T pItem) where T : struct
		{
			if (!pArray.IsCreated)
			{
				pArray = new NativeArray<T>(1, Allocator.Persistent);
				pArray[0] = pItem;
				return;
			}

			var newArray = new NativeArray<T>(pArray.Length + 1, Allocator.Persistent);
			for (int i = 0; i < pArray.Length; i++)
			{
				newArray[i] = pArray[i];
			}
			newArray[^1] = pItem;
			pArray.Dispose();
			pArray = newArray;
		}

		internal static void AppendNative<T>(ref NativeArray<T> pArray, T[] pItems) where T : struct
		{
			if (!pArray.IsCreated)
			{
				pArray = new NativeArray<T>(pItems, Allocator.Persistent);
				return;
			}

			var newArray = new NativeArray<T>(pArray.Length + pItems.Length, Allocator.Persistent);
			for (int i = 0; i < pArray.Length; i++)
			{
				newArray[i] = pArray[i];
			}
			for (int i = 0; i < pItems.Length; i++)
			{
				newArray[pArray.Length + i] = pItems[i];
			}
			pArray.Dispose();
			pArray = newArray;
		}

		internal static void AppendNative<T>(ref NativeArray<T> pArray, List<T> pItems) where T : struct
		{
			if (!pArray.IsCreated)
			{
				pArray = new NativeArray<T>(pItems.ToArray(), Allocator.Persistent);
				return;
			}

			var newArray = new NativeArray<T>(pArray.Length + pItems.Count, Allocator.Persistent);
			for (int i = 0; i < pArray.Length; i++)
			{
				newArray[i] = pArray[i];
			}
			for (int i = 0; i < pItems.Count; i++)
			{
				newArray[pArray.Length + i] = pItems[i];
			}
			pArray.Dispose();
			pArray = newArray;
		}
	}
}
