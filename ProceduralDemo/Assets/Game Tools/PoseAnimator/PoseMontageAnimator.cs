using Unity.Collections;
using ODev.Util;
using System;

namespace ODev.PoseAnimator
{
	[Serializable]
	public class PoseMontageAnimator
	{
		public const int NULL_HANDLE = int.MinValue;

		public const int MAX_MONTAGE_COUNT = 2; // Active, Previous
		public const int MAX_MONTAGE_POSE_COUNT = 8;
		public const int MAX_MONTAGE_POSE_COUNT_PER_ANIMATOR = MAX_MONTAGE_POSE_COUNT * MAX_MONTAGE_COUNT;

		public const int ACTIVE_MONTAGE_INDEX = 1;
		public const int PREVIOUS_MONTAGE_INDEX = 0;

		private PoseMontageAnimatorState[] m_MontageStates;
		private NativeArray<PoseAnimation> m_Animations;
		private NativeArray<PoseWeight> m_PoseWeights;
		private NativeArray<PoseKey> m_PoseKeys;
		private int m_SkeletonKeyCount;
		private int m_NextHandle = NULL_HANDLE + 1;

		public NativeArray<PoseAnimation> Animations => m_Animations;
		public NativeArray<PoseWeight> PoseWeights => m_PoseWeights;
		public NativeArray<PoseKey> PoseKeys => m_PoseKeys;

		public void Initalize(int pSkeletonKeyCount, int pBatchSize)
		{
			m_SkeletonKeyCount = pSkeletonKeyCount;
			m_MontageStates = new PoseMontageAnimatorState[MAX_MONTAGE_COUNT * pBatchSize];
			m_Animations = new(MAX_MONTAGE_COUNT * pBatchSize, Allocator.Persistent);
			m_PoseKeys = new(MAX_MONTAGE_POSE_COUNT_PER_ANIMATOR * m_SkeletonKeyCount * pBatchSize, Allocator.Persistent);
			m_PoseWeights = new(MAX_MONTAGE_COUNT * pBatchSize, Allocator.Persistent);
		}

		public void ResizeArrays(int pNewBatchSize)
		{
			Array.Resize(ref m_MontageStates, MAX_MONTAGE_COUNT * pNewBatchSize);
			PoseUtil.ResizeNative(ref m_Animations, MAX_MONTAGE_COUNT * pNewBatchSize);
			PoseUtil.ResizeNative(ref m_PoseKeys, MAX_MONTAGE_POSE_COUNT_PER_ANIMATOR * m_SkeletonKeyCount * pNewBatchSize);
			PoseUtil.ResizeNative(ref m_PoseWeights, MAX_MONTAGE_COUNT * pNewBatchSize);
		}

		public void Dispose()
		{
			m_MontageStates = null;
			m_Animations.Dispose();
			m_PoseKeys.Dispose();
			m_PoseWeights.Dispose();
		}

		public int PlayMontage(int pAnimatorHandle, SOPoseMontage pMontage)
		{
			if (pMontage == null)
			{
				this.LogError("Cannot play a null montage");
				return NULL_HANDLE;
			}

			int montageHandle = ++m_NextHandle;
			if (m_NextHandle == int.MaxValue)
			{
				m_NextHandle = NULL_HANDLE + 1;
			}

			AnimatorHandleToMontageIndexes(pAnimatorHandle, out int activeMontageIndex, out int previousMontageIndex);
			SetActiveMontageToPrevious(activeMontageIndex, previousMontageIndex);
			SetActiveMontage(pMontage, montageHandle, activeMontageIndex);
			// this.Log(pMontage.name);

			return montageHandle;
		}

		private void SetActiveMontage(SOPoseMontage pMontage, int pMontageHandle, int pActiveMontageIndex)
		{
			m_Animations[pActiveMontageIndex] = new(pMontage.Animation, pActiveMontageIndex * MAX_MONTAGE_POSE_COUNT);
			// m_Weights[activeMontageIndex] = new PoseWeight(0.0f);

			int montageStartIndex = m_SkeletonKeyCount * MAX_MONTAGE_POSE_COUNT * pActiveMontageIndex;
			for (int m = 0; m < pMontage.Animation.Clips.Length; m++)
			{
				SOPoseAnimation.AnimationClip clip = pMontage.Animation.Clips[m];
				int clipStartIndex = (m * m_SkeletonKeyCount) + montageStartIndex;
				// this.Log($"clipStartIndex {clipStartIndex}");
				for (int i = 0; i < m_SkeletonKeyCount; i++)
				{
					PoseKey key = clip.Clip.Keys[i];
					// this.Log($"Set Key {clipStartIndex + i}");
					m_PoseKeys[clipStartIndex + i] = new PoseKey() { Position = key.Position, Rotation = key.Rotation, Scale = key.Scale };
				}
			}

			m_MontageStates[pActiveMontageIndex] = new PoseMontageAnimatorState()
			{
				Handle = pMontageHandle,
				Montage = pMontage
			};
		}

		private void SetActiveMontageToPrevious(int pActiveMontageIndex, int pPreviousMontageIndex)
		{
			if (m_MontageStates[pActiveMontageIndex].IsComplete)
			{
				return;
			}
			m_Animations[pPreviousMontageIndex] = m_Animations[pActiveMontageIndex].SetClipsStartIndex(0);
			// m_Weights[previousMontageIndex] = m_Weights[activeMontageIndex];

			int activeMontageIndex2 = m_SkeletonKeyCount * MAX_MONTAGE_POSE_COUNT * pActiveMontageIndex;
			int previousMontageIndex2 = m_SkeletonKeyCount * MAX_MONTAGE_POSE_COUNT * pPreviousMontageIndex;
			// this.Log($"montageIndex {activeMontageIndex2}");
			for (int m = 0; m < m_MontageStates[ACTIVE_MONTAGE_INDEX].Montage.Animation.Clips.Length; m++)
			{
				int clipStartIndex = m * m_SkeletonKeyCount;
				// this.Log($"clipStartIndex {clipStartIndex}");
				for (int i = 0; i < m_SkeletonKeyCount; i++)
				{
					int index = clipStartIndex + i;
					// this.Log($"Swapped Key {index} = {index + activeMontageIndex2}");
					m_PoseKeys[index + previousMontageIndex2] = m_PoseKeys[index + activeMontageIndex2];
				}
			}

			m_MontageStates[pPreviousMontageIndex] = m_MontageStates[pActiveMontageIndex];
			m_MontageStates[pPreviousMontageIndex].StartFadeOut();
			m_MontageStates[pActiveMontageIndex].Clear();
		}

		public void CancelMontage(int pAnimatorHandle, int pMontageHandle)
		{
			int animatorIndex = pAnimatorHandle * 2;
			int activeMontageIndex = animatorIndex + ACTIVE_MONTAGE_INDEX;
			if (m_MontageStates[activeMontageIndex].Handle == pMontageHandle)
			{
				m_MontageStates[activeMontageIndex].StartFadeOut();
				return;
			}

			int previousMontageIndex = animatorIndex + PREVIOUS_MONTAGE_INDEX;
			if (m_MontageStates[previousMontageIndex].Handle == pMontageHandle)
			{
				m_MontageStates[previousMontageIndex].StartFadeOut();
				return;
			}
		}

		public void Tick(float pDeltaTime)
		{
			for (int i = 0; i < m_MontageStates.Length; i++)
			{
				if (!m_MontageStates[i].IsComplete)
				{
					m_MontageStates[i].AddTime(pDeltaTime);
					m_PoseWeights[i] = m_MontageStates[i].GetPoseWeight();
				}
			}
		}

		public static void AnimatorHandleToMontageIndexes(int pAnimatorHandle, out int oActiveMontageIndex, out int oPreviousMontageIndex)
		{
			pAnimatorHandle *= MAX_MONTAGE_COUNT;
			oActiveMontageIndex = pAnimatorHandle + ACTIVE_MONTAGE_INDEX;
			oPreviousMontageIndex = pAnimatorHandle + PREVIOUS_MONTAGE_INDEX;
		}
	}
}
