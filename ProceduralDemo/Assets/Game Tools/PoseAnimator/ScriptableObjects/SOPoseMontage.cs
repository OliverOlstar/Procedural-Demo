using ODev.Picker;
using UnityEngine;

namespace ODev.PoseAnimator
{
	[CreateAssetMenu(fileName = "New Pose Montage", menuName = "PoseAnimator/Montage", order = 0)]
	public class SOPoseMontage : ScriptableObject
	{
		[SerializeField, AssetNonNull]
		private SOPoseAnimation m_Animation = null;

		[Header("Progress")]
		[SerializeField]
		private float m_StartSeconds = 0.0f;
		[SerializeField]
		private float m_Seconds = 1.0f;
		[SerializeField]
		private float m_EndSeconds = 0.25f;

		[Header("Weight")]
		[SerializeField]
		private float m_FadeInSeconds = 0.25f;
		[SerializeField]
		private float m_FadeOutSeconds = 0.25f;

		public SOPoseAnimation Animation => m_Animation;

		public float TotalSeconds => m_Seconds + m_StartSeconds + m_EndSeconds;
		public float StartSeconds => m_StartSeconds;
		public float Seconds => m_Seconds;
		public float EndSeconds => m_EndSeconds;

		public float FadeInSeconds => m_FadeInSeconds;
		public float FadeOutSeconds => m_FadeOutSeconds;
	}
}
