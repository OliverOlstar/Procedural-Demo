using System.Collections;
using System.Collections.Generic;
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
		private struct MontageState
		{
			public int Index;
			public SOPoseMontage Montage;
			public float Time;

			public readonly float Progress01 => Mathf.Clamp01((Time - Montage.StartSeconds) / Montage.Seconds);
			public readonly float Weight01()
			{
				if (Time < Montage.FadeInSeconds)
				{
					return (Time / Montage.FadeInSeconds).Clamp01();
				}
				if (Time > Montage.Seconds - Montage.FadeOutSeconds)
				{
					return ((Montage.TotalSeconds - Time) / Montage.FadeOutSeconds).Clamp01();
				}
				return 1.0f;
			}
			public readonly bool IsComplete => Montage == null || Time > Montage.TotalSeconds;
		}

		private const int MAX_MONTAGE_COUNT = 1;
		private const int MAX_MONTAGE_POSE_COUNT = MAX_MONTAGE_COUNT * 4;

		private NativeArray<PoseAnimation> m_Animations;
		private NativeArray<PoseWeight> m_Weights;
		private NativeArray<PoseKey> m_PoseKeys;
		private readonly MontageState[] m_ActiveMontages = new MontageState[MAX_MONTAGE_COUNT];

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

			m_ActiveMontages[0] = new MontageState()
			{
				Index = 0,
				Montage = pMontage,
				Time = 0
			};

			// this.Log(pMontage.name);
			return 0;
		}

		public void CancelMontage(int pIndex)
		{
			this.DevException(new System.NotImplementedException());
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
			m_Weights[0] = new()
			{
				Weight01 = m_ActiveMontages[0].Weight01(),
				Progress01 = m_ActiveMontages[0].Progress01,
			};
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
