using System.Collections.Generic;
using ODev.Picker;
using UnityEngine;

namespace ODev.PoseAnimator
{
	[CreateAssetMenu(fileName = "New Pose Animator Config", menuName = "PoseAnimator/Config")]
	public class SOPoseAnimatorConfig : ScriptableObject
	{
        [SerializeField, AssetNonNull] private SOPoseSkeleton m_PoseSkeleton; 
		[SerializeField, AssetNonNull] private SOPoseAnimation[] m_PoseAnimations = new SOPoseAnimation[0];

        private readonly Dictionary<SOPoseAnimation, int> m_AnimationToIndex = new();
        // private readonly 

        public IReadOnlyList<SOPoseAnimation> Animations => m_PoseAnimations;
        public SOPoseSkeleton Skeleton => m_PoseSkeleton;

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

        // private void 
	}
}
