using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using ODev.Util;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODev.PoseAnimator
{
    public class PoseSystem
    {
        // private struct AnimatorCollections
        // {
        //     public bool IsAnimating;
        //     public NativeArray<PoseWeight> Weights;
        //     public NativeArray<PoseKey> PoseKeys;
        //     public NativeArray<PoseKey> NextPose;
        // }

        private const int DEFAULT_BATCH_SIZE = 2;

        private readonly SOPoseSkeleton m_Skeleton;
        private readonly SOPoseAnimatorConfig m_Config;

        private NativeArray<PoseKey> m_SkeletonKeys;
        private NativeArray<PoseAnimation> m_Animations;
        private NativeArray<bool> m_IsAnimating;
        private NativeArray<PoseWeight> m_Weights;
        private NativeArray<PoseKey> m_PoseKeys;
        private NativeArray<PoseKey> m_NextPose;
        private TransformAccessArray m_AccessArray;
        private PoseBoneSystem m_BonePoseJob;
        private ApplyTransformSystem m_ApplyTransformJob;

        private JobHandle m_Handle;
        private readonly List<int> m_FreeIndexes = new(DEFAULT_BATCH_SIZE);
        private bool m_IsDisposed = false;
        private int m_BatchSize = 0;
        private readonly int m_PoseKeyCountInConfig;

        public PoseSystem(SOPoseSkeleton pSkeleton, SOPoseAnimatorConfig pConfig)
        {
            m_Skeleton = pSkeleton;
            m_Config = pConfig;

            m_SkeletonKeys = new NativeArray<PoseKey>(m_Skeleton.BoneCount, Allocator.Persistent);
            PoseUtil.CopySkeleton(m_SkeletonKeys, m_Skeleton);

            m_PoseKeyCountInConfig = 0;
            foreach (var animation in m_Config.Animations)
                foreach (var clip in animation.Clips)
                {
                    m_PoseKeyCountInConfig += clip.Clip.Keys.Count;
                }
            m_PoseKeys = new NativeArray<PoseKey>(m_PoseKeyCountInConfig, Allocator.Persistent);
            m_Animations = new NativeArray<PoseAnimation>(m_Config.Animations.Count, Allocator.Persistent);

            InitalizeAnimations();

            // m_Montages.Initalize(m_Skeleton.BoneCount);
        }

        public void InitalizeAnimations()
        {
            int poseKeyCount = 0;
            for (int i = 0; i < m_Config.Animations.Count; i++)
            {
                var animation = m_Config.Animations[i];
                if (animation == null)
                {
                    this.DevException($"Cannot add null animations at index {i}");
                }

                m_Animations[i] = new(animation, Mathf.FloorToInt(poseKeyCount / m_SkeletonKeys.Length));

                foreach (var clip in animation.Clips)
                    foreach (var key in clip.Clip.Keys)
                    {
                        m_PoseKeys[poseKeyCount] = new PoseKey()
                        {
                            Position = key.Position,
                            Rotation = key.Rotation,
                            Scale = key.Scale
                        };
                        poseKeyCount++;
                    }
                // this.Log($"[{animation.name}] m_Animations {m_Animations.Length} | m_Weights {m_Weights.Length} | m_PoseKeys {PoseKeys.Count}");
            }

            CreateJobs();
        }

        private void ResizeArrays(int pAmountRequired)
        {
            int previousSize = m_BatchSize;
            m_BatchSize += pAmountRequired;
            m_BatchSize = Mathf.CeilToInt((float)m_BatchSize / DEFAULT_BATCH_SIZE) * DEFAULT_BATCH_SIZE;
            this.Log(m_BatchSize.ToString());

            if (!m_IsAnimating.IsCreated)
            {
                m_IsAnimating = new NativeArray<bool>(m_BatchSize, Allocator.Persistent);
                m_Weights = new NativeArray<PoseWeight>(m_Animations.Length * m_BatchSize, Allocator.Persistent);
                m_NextPose = new NativeArray<PoseKey>(m_SkeletonKeys.Length * m_BatchSize, Allocator.Persistent);
                m_AccessArray = new TransformAccessArray(m_SkeletonKeys.Length * m_BatchSize);
            }
            else
            {
                PoseUtil.ResizeNative(ref m_IsAnimating, m_BatchSize);
                PoseUtil.ResizeNative(ref m_Weights, m_Animations.Length * m_BatchSize);
                PoseUtil.ResizeNative(ref m_NextPose, m_SkeletonKeys.Length * m_BatchSize);
                m_AccessArray.capacity = m_SkeletonKeys.Length * m_BatchSize;
            }

            while (m_AccessArray.length < m_AccessArray.capacity)
            {
                m_AccessArray.Add(null);
            }

            m_FreeIndexes.Capacity = m_BatchSize;
            for (int i = previousSize; i < m_BatchSize; i++)
            {
                m_FreeIndexes.Add(i);
            }

            CreateJobs();
        }

        private void CreateJobs()
        {
            m_BonePoseJob = new PoseBoneSystem()
            {
                SkeletonKeys = m_SkeletonKeys,
                SkeletonLength = m_SkeletonKeys.Length,
                Animations = m_Animations,
                Weights = m_Weights,
                PoseKeys = m_PoseKeys,
                IsAnimating = m_IsAnimating,
                UseNextPoseAsTheBase = false,

                NextPose = m_NextPose, // Modify
            };

            m_ApplyTransformJob = new ApplyTransformSystem()
            {
                NextPose = m_NextPose,
            };
        }

        public void Dispose()
        {
            m_Handle.Complete();
            // m_Montages.Destroy();

            m_SkeletonKeys.Dispose();
            m_Animations.Dispose();
            m_Weights.Dispose();
            m_PoseKeys.Dispose();
            m_AccessArray.Dispose();
            m_NextPose.Dispose();
            m_FreeIndexes.Clear();

            m_IsDisposed = true;
        }

        public void Tick(float pDeltaTime)
        {
            Profiler.BeginSample($"{nameof(PoseAnimator)}.{nameof(Tick)}");

            // m_Montages.Tick(pDeltaTime);

            // if (!m_Montages.IsWeightFull())
            {
                m_Handle = m_BonePoseJob.Schedule(m_BatchSize, DEFAULT_BATCH_SIZE, m_Handle);
            }

            // m_Handle = m_Montages.TickSchedule(m_SkeletonKeys, m_NextPose, m_Handle);

            m_Handle = m_ApplyTransformJob.Schedule(m_AccessArray, m_Handle);

            Profiler.EndSample();
        }

        public void TickComplete(float pDeltaTime)
        {
            m_Handle.Complete();
            m_Handle = default;
        }

        public int AddAnimator(Transform pRoot)
        {
            Assert.IsFalse(m_IsDisposed);

            if (m_FreeIndexes.Count == 0)
            {
                ResizeArrays(1); // TODO: Change to a request and delay till next tick. This allows for resizing the arrays only once rather than 20 times in a single frame
            }

            int newIndex = m_FreeIndexes[0];
            m_FreeIndexes.RemoveAt(0); //? Can Swapback instead?
            this.Log(newIndex.ToString());

            m_IsAnimating[newIndex] = true;

            int weightsStartIndex = m_Animations.Length * newIndex;
            for (int i = 0; i < m_Animations.Length; i++)
            {
                m_Weights[weightsStartIndex + i] = new PoseWeight(0.0f);
            }

            int accessStartIndex = m_SkeletonKeys.Length * newIndex;
            int index = 0;
            foreach (PoseUtil.Bone bone in PoseUtil.GetAllBones(m_Skeleton, pRoot)) // TODO: Ensure get all bones doesn't go more than the expect amount!
            {
                m_AccessArray[accessStartIndex + index] = bone.Transform;
                index++;
            }

            return newIndex;
        }

        public void RemoveAnimator(int animatorHandle)
        {
            Assert.IsFalse(m_IsDisposed);

            int accessStartIndex = m_SkeletonKeys.Length * animatorHandle;
            for (int i = 0; i < m_SkeletonKeys.Length; i++)
            {
                m_AccessArray[accessStartIndex + i] = null;
            }

            m_IsAnimating[animatorHandle] = true;
            m_FreeIndexes.Add(animatorHandle);
        }

        public PoseWeight GetWeight(int pHandle, int pIndex)
        {
            Assert.IsFalse(m_IsDisposed);

            int keyIndex = (pHandle * m_Animations.Length) + pIndex;
            return m_Weights[keyIndex];
        }

        public void SetWeight(int pHandle, int pIndex, PoseWeight poseWeight)
        {
            Assert.IsFalse(m_IsDisposed);

            int keyIndex = (pHandle * m_Animations.Length) + pIndex;
            m_Weights[keyIndex] = poseWeight;
        }
    }
}
