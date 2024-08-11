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
	// [SerializeField, AssetNonNull]
	// private SOPoseAnimation m_WalkAnimation = null;
	[SerializeField, AssetNonNull]
	private SOPoseAnimation m_RunAnimation = null;

	[SerializeField]
	private CardinalWheel m_Wheel = null;


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

	private int m_RunHandle;
	// private float m_Progress;
	// private float m_ProgressVelocity = 0.0f;

	private void Start()
	{
		m_Wheel.OnAngleChanged.AddListener(OnMoveAngleChanged);
		m_RunHandle = m_Animator.Add(m_RunAnimation);
	}
	
	private void OnDestroy()
	{
		m_Wheel.OnAngleChanged.RemoveListener(OnMoveAngleChanged);
	}

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
	}


	private void OnMoveAngleChanged(float pAngle)
	{
		float progress = (pAngle % 180.0f) / 180.0f;
		m_Animator.SetWeight(m_RunHandle, progress, 1.0f);
	}
}
