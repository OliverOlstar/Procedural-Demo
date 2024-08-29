using System;
using ODev.Picker;
using ODev.PoseAnimator;
using ODev.Util;
using UnityEngine;

[System.Serializable]
public class PoseAnimatorCrouch : PoseAnimatorControllerBase
{	
	[SerializeField, AssetNonNull]
	private SOPoseAnimation m_IdleAnimation = null;
	[SerializeField, AssetNonNull]
	private SOPoseAnimation m_WalkAnimation = null;

	[Space, SerializeField]
	private float m_IdleAnimationSpeed = 0.1f;
	[SerializeField]
	private float m_WalkVelocity = 10.0f;
	[SerializeField]
	private float m_WalkDampening = 1.0f;
	[SerializeField]
	private float m_WalkWheelRadius = 0.5f;
	
	[Header("Overall Weight")]
	[SerializeField]
	private float m_WeightSpring = 100.0f;
	[SerializeField]
	private float m_WeightDamper = 10.0f;

	[Header("Forces")]
	[SerializeField]
	private float m_JumpForce = 0.0f;
	[SerializeField]
	private float m_LandForce = 0.0f;
	[SerializeField, Tooltip("Multiplies m_LandForce by 1.0f - 2.0f based on where player velocity Y is between range")]
	private Vector2 m_LandVelocitySmoothStep = new(0.5f, 5.0f);

	private bool m_IsCrouching = false;
	private int m_IdleHandle = -1;
	private int m_WalkHandle = -1;
	private float m_Weight01 = 0.0f;
	private float m_WalkWeight01 = 0.0f;
	private float m_WeightVelocity = 0.0f;

	protected override void Setup()
	{
		m_IdleHandle = Animator.Add(m_IdleAnimation);
		m_WalkHandle = Animator.Add(m_WalkAnimation);
		Controller.WheelRadius.AddWheelRadius(m_WalkHandle, m_WalkWheelRadius);
		Controller.CenterOfMassBounce.AddBounce(m_IdleHandle, 0.0f);

		Root.Abilities.OnAbilityActivated.AddListener(OnAbilityActivated);
		Root.Abilities.OnAbilityDeactivated.AddListener(OnAbilityDeactivated);
		Root.OnGround.OnGroundEnterEvent.AddListener(OnGroundEnter);
		Controller.Wheel.OnAngleChanged.AddListener(OnMoveAngleChanged);
	}

	public override void Destroy()
	{
		Root.Abilities.OnAbilityActivated.RemoveListener(OnAbilityActivated);
		Root.Abilities.OnAbilityDeactivated.RemoveListener(OnAbilityDeactivated);
		Root.OnGround.OnGroundEnterEvent.RemoveListener(OnGroundEnter);
		Controller.Wheel.OnAngleChanged.RemoveListener(OnMoveAngleChanged);
	}

	public override void Tick(float pDeltaTime)
	{
		WeightSpringDamper(pDeltaTime);
		IdleProgress(pDeltaTime);
		WalkProgress(pDeltaTime);
	}

	private void WeightSpringDamper(float pDeltaTime)
	{
		m_Weight01 = Func.SpringDamper(m_Weight01, m_IsCrouching ? 1.0f : 0.0f, ref m_WeightVelocity, m_WeightSpring, m_WeightDamper, pDeltaTime);
	}

	private void IdleProgress(float pDeltaTime)
	{
		Animator.ModifyWeight(m_IdleHandle, m_IdleAnimationSpeed * pDeltaTime, m_Weight01);
	}

	private void WalkProgress(float pDeltaTime)
	{
		float nextWeight = Func.SmoothStep(0.0f, m_WalkVelocity, Root.Movement.VelocityXZ.magnitude);
		m_WalkWeight01 = Mathf.Lerp(m_WalkWeight01, nextWeight, pDeltaTime * m_WalkDampening);
		Animator.ModifyWeight(m_WalkHandle, 0.0f, m_WalkWeight01 * m_Weight01);
	}

	private void OnMoveAngleChanged(float pAngle)
	{
		float progress = pAngle / 180.0f;
		Animator.SetWeight(m_WalkHandle, progress, m_WalkWeight01 * m_Weight01);
	}

	private void OnAbilityActivated(AbilityTags pTags)
	{
		if (pTags.HasFlag(AbilityTags.Jump))
		{
			m_WeightVelocity += m_JumpForce;
		}
		else if (pTags.HasFlag(AbilityTags.Crouch))
		{
			m_IsCrouching = true;
		}
	}

	private void OnAbilityDeactivated(AbilityTags pTags)
	{
		if (pTags.HasFlag(AbilityTags.Crouch))
		{
			m_IsCrouching = false;
		}
	}

	private void OnGroundEnter()
	{
		float scalar = Func.SmoothStep(m_LandVelocitySmoothStep, Root.Movement.VelocityY);
		m_WeightVelocity += m_LandForce * (scalar + 1);
	}
}