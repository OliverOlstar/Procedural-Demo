using System.Collections.Generic;
using System.Text;
using ODev.Picker;
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
		private Mono.Updateable m_Updateable = new(Mono.Type.Fixed, Mono.Priorities.PoseAnimator);
		private Mono.Updateable m_UpdateableComplete = new(Mono.Type.Fixed, Mono.Priorities.PoseAnimator + 1);

		[SerializeField]
		private Transform m_Root = null;
		[SerializeField, AssetNonNull]
		private SOPoseSkeleton m_Skeleton = null;

		// Data
		private NativeArray<PoseKey> m_SkeletonKeys;
		private NativeArray<PoseAnimation> m_Animations;
		private NativeArray<PoseWeight> m_Weights;
		private NativeArray<PoseKey> m_PoseKeys;

		// Context
		private TransformAccessArray m_AccessArray;
		private NativeArray<PoseKey> m_NextPose;

		private bool m_IsInitalized = false;
		private JobHandle m_Handle;

		private void OnEnable()
		{
			m_Updateable.Register(Tick);
			m_UpdateableComplete.Register(TickComplete);
		}
		private void OnDisable()
		{
			m_Updateable.Deregister();
			m_UpdateableComplete.Deregister();
		}

		private void Initalize()
		{
			if (m_IsInitalized)
			{
				return;
			}
			m_IsInitalized = true;

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
		}
		private void Start() => Initalize();

		void OnDestroy()
		{
			m_Handle.Complete();
			m_SkeletonKeys.Dispose();
			m_Animations.Dispose();
			m_Weights.Dispose();
			m_PoseKeys.Dispose();
			m_AccessArray.Dispose();
			m_NextPose.Dispose();
		}

		private void Tick(float pDeltaTime)
		{
			PoseBoneSystem poseBoneSystem = new()
			{
				SkeletonKeys = m_SkeletonKeys,
				SkeletonLength = m_SkeletonKeys.Length,
				Animations = m_Animations,
				Weights = m_Weights,
				PoseKeys = m_PoseKeys,

				NextPose = m_NextPose, // Modify
			};
			m_Handle = poseBoneSystem.Schedule(poseBoneSystem.SkeletonLength, poseBoneSystem.SkeletonLength);

			ApplyTransformSystem applyTransformSystem = new()
			{
				NextPose = m_NextPose,
			};
			m_Handle = applyTransformSystem.Schedule(m_AccessArray, m_Handle);
		}

		private void TickComplete(float pDeltaTime)
		{
			m_Handle.Complete();
		}


		public int Add(SOPoseAnimation pAnimation)
		{
			if (pAnimation == null)
			{
				this.DevException("Cannot add null animations");
			}

			Initalize();

			PoseAnimation animation = new(pAnimation, Mathf.FloorToInt(m_PoseKeys.Length / m_SkeletonKeys.Length));
			PoseUtil.AppendNative(ref m_Animations, animation);
			PoseUtil.AppendNative(ref m_Weights, new PoseWeight(1.0f));

			List<PoseKey> PoseKeys = ListPool<PoseKey>.Get();
			foreach (SOPoseAnimation.AnimationClip clip in pAnimation.Clips)
			{
				for (int i = 0; i < clip.Clip.KeyCount; i++)
				{
					PoseKey key = clip.Clip.GetKey(i);
					PoseKeys.Add(new PoseKey() { Position = key.Position, Rotation = key.Rotation, Scale = key.Scale });
				}
			}
			PoseUtil.AppendNative(ref m_PoseKeys, PoseKeys);
			ListPool<PoseKey>.Release(PoseKeys);

			this.Log($"[{pAnimation.name}] m_Animations {m_Animations.Length} | m_Weights {m_Weights.Length} | m_PoseKeys {m_PoseKeys.Length}");
			return m_Animations.Length - 1;
		}

		public void SetWeight(int pIndex, float pProgress01, float pWeight01 = 1.0f)
		{
			m_Weights[pIndex] = new PoseWeight()
			{
				Progress01 = pProgress01,
				Weight01 = pWeight01
			};
		}

		public void ModifyWeight(int pIndex, float pProgressDelta, float pWeight01 = 1.0f)
		{
			float progress = m_Weights[pIndex].Progress01 + pProgressDelta;
			m_Weights[pIndex] = new PoseWeight()
			{
				Progress01 = progress,
				Weight01 = pWeight01
			};
		}
	}
}
