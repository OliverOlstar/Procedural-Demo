using ODev.Picker;
using UnityEngine;

namespace ODev.PoseAnimator
{
	[CreateAssetMenu(fileName = "New Pose Animation", menuName = "PoseAnimator/Animation", order = 0)]
	public class SOPoseAnimation : ScriptableObject
	{
		[SerializeField, AssetNonNull]
		private SOPoseClip[] m_Clips = new SOPoseClip[2];
		public SOPoseClip[] Clips => m_Clips;
    }
}
