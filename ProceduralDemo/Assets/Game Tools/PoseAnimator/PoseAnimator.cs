using System.Collections.Generic;
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
		private readonly Mono.Updateable m_Updateable = new(Mono.Type.Fixed, Mono.Priorities.PoseAnimator);
		private readonly Mono.Updateable m_UpdateableComplete = new(Mono.Type.Fixed, Mono.Priorities.PoseAnimator + 1);

		[SerializeField]
		private Transform m_Root = null;
		[SerializeField, AssetNonNull]
		private SOPoseSkeleton m_Skeleton = null;

		// Data
		private NativeArray<PoseKey> m_SkeletonKeys;
		private NativeArray<PoseAnimation> m_Animations;
		private NativeArray<PoseKey> m_PoseKeys;

		// Dynamic Data
		private TransformAccessArray m_AccessArray;
		private NativeArray<PoseKey> m_NextPose;

		private bool m_IsInitalized = false;
		private JobHandle m_Handle;

		private void OnEnable()
		{
			m_Updateable.Register(Tick);
			m_UpdateableComplete.Register(TickComplete);
			this.Log();
		}
		private void OnDisable()
		{
			m_Updateable.Deregister();
			m_UpdateableComplete.Deregister();
			this.Log();
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
			this.Log();
		}
		private void Start() => Initalize();

		public void Add(SOPoseAnimation pAnimation)
		{
			Initalize();

			List<PoseKey> PoseKeys = new();
			foreach (SOPoseClip clip in pAnimation.Clips)
			{
				for (int i = 0; i < clip.KeyCount; i++)
				{
					PoseKey key = clip.GetKey(i);
					PoseKeys.Add(new PoseKey() { Position = key.Position, Rotation = key.Rotation, Scale = key.Scale });
				}
			}

			PoseAnimation[] animations = new PoseAnimation[1] { new() { ClipIndexA = 0, ClipIndexB = 1, ClipIndexC = 2 } };
			m_Animations = new NativeArray<PoseAnimation>(animations, Allocator.Persistent);
			m_PoseKeys = new NativeArray<PoseKey>(PoseKeys.ToArray(), Allocator.Persistent);
			this.Log();
		}

		void OnDestroy()
		{
			m_Handle.Complete();
			m_SkeletonKeys.Dispose();
			m_Animations.Dispose();
			m_PoseKeys.Dispose();
			m_AccessArray.Dispose();
			m_NextPose.Dispose();
			this.Log();
		}

		private void Tick(float pDeltaTime)
		{
			m_Animations[0].ModifyProgress(pDeltaTime * 0.1f);
			
			PoseBoneSystem poseBoneSystem = new()
			{
				SkeletonKeys = m_SkeletonKeys,
				SkeletonLength = m_SkeletonKeys.Length,
				Animations = m_Animations,
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
	}
}
