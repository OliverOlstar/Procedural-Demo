using System.Collections.Generic;
using ODev;
using ODev.Picker;
using ODev.PoseAnimator;
using UnityEngine;

public class PoseAnimatorController : UpdateableMonoBehaviour
{
	[SerializeField]
	private PoseAnimator m_Animator = null;
	[SerializeField]
	private PlayerRoot m_Root = null;

	[Header("Controllers")]
	[SerializeField]
	private PoseAnimatorIdle m_Idle = new();
	[SerializeField]
	private PoseAnimatorLocomotion m_Locomotion = new();
	[SerializeField]
	private PoseAnimatorJump m_Jump = new();

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

	// private float m_Progress;
	// private float m_ProgressVelocity = 0.0f;

	private IEnumerable<PoseAnimatorControllerBase> GetControllers()
	{
		yield return m_Idle;
		yield return m_Locomotion;
		yield return m_Jump;
	}

	private void Start()
	{
		foreach (PoseAnimatorControllerBase controller in GetControllers())
		{
			controller.Setup(m_Root, m_Animator);
		}
	}
	
	private void OnDestroy()
	{
		foreach (PoseAnimatorControllerBase controller in GetControllers())
		{
			controller.Destroy();
		}
	}

	protected override void Tick(float pDeltaTime)
	{
		foreach (PoseAnimatorControllerBase controller in GetControllers())
		{
			controller.Tick(pDeltaTime);
		}

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
}
