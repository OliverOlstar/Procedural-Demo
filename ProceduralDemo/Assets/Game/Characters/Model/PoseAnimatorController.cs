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
	[SerializeField]
	private CardinalWheel m_Wheel = null;

	[Header("Controllers")]
	[SerializeField]
	private PoseAnimatorIdle m_Idle = new();
	[SerializeField]
	private PoseAnimatorLocomotion m_Locomotion = new();
	[SerializeField]
	private PoseAnimatorJump m_Jump = new();
	[SerializeField]
	private PoseAnimatorCrouch m_Crouch = new();
	[SerializeField]
	private CardinalWheelRadius m_WheelRadius = new();

	public CardinalWheel Wheel => m_Wheel;
	public CardinalWheelRadius WheelRadius => m_WheelRadius;

	private IEnumerable<PoseAnimatorControllerBase> GetControllers()
	{
		yield return m_Idle;
		yield return m_Locomotion;
		yield return m_Jump;
		yield return m_Crouch;
		yield return m_WheelRadius;
	}

	private void Start()
	{
		foreach (PoseAnimatorControllerBase controller in GetControllers())
		{
			controller.Setup(m_Root, m_Animator, this);
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
	}
}
