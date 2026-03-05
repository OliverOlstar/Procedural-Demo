using System.Collections.Generic;
using ODev.Picker;
using ODev.Util;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ODev.PoseAnimator
{
    [CreateAssetMenu(fileName = "New Pose Animator Config", menuName = "PoseAnimator/Config")]
    public class SOPoseAnimatorConfig : ScriptableObject
    {
        [SerializeField, AssetNonNull] private SOPoseSkeleton m_PoseSkeleton;
        [SerializeField, AssetNonNull] private SOPoseAnimation[] m_PoseAnimations = new SOPoseAnimation[0];
        // [SerializeField, ReadOnly] private int[] m_IgnoreSkeletonBones;

        private readonly Dictionary<SOPoseAnimation, int> m_AnimationToIndex = new();

        public SOPoseSkeleton Skeleton => m_PoseSkeleton;
        public IReadOnlyList<SOPoseAnimation> Animations => m_PoseAnimations;
        // public IReadOnlyList<int> IgnoreSkeletonBones => m_IgnoreSkeletonBones;

        public bool TryGetIndex(SOPoseAnimation animation, out int handle)
        {
            return m_AnimationToIndex.TryGetValue(animation, out handle);
        }

        private void OnEnable() // OnLoad
        {
            m_AnimationToIndex.Clear();
            m_AnimationToIndex.EnsureCapacity(m_PoseAnimations.Length);
            for (int i = 0; i < m_PoseAnimations.Length; i++)
            {
                m_AnimationToIndex.Add(m_PoseAnimations[i], i);
            }
        }
        private void OnValidate() => OnEnable();

// #if UNITY_EDITOR
//         [Button("Calculate Bones To Ignore")]
//         private void EditorCalculateBonesToIgnore()
//         {
//             m_IgnoreSkeletonBones = new int[m_PoseSkeleton.BoneCount];
//             for (int i = 0; i < m_PoseSkeleton.BoneCount; i++)
//             {
//                 m_IgnoreSkeletonBones[i] = IsBoneUsedAtIndex(i);
//             }

//             UnityEditor.EditorUtility.SetDirty(this);
//             UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
//         }

//         private int IsBoneUsedAtIndex(int pBoneIndex)
//         {
//             for (int i = 0; i < m_PoseAnimations.Length; i++)
//             {
//                 SOPoseAnimation animation = m_PoseAnimations[i];
//                 foreach (var clip in animation.Clips)
//                 {
//                     var key = clip.Clip.Keys[pBoneIndex];
//                     if (!key.Position.IsNearZero())
//                     {
//                         return i;
//                     }
//                     if (!key.Rotation.IsNearIdentity())
//                     {
//                         return i;
//                     }
//                     if (!key.Scale.IsNearZero())
//                     {
//                         return i;
//                     }
//                 }
//             }
//             return -1;
//         }
// #endif
    }
}
