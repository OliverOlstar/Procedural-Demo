using ODev.Picker;
using ODev.PoseAnimator;
using ODev.Util;
using UnityEngine;

[System.Serializable]
public class PoseAnimatorLocomotion : PoseAnimatorControllerBase
{
	[SerializeField, AssetNonNull]
	private SOPoseAnimation m_WalkAnimation = null;
	[SerializeField, AssetNonNull]
	private SOPoseAnimation m_RunAnimation = null;

	[Space, SerializeField]
	private float m_RunVelocity = 10.0f;
	[SerializeField]
	private float m_WalkVelocity = 1.0f;
	[SerializeField]
	private float m_WeightDampening = 1.0f;

	[SerializeField]
	private float m_WalkBounceHeight = 0.5f;
	[SerializeField]
	private float m_RunBounceHeight = 0.25f;

	private int m_WalkHandle = -1;
	private int m_RunHandle = -1;

	private float m_RunWeight01 = 0.0f;
	private float m_WalkWeight01 = 0.0f;

	protected override void Setup()
	{
		m_WalkHandle = Animator.Add(m_WalkAnimation);
		m_RunHandle = Animator.Add(m_RunAnimation);
		Controller.WheelRadius.AddWheelRadius(m_WalkHandle, 1.0f);
		Controller.WheelRadius.AddWheelRadius(m_RunHandle, 2.0f);
		Controller.CenterOfMassBounce.AddBounce(m_WalkHandle, m_WalkBounceHeight);
		Controller.CenterOfMassBounce.AddBounce(m_RunHandle, m_RunBounceHeight);
	}

	public override void Destroy()
	{

	}

	public override void Tick(float pDeltaTime)
	{
		float nextWeight = Func.SmoothStep(m_WalkVelocity, m_RunVelocity, Root.Movement.VelocityXZ.magnitude);
		m_RunWeight01 = Mathf.Lerp(m_RunWeight01, nextWeight, pDeltaTime * m_WeightDampening);

		nextWeight = Func.SmoothStep(0.0f, m_WalkVelocity, Root.Movement.VelocityXZ.magnitude);
		m_WalkWeight01 = Mathf.Lerp(m_WalkWeight01, nextWeight, pDeltaTime * m_WeightDampening);

		float progress = Controller.Wheel.Angle / 180.0f;
		Animator.SetWeight(m_WalkHandle, progress, m_WalkWeight01);
		Animator.SetWeight(m_RunHandle, progress, m_RunWeight01);
	}
}
