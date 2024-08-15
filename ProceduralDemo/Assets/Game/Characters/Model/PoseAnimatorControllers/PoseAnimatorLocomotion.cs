using ODev.Picker;
using ODev.PoseAnimator;
using ODev.Util;
using UnityEngine;

[System.Serializable]
public class PoseAnimatorLocomotion : PoseAnimatorControllerBase
{
	[SerializeField]
	private CardinalWheel m_Wheel = null;
	[SerializeField]
	private Transform m_CenterOfMass = null;

	[Space, SerializeField, AssetNonNull]
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
	private float m_BounceHeight = 0.25f;

	private int m_WalkHandle = -1;
	private int m_RunHandle = -1;

	private float m_RunWeight01 = 0.0f;
	private float m_WalkWeight01 = 0.0f;

	protected override void Setup()
	{
		m_WalkHandle = Animator.Add(m_WalkAnimation);
		m_RunHandle = Animator.Add(m_RunAnimation);

		m_Wheel.OnAngleChanged.AddListener(OnMoveAngleChanged);
	}

	public override void Destroy()
	{
		m_Wheel.OnAngleChanged.RemoveListener(OnMoveAngleChanged);
	}

	public override void Tick(float pDeltaTime)
	{
		float nextWeight = Func.SmoothStep(m_WalkVelocity, m_RunVelocity, Root.Movement.VelocityXZ.magnitude);
		m_RunWeight01 = Mathf.Lerp(m_RunWeight01, nextWeight, pDeltaTime * m_WeightDampening);
		m_Wheel.SetRadius(Mathf.Lerp(1.0f, 2.0f, m_RunWeight01));

		nextWeight = Func.SmoothStep(0.0f, m_WalkVelocity, Root.Movement.VelocityXZ.magnitude);
		m_WalkWeight01 = Mathf.Lerp(m_WalkWeight01, nextWeight, pDeltaTime * m_WeightDampening);
	}

	private void OnMoveAngleChanged(float pAngle)
	{
		float progress = pAngle / 180.0f;
		Animator.SetWeight(m_WalkHandle, progress, m_WalkWeight01);
		Animator.SetWeight(m_RunHandle, progress, m_RunWeight01);

		float x = (pAngle % 90.0f) / 90.0f;
		float y = Mathf.Abs(Mathf.Sin(x * Mathf.PI)) * m_BounceHeight / m_Wheel.Radius;
		m_CenterOfMass.transform.localPosition = new Vector3(0.0f, y * m_WalkWeight01, 0.0f);
	}
}
