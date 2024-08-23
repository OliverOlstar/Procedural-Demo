using ODev.Picker;
using ODev.PoseAnimator;
using UnityEngine;

[System.Serializable]
public class PoseAnimatorIdle : PoseAnimatorControllerBase
{
	[SerializeField, AssetNonNull]
	private SOPoseAnimation m_IdleAnimation = null;
	
	[Space, SerializeField]
	private float m_IdleAnimationSpeed = 0.1f;

	private int m_IdleHandle = -1;

	protected override void Setup()
	{
		m_IdleHandle = Animator.Add(m_IdleAnimation);
	}

	public override void Destroy() { }

	public override void Tick(float pDeltaTime)
	{
		Animator.ModifyWeight(m_IdleHandle, m_IdleAnimationSpeed * pDeltaTime);
	}
}
