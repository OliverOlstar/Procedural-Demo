using Unity.Jobs;
using Unity.Collections;
using ODev.Util;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ODev.PoseAnimator
{
	[System.Serializable]
	public class PoseMontageAnimator
	{
		private const int MAX_MONTAGE_COUNT = 2; // Active, Previous
		private const int MAX_MONTAGE_POSE_COUNT = MAX_MONTAGE_COUNT * 4;

		private const int ACTIVE_MONTAGE_INDEX = 1;
		private const int PREVIOUS_MONTAGE_INDEX = 0;

		private NativeArray<PoseAnimation> m_Animations;
		private NativeArray<PoseWeight> m_Weights;
		private NativeArray<PoseKey> m_PoseKeys;
		private PoseMontageAnimatorState m_ActiveMontage = new();
		private PoseMontageAnimatorState m_PreviousMontage = new();
		private int m_SkeletonKeyCount;
		private int m_NextHandle = int.MinValue;

		public void Initalize(int pSkeletonKeyCount)
		{
			m_SkeletonKeyCount = pSkeletonKeyCount;
			m_Animations = new(MAX_MONTAGE_COUNT, Allocator.Persistent);
			m_Weights = new(MAX_MONTAGE_COUNT, Allocator.Persistent);
			m_PoseKeys = new(m_SkeletonKeyCount * MAX_MONTAGE_POSE_COUNT * MAX_MONTAGE_COUNT, Allocator.Persistent);
		}

		public void Destroy()
		{
			m_Animations.Dispose();
			m_Weights.Dispose();
			m_PoseKeys.Dispose();
		}

		[Button]
		public int PlayMontage(SOPoseMontage pMontage)
		{
			if (pMontage == null)
			{
				this.LogError("Cannot play a null montage");
				return int.MinValue;
			}

			int handle = ++m_NextHandle;
			if (m_NextHandle == int.MaxValue)
			{
				m_NextHandle = int.MinValue;
			}

			SetActiveMontageToPrevious();
			SetActiveMontage(pMontage, handle);
			// this.Log(pMontage.name);

			return handle;
		}

		private void SetActiveMontage(SOPoseMontage pMontage, int pHandle)
		{
			m_Animations[ACTIVE_MONTAGE_INDEX] = new(pMontage.Animation, ACTIVE_MONTAGE_INDEX * MAX_MONTAGE_POSE_COUNT);
			m_Weights[ACTIVE_MONTAGE_INDEX] = new PoseWeight(0.0f);

			int montageStartIndex = m_SkeletonKeyCount * MAX_MONTAGE_POSE_COUNT * ACTIVE_MONTAGE_INDEX;
			for (int m = 0; m < pMontage.Animation.Clips.Length; m++)
			{
				SOPoseAnimation.AnimationClip clip = pMontage.Animation.Clips[m];
				int clipStartIndex = (m * m_SkeletonKeyCount) + montageStartIndex;
				// this.Log($"clipStartIndex {clipStartIndex}");
				for (int i = 0; i < m_SkeletonKeyCount; i++)
				{
					PoseKey key = clip.Clip.GetKey(i);
					// this.Log($"Set Key {clipStartIndex + i}");
					m_PoseKeys[clipStartIndex + i] = new PoseKey() { Position = key.Position, Rotation = key.Rotation, Scale = key.Scale };
				}
			}

			m_ActiveMontage = new PoseMontageAnimatorState()
			{
				Handle = pHandle,
				Montage = pMontage
			};
		}

		private void SetActiveMontageToPrevious()
		{
			if (m_ActiveMontage.IsComplete)
			{
				return;
			}
			m_Animations[PREVIOUS_MONTAGE_INDEX] = m_Animations[ACTIVE_MONTAGE_INDEX].SetClipsStartIndex(0);
			m_Weights[PREVIOUS_MONTAGE_INDEX] = m_Weights[ACTIVE_MONTAGE_INDEX];

			int activeMontageIndex = m_SkeletonKeyCount * MAX_MONTAGE_POSE_COUNT * ACTIVE_MONTAGE_INDEX;
			int previousMontageIndex = m_SkeletonKeyCount * MAX_MONTAGE_POSE_COUNT * PREVIOUS_MONTAGE_INDEX;
			// this.Log($"montageIndex {activeMontageIndex}");
			for (int m = 0; m < m_ActiveMontage.Montage.Animation.Clips.Length; m++)
			{
				int clipStartIndex = m * m_SkeletonKeyCount;
				// this.Log($"clipStartIndex {clipStartIndex}");
				for (int i = 0; i < m_SkeletonKeyCount; i++)
				{
					int index = clipStartIndex + i;
					// this.Log($"Swapped Key {index} = {index + activeMontageIndex}");
					m_PoseKeys[index + previousMontageIndex] = m_PoseKeys[index + activeMontageIndex];
				}
			}

			m_PreviousMontage = m_ActiveMontage;
			m_PreviousMontage.StartFadeOut();
			m_ActiveMontage.Clear();
		}

		[Button]
		public void CancelMontage(int pHandle)
		{
			if (m_ActiveMontage.Handle == pHandle)
			{
				m_ActiveMontage.StartFadeOut();
			}
			else if (m_PreviousMontage.Handle == pHandle)
			{
				m_PreviousMontage.StartFadeOut();
			}
		}

		public bool IsWeightFull() => m_Weights.IsCreated && m_Weights[ACTIVE_MONTAGE_INDEX].Weight01 >= 1.0f;

		public void Tick(float pDeltaTime)
		{
			TickMontage(ref m_ActiveMontage, ACTIVE_MONTAGE_INDEX, pDeltaTime);
			TickMontage(ref m_PreviousMontage, PREVIOUS_MONTAGE_INDEX, pDeltaTime);
		}
		private void TickMontage(ref PoseMontageAnimatorState montage, int pIndex, float pDeltaTime)
		{
			if (montage.IsComplete)
			{
				return;
			}
			montage.Time += pDeltaTime;
			m_Weights[pIndex] = montage.GetPoseWeight();
		}

		public JobHandle TickSchedule(NativeArray<PoseKey> pSkeletonKeys, NativeArray<PoseKey> pNextPose, JobHandle pDependsOn = default)
		{
			if (m_ActiveMontage.IsComplete && m_PreviousMontage.IsComplete)
			{
				return pDependsOn;
			}
			// this.Log($"Active Weight01 {m_Weights[ACTIVE_MONTAGE_INDEX].Weight01}, Progress01 {m_Weights[ACTIVE_MONTAGE_INDEX].Progress01}");
			// this.Log($"Previous Weight01 {m_Weights[PREVIOUS_MONTAGE_INDEX].Weight01}, Progress01 {m_Weights[PREVIOUS_MONTAGE_INDEX].Progress01}");
			PoseBoneSystem poseBoneSystem = new()
			{
				SkeletonKeys = pSkeletonKeys,
				SkeletonLength = pSkeletonKeys.Length,
				Animations = m_Animations,
				Weights = m_Weights,
				PoseKeys = m_PoseKeys,
				UseNextPoseAsTheBase = true,

				NextPose = pNextPose, // Modify
			};
			return poseBoneSystem.Schedule(poseBoneSystem.SkeletonLength, poseBoneSystem.SkeletonLength, pDependsOn);
		}
	}
}
