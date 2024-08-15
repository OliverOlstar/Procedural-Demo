using ODev.Picker;
using ODev.Util;
using UnityEngine;

namespace ODev.PoseAnimator
{
	[CreateAssetMenu(fileName = "New Pose Animation", menuName = "PoseAnimator/Animation", order = 0)]
	public class SOPoseAnimation : ScriptableObject
	{
		[System.Serializable]
		public struct AnimationClip
		{
			// [Range(0.0f, 1.0f)]
			// public float Progress01;
			[AssetNonNull]
			public SOPoseClip Clip;
		}

		[SerializeField]
		private AnimationClip[] m_AnimationClips = new AnimationClip[2];
		public AnimationClip[] Clips => m_AnimationClips;

		[SerializeField]
		private PoseAnimationType m_Type = PoseAnimationType.Linear;
		public PoseAnimationType PlayType => m_Type;
		
		[SerializeField]
		private Easing.EaseParams m_Easing;
		public Easing.EaseParams Easing => m_Easing;

		// [SerializeField]
		// private bool m_AutoSetProgress = true;

		// private void OnValidate()
		// {
		// 	for (int i = 0; i < m_AnimationClips.Length; i++)
		// 	{
		// 		if (i == 0)
		// 		{
		// 			m_AnimationClips[i].Progress01 = 0.0f;
		// 			continue;
		// 		}
		// 		if (i == m_AnimationClips.Length - 1)
		// 		{
		// 			m_AnimationClips[i].Progress01 = 1.0f;
		// 			continue;
		// 		}
		// 		if (m_AutoSetProgress)
		// 		{
		// 			float spacing = 1.0f / (m_AnimationClips.Length - 1.0f);
		// 			m_AnimationClips[i].Progress01 = spacing * i;
		// 		}
		// 		else
		// 		{
		// 			m_AnimationClips[i].Progress01 = Mathf.Clamp(m_AnimationClips[i].Progress01, m_AnimationClips[i - 1].Progress01, 1.0f);
		// 		}
		// 	}
		// }
    }
}
