using ODev.Picker;
using UnityEngine;

namespace ODev.PoseAnimator
{
	[CreateAssetMenu(fileName = "New Pose Animation", menuName = "PoseAnimator/Animation", order = 0)]
	public class SOPoseAnimation : ScriptableObject
	{
		[SerializeField, AssetNonNull]
		private SOPoseClip[] m_Clips = new SOPoseClip[2];

		// [SerializeField, Range(-1.0f, 2.0f)]
		// private float m_Progress = 0.0f;
		[SerializeField, Range(0.0f, 1.0f)]
		private float m_Progress01 = 0.0f;

		public SOPoseClip[] Clips => m_Clips;
		private float Progress01 => m_Progress01 /*+ m_Progress*/;

		public void Apply(Transform pBone, int pIndex)
		{
			float progress01 = Progress01;
			progress01 = GetClips(progress01, out SOPoseClip clipA, out SOPoseClip clipB);
			Vector3 position = Vector3.LerpUnclamped(clipA.GetKey(pIndex).Position, clipB.GetKey(pIndex).Position, progress01);
			Quaternion rotation = Quaternion.LerpUnclamped(clipA.GetKey(pIndex).Rotation, clipB.GetKey(pIndex).Rotation, progress01);
			Vector3 scale = Vector3.LerpUnclamped(clipA.GetKey(pIndex).Scale, clipB.GetKey(pIndex).Scale, progress01);
			pBone.SetLocalPositionAndRotation(position, rotation);
			pBone.localScale = scale;
		}

		private float GetClips(float pProgress01, out SOPoseClip oClipA, out SOPoseClip oClipB)
		{
			float scaledProgress = pProgress01 * (m_Clips.Length - 1);
			int indexA = Mathf.FloorToInt(scaledProgress);
			indexA = Mathf.Clamp(indexA, 0, m_Clips.Length - 2);
			oClipA = m_Clips[indexA];
			oClipB = m_Clips[indexA + 1];
			return scaledProgress - indexA;
		}
    }
}
