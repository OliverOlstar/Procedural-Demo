using ODev.Picker;
using ODev.PoseAnimator;
using ODev.Util;
using UnityEngine;

[System.Serializable]
public class PoseAnimatorJump : PoseAnimatorControllerBase
{
	[SerializeField, AssetNonNull]
	private SOPoseAnimation m_JumpAnimation = null;

	[Space, SerializeField]
	private float m_WeightDampening = 1.0f;
	[SerializeField]
	private float m_ProgressDampening = 10.0f;
	[SerializeField]
	private float m_ProgressScale = 0.15f;
	[SerializeField, Range(0.0f, 1.0f)]
	private float m_ProgressOffset = 0.5f;

	private int m_JumpHandle = -1;

	private float m_Progress01 = 0.0f;
	private float m_Weight01 = 0.0f;

	protected override void Setup()
	{
		m_JumpHandle = Animator.Add(m_JumpAnimation);
	}

	public override void Destroy() { }

	public override void Tick(float pDeltaTime)
	{
		float progress = (m_ProgressOffset - (Root.Movement.VelocityY * m_ProgressScale)).Clamp01();
		m_Progress01 = Mathf.Lerp(m_Progress01, progress, pDeltaTime * m_ProgressDampening);

		float weight = Root.OnGround.IsInAir ? 1.0f : 0.0f;
		m_Weight01 = Mathf.Lerp(m_Weight01, weight, pDeltaTime * m_WeightDampening);

		Animator.SetWeight(m_JumpHandle, m_Progress01, m_Weight01);
	}
}
