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
		private const int MAX_MONTAGE_COUNT = 1;
		private const int MAX_MONTAGE_POSE_COUNT = MAX_MONTAGE_COUNT * 4;

		private NativeArray<PoseAnimation> m_Animations;
		private NativeArray<PoseWeight> m_Weights;
		private NativeArray<PoseKey> m_PoseKeys;
		private readonly PoseMontageAnimatorState[] m_ActiveMontages = new PoseMontageAnimatorState[MAX_MONTAGE_COUNT];

		public void Initalize(int pSkeletonKeyCount)
		{
			m_Animations = new(MAX_MONTAGE_COUNT, Allocator.Persistent);
			m_Weights = new(MAX_MONTAGE_COUNT, Allocator.Persistent);
			m_PoseKeys = new(pSkeletonKeyCount * MAX_MONTAGE_POSE_COUNT, Allocator.Persistent);
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
				this.LogWarning("Cannot play a null montage");
			}

			m_Animations[0] = new(pMontage.Animation, 0);
			m_Weights[0] = new PoseWeight(0.0f);

			for (int m = 0; m < pMontage.Animation.Clips.Length; m++)
			{
				SOPoseAnimation.AnimationClip clip = pMontage.Animation.Clips[m];
				int startIndex = m * clip.Clip.KeyCount;
				for (int i = 0; i < clip.Clip.KeyCount; i++)
				{
					PoseKey key = clip.Clip.GetKey(i);
					m_PoseKeys[startIndex + i] = new PoseKey() { Position = key.Position, Rotation = key.Rotation, Scale = key.Scale };
				}
			}

			m_ActiveMontages[0] = new PoseMontageAnimatorState()
			{
				Index = 0,
				Montage = pMontage,
				Time = 0
			};

			// this.Log(pMontage.name);
			return 0;
		}

		[Button]
		public void CancelMontage(int pIndex)
		{
			m_ActiveMontages[pIndex].StartFadeOut();
		}

		public bool IsWeightFull()
		{
			return m_Weights.IsCreated && m_Weights[0].Weight01 >= 1.0f;
		}

		public void Tick(float pDeltaTime)
		{
			if (m_ActiveMontages[0].IsComplete)
			{
				return;
			}
			m_ActiveMontages[0].Time += pDeltaTime;
			m_Weights[0] = m_ActiveMontages[0].GetPoseWeight();
		}

		public JobHandle TickSchedule(NativeArray<PoseKey> pSkeletonKeys, NativeArray<PoseKey> pNextPose, JobHandle pDependsOn = default)
		{
			if (m_ActiveMontages[0].IsComplete)
			{
				return pDependsOn;
			}
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
