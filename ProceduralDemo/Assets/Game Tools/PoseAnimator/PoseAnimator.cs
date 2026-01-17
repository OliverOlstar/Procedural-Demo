using System.Collections.Generic;
using ODev.Picker;
using ODev.Updateables;
using ODev.Util;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Pool;

namespace ODev.PoseAnimator
{
	public class PoseAnimator : MonoBehaviour
	{
		private readonly Updateable m_Updateable = new(UpdateableType.Fixed, UpdateablePriority.PoseAnimator);
		private readonly Updateable m_UpdateableComplete = new(UpdateableType.Fixed, UpdateablePriority.PoseAnimator + 1);

		[SerializeField] private Transform m_Root = null;
		[SerializeField, AssetNonNull] private SOPoseSkeleton m_Skeleton = null;
		[SerializeField, AssetNonNull] private SOPoseAnimatorConfig m_Config = null;
		[SerializeField] private PoseMontageAnimator m_Montages = new();

		// Data
		private NativeArray<PoseKey> m_SkeletonKeys;
		private NativeArray<PoseAnimation> m_Animations;
		private NativeArray<PoseWeight> m_Weights;
		private NativeArray<PoseKey> m_PoseKeys;

		// Context
		private TransformAccessArray m_AccessArray;
		private NativeArray<PoseKey> m_NextPose;

		private JobHandle m_Handle;

		private void OnEnable()
		{
			m_Updateable.Register(Tick);
			m_UpdateableComplete.Register(TickComplete);
		}

		private void OnDisable()
		{
			m_Updateable.UnRegister();
			m_UpdateableComplete.UnRegister();
		}

		private void Start()
		{
			m_SkeletonKeys = new NativeArray<PoseKey>(m_Skeleton.BoneCount, Allocator.Persistent);
			m_AccessArray = new TransformAccessArray(m_Skeleton.BoneCount);
			m_NextPose = new NativeArray<PoseKey>(m_Skeleton.BoneCount, Allocator.Persistent);

			PoseUtil.CopySkeleton(m_SkeletonKeys, m_Skeleton);

			int index = 0;
			foreach (PoseUtil.Bone bone in PoseUtil.GetAllBones(m_Skeleton, m_Root))
			{
				m_AccessArray.Add(bone.Transform);
				index++;
			}

			InitalizeAnimations();

			m_Montages.Initalize(m_Skeleton.BoneCount);
		}

		public int InitalizeAnimations()
		{
			m_Animations = new NativeArray<PoseAnimation>(m_Config.Animations.Count, Allocator.Persistent);
			m_Weights = new NativeArray<PoseWeight>(m_Config.Animations.Count, Allocator.Persistent);

			List<PoseKey> PoseKeys = ListPool<PoseKey>.Get();
			for (int i = 0; i < m_Config.Animations.Count; i++)
			{
				var animation = m_Config.Animations[i];
				if (animation == null)
				{
					this.DevException($"Cannot add null animations at index {i}");
				}

				m_Animations[i] = new(animation, Mathf.FloorToInt(PoseKeys.Count / m_SkeletonKeys.Length));
				m_Weights[i] = new PoseWeight(0.0f);

				foreach (SOPoseAnimation.AnimationClip clip in animation.Clips)
				{
					for (int z = 0; z < clip.Clip.KeyCount; z++)
					{
						PoseKey key = clip.Clip.GetKey(z);
						PoseKeys.Add(new PoseKey() { Position = key.Position, Rotation = key.Rotation, Scale = key.Scale });
					}
				}
				this.Log($"[{animation.name}] m_Animations {m_Animations.Length} | m_Weights {m_Weights.Length} | m_PoseKeys {PoseKeys.Count}");
			}

			m_PoseKeys = new NativeArray<PoseKey>(PoseKeys.Count, Allocator.Persistent);
			for (int i = 0; i < PoseKeys.Count; i++)
			{
				m_PoseKeys[i] = PoseKeys[i];
			}
			ListPool<PoseKey>.Release(PoseKeys);
			return m_Animations.Length - 1;
		}

		private void OnDestroy()
		{
			m_Handle.Complete();
			m_Montages.Destroy();

			m_SkeletonKeys.Dispose();
			m_Animations.Dispose();
			m_Weights.Dispose();
			m_PoseKeys.Dispose();
			m_AccessArray.Dispose();
			m_NextPose.Dispose();
		}

		private void Tick(float pDeltaTime)
		{
			m_Montages.Tick(pDeltaTime);

			if (!m_Montages.IsWeightFull())
			{
				PoseBoneSystem poseBoneSystem = new()
				{
					SkeletonKeys = m_SkeletonKeys,
					SkeletonLength = m_SkeletonKeys.Length,
					Animations = m_Animations,
					Weights = m_Weights,
					PoseKeys = m_PoseKeys,
					UseNextPoseAsTheBase = false,

					NextPose = m_NextPose, // Modify
				};
				m_Handle = poseBoneSystem.Schedule(poseBoneSystem.SkeletonLength, poseBoneSystem.SkeletonLength, m_Handle);
			}

			m_Handle = m_Montages.TickSchedule(m_SkeletonKeys, m_NextPose, m_Handle);

			ApplyTransformSystem applyTransformSystem = new()
			{
				NextPose = m_NextPose,
			};
			m_Handle = applyTransformSystem.Schedule(m_AccessArray, m_Handle);
		}

		private void TickComplete(float pDeltaTime)
		{
			m_Handle.Complete();
			m_Handle = default;
		}

		public int GetHandle(SOPoseAnimation pAnimation)
		{
			if (!m_Config.TryGetIndex(pAnimation, out int handle))
			{
				this.DevException($"Animation {pAnimation.name} not found in this {nameof(PoseAnimator)}");
			}
			return handle;
		}

		public PoseWeight GetWeight(int pIndex)
		{
			return m_Weights[pIndex];
		}

		public void SetWeight(int pHandle, float pProgress01, float pWeight01 = 1.0f)
		{
			m_Weights[pHandle] = new PoseWeight()
			{
				Progress01 = pProgress01,
				Weight01 = pWeight01
			};
		}

		public void ModifyWeight(int pHandle, float pProgressDelta, float pWeight01 = 1.0f)
		{
			float progress = m_Weights[pHandle].Progress01 + pProgressDelta;
			m_Weights[pHandle] = new PoseWeight()
			{
				Progress01 = progress,
				Weight01 = pWeight01
			};
		}

		public int PlayMontage(SOPoseMontage pMontage) => m_Montages.PlayMontage(pMontage);
		public void CancelMontage(int pIndex) => m_Montages.CancelMontage(pIndex);
	}
}
