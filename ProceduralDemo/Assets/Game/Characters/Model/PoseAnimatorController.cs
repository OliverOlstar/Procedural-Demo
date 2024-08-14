using System;
using System.Collections;
using System.Collections.Generic;
using ODev;
using ODev.Picker;
using ODev.PoseAnimator;
using ODev.Util;
using UnityEngine;

public class PoseAnimatorController : UpdateableMonoBehaviour
{
	[SerializeField]
	private PoseAnimator m_Animator = null;
	[SerializeField]
	private PlayerRoot m_Root = null;

	[SerializeField, AssetNonNull]
	private SOPoseAnimation m_IdleAnimation = null;
	[SerializeField, AssetNonNull]
	private SOPoseAnimation m_WalkAnimation = null;
	[SerializeField, AssetNonNull]
	private SOPoseAnimation m_RunAnimation = null;
	[SerializeField, AssetNonNull]
	private SOPoseAnimation m_JumpAnimation = null;
	[SerializeField]
	private float m_IdleAnimationSpeed = 0.1f;

	[SerializeField]
	private CardinalWheel m_Wheel = null;

	[SerializeField]
	private Transform m_CenterOfMass = null;


	// [Header("Spring")]
	// [SerializeField, Range(0.0f, 1.0f)]
	// private float m_TargetProgress01 = 0.0f;
	// [SerializeField, Range(0.0f, 1.0f)]
	// private float m_Weight01 = 0.0f;
	// [SerializeField]
	// private float m_Spring = 100.0f;
	// [SerializeField]
	// private float m_Damper = 10.0f;

	// [Header("Smooth")]
	// [SerializeField]
	// private float m_Speed = 0.1f;
	// [SerializeField]
	// private Easing.EaseParams m_Ease = new();

	// [SerializeField]
	// private bool m_UseSpring = false;

	private int m_IdleHandle = -1;
	private int m_WalkHandle = -1;
	private int m_RunHandle = -1;
	private int m_JumpHandle = -1;
	private float m_RunWeight01 = 0.0f;
	private float m_WalkWeight01 = 0.0f;
	private float m_JumpWeight01 = 0.0f;
	// private float m_Progress;
	// private float m_ProgressVelocity = 0.0f;

	private void Start()
	{
		m_Wheel.OnAngleChanged.AddListener(OnMoveAngleChanged);
		m_IdleHandle = m_Animator.Add(m_IdleAnimation);
		m_WalkHandle = m_Animator.Add(m_WalkAnimation);
		m_RunHandle = m_Animator.Add(m_RunAnimation);
		m_JumpHandle = m_Animator.Add(m_JumpAnimation);
	}
	
	private void OnDestroy()
	{
		m_Wheel.OnAngleChanged.RemoveListener(OnMoveAngleChanged);
	}

	[SerializeField]
	private float m_RunVelocity = 10.0f;
	[SerializeField]
	private float m_WalkVelocity = 1.0f;

	[SerializeField]
	private float m_WeightDampening = 1.0f;

	protected override void Tick(float pDeltaTime)
	{
		// if (m_UseSpring)
		// {
		// 	m_Progress = Func.SpringDamper(m_Progress, m_TargetProgress01, ref m_ProgressVelocity, m_Spring, m_Damper, pDeltaTime);
		// 	m_Animator.SetWeight(m_AnimationIndex, m_Progress, m_Weight01);
		// }
		// else
		// {
		// 	m_Progress += pDeltaTime * m_Speed;
		// 	float progress = Easing.Ease(m_Ease, (m_Progress * 2.0f)) * 0.5f;
		// 	m_Animator.SetWeight(m_AnimationIndex, progress, m_Weight01);
		// }
		m_Animator.ModifyWeight(m_IdleHandle, m_IdleAnimationSpeed * pDeltaTime);

		float nextWeight = Func.SmoothStep(m_WalkVelocity, m_RunVelocity, m_Root.Movement.VelocityXZ.magnitude);
		m_RunWeight01 = Mathf.Lerp(m_RunWeight01, nextWeight, pDeltaTime * m_WeightDampening);
		m_Wheel.SetRadius(Mathf.Lerp(1.0f, 2.0f, m_RunWeight01));

		nextWeight = Func.SmoothStep(0.0f, m_WalkVelocity, m_Root.Movement.VelocityXZ.magnitude);
		m_WalkWeight01 = Mathf.Lerp(m_WalkWeight01, nextWeight, pDeltaTime * m_WeightDampening);

		float jumpProgress = 0.5f - (m_Root.Movement.VelocityY * 0.15f);
		float jumpWeight = m_Root.OnGround.IsInAir ? 1.0f : 0.0f;
		m_JumpWeight01 = Mathf.Lerp(m_JumpWeight01, jumpWeight, pDeltaTime * m_WeightDampening);
		m_Animator.SetWeight(m_JumpHandle, jumpProgress.Clamp01(), m_JumpWeight01);
	}

	[SerializeField]
	private float m_BounceHeight = 0.25f;

	private void OnMoveAngleChanged(float pAngle)
	{
		float progress = pAngle / 180.0f;
		m_Animator.SetWeight(m_WalkHandle, progress, m_WalkWeight01);
		m_Animator.SetWeight(m_RunHandle, progress, m_RunWeight01);

		float x = (pAngle % 90.0f) / 90.0f;
		float y = Mathf.Abs(Mathf.Sin(x * Mathf.PI)) * m_BounceHeight / m_Wheel.Radius;
		m_CenterOfMass.transform.localPosition = new Vector3(0.0f, y * m_WalkWeight01, 0.0f);
	}
}
