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
        private const int DEFAULT_BATCH_RESIZE = 4;

        private readonly SOPoseSkeleton m_Skeleton;
        private readonly SOPoseAnimatorConfig m_Config;

        private NativeArray<PoseKey> m_SkeletonKeys;
        private NativeArray<PoseAnimation> m_Animations;
        private NativeArray<bool> m_IsAnimating; // Size = Number of animators
        private NativeArray<PoseKey> m_PoseKeys; // Size = Number of PoseKeys in Animations + montage animation keys
        private NativeArray<PoseWeight> m_Weights; // Size = Animations count + montages
        private NativeArray<PoseKey> m_NextPose; // Size = Bones count
        private TransformAccessArray m_AccessArray; // Size = Bones count
        private PoseBoneSystem m_BonePoseJob;
        private ApplyTransformSystem m_ApplyTransformJob;

        private readonly PoseMontageAnimator m_Montages = new();

        private JobHandle m_JobsHandle;
        private readonly List<int> m_FreeIndexes = new(DEFAULT_BATCH_SIZE);
        private bool m_IsDisposed = false;
        private int m_BatchSize = 0;

        public PoseSystem(SOPoseSkeleton pSkeleton, SOPoseAnimatorConfig pConfig)
        {
            m_Skeleton = pSkeleton;
            m_Config = pConfig;

            m_SkeletonKeys = new NativeArray<PoseKey>(m_Skeleton.BoneCount, Allocator.Persistent);
            PoseUtil.CopySkeleton(m_SkeletonKeys, m_Skeleton);

            int poseKeyCountInConfig = 0;
            foreach (var animation in m_Config.Animations)
                foreach (var clip in animation.Clips)
                {
                    poseKeyCountInConfig += clip.Clip.Keys.Count;
                }
            poseKeyCountInConfig += m_Skeleton.BoneCount * PoseMontageAnimator.MAX_MONTAGE_POSE_COUNT * PoseMontageAnimator.MAX_MONTAGE_COUNT;
            m_PoseKeys = new NativeArray<PoseKey>(poseKeyCountInConfig, Allocator.Persistent);
            m_Animations = new NativeArray<PoseAnimation>(m_Config.Animations.Count + PoseMontageAnimator.MAX_MONTAGE_COUNT, Allocator.Persistent);

            m_Montages.Initalize(pSkeleton.BoneCount, m_BatchSize);

            InitalizeAnimations();
        }

        public void InitalizeAnimations()
        {
            int poseKeyIndex = 0;
            for (int i = 0; i < m_Config.Animations.Count; i++)
            {
                var animation = m_Config.Animations[i];
                if (animation == null)
                {
                    this.DevException($"Cannot add null animations at index {i}");
                }

                m_Animations[i] = new(animation, Mathf.FloorToInt(poseKeyIndex / m_SkeletonKeys.Length));

                foreach (var clip in animation.Clips)
                    foreach (var key in clip.Clip.Keys)
                    {
                        m_PoseKeys[poseKeyIndex] = new PoseKey()
                        {
                            Position = key.Position,
                            Rotation = key.Rotation,
                            Scale = key.Scale
                        };
                        poseKeyIndex++;
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
                m_Weights = new NativeArray<PoseWeight>((m_Animations.Length + PoseMontageAnimator.MAX_MONTAGE_COUNT) * m_BatchSize, Allocator.Persistent);
                m_NextPose = new NativeArray<PoseKey>(m_SkeletonKeys.Length * m_BatchSize, Allocator.Persistent);
                m_AccessArray = new TransformAccessArray(m_SkeletonKeys.Length * m_BatchSize);
            }
            else
            {
                PoseUtil.ResizeNative(ref m_IsAnimating, m_BatchSize);
                PoseUtil.ResizeNative(ref m_Weights, (m_Animations.Length + PoseMontageAnimator.MAX_MONTAGE_COUNT) * m_BatchSize);
                PoseUtil.ResizeNative(ref m_NextPose, m_SkeletonKeys.Length * m_BatchSize);
                m_AccessArray.capacity = m_SkeletonKeys.Length * m_BatchSize;
            }

            while (m_AccessArray.length < m_AccessArray.capacity)
            {
                m_AccessArray.Add(null);
            }

            m_Montages.ResizeArrays(m_BatchSize);

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
                PoseKeys = m_PoseKeys,
                Weights = m_Weights,
                IsAnimating = m_IsAnimating,
                
                MontageAnimations = m_Montages.Animations,
                MontagePoseKeys = m_Montages.PoseKeys,
                MontageWeights = m_Montages.PoseWeights,

                NextPose = m_NextPose, // Modify
            };

            m_ApplyTransformJob = new ApplyTransformSystem()
            {
                NextPose = m_NextPose,
            };
        }

        public void Dispose()
        {
            m_JobsHandle.Complete();
            m_Montages.Dispose();

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

            m_Montages.Tick(pDeltaTime);

            // if (!m_Montages.IsWeightFull())
            // {
                m_JobsHandle = m_BonePoseJob.Schedule(m_BatchSize, DEFAULT_BATCH_SIZE, m_JobsHandle);
            // }

            // m_JobsHandle = m_Montages.TickSchedule(m_SkeletonKeys, m_NextPose, m_JobsHandle);

            m_JobsHandle = m_ApplyTransformJob.Schedule(m_AccessArray, m_JobsHandle);

            Profiler.EndSample();
        }

        public void TickComplete(float pDeltaTime)
        {
            m_JobsHandle.Complete();
            m_JobsHandle = default;
        }

        public int AddAnimator(Transform pRoot)
        {
            Assert.IsFalse(m_IsDisposed);

            if (m_FreeIndexes.Count == 0)
            {
                ResizeArrays(DEFAULT_BATCH_RESIZE); // TODO: Change to a request and delay till next tick. This allows for resizing the arrays only once rather than 20 times in a single frame
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

        public int PlayMontage(int pHandle, SOPoseMontage pMontage) => m_Montages.PlayMontage(pHandle, pMontage);
        public void CancelMontage(int pHandle, int pMontageHandle) => m_Montages.CancelMontage(pHandle, pMontageHandle);
    }
}
