using System;
using ODev.Picker;
using ODev.PoseAnimator;
using ODev.Util;
using UnityEngine;

[System.Serializable]
public class PoseAnimatorSlide : PoseAnimatorControllerBase
{	
	[SerializeField, AssetNonNull]
	private SOPoseAnimation m_Animation = null;

	[Header("Overall Weight")]
	[SerializeField]
	private float m_WeightSpring = 100.0f;
	[SerializeField]
	private float m_WeightDamper = 10.0f;

	private bool m_IsSliding = false;
	private int m_Handle = -1;
	private float m_Weight01 = 0.0f;
	private float m_WeightVelocity = 0.0f;

	private bool IsSliding => m_IsSliding || Root.OnGround.IsOnSlope;

	protected override void Setup()
	{
		m_Handle = Animator.Add(m_Animation);
		Controller.CenterOfMassBounce.AddBounce(m_Handle, 0.0f);

		Root.Abilities.OnAbilityActivated.AddListener(OnAbilityActivated);
		Root.Abilities.OnAbilityDeactivated.AddListener(OnAbilityDeactivated);
	}

	public override void Destroy()
	{
		Root.Abilities.OnAbilityActivated.RemoveListener(OnAbilityActivated);
		Root.Abilities.OnAbilityDeactivated.RemoveListener(OnAbilityDeactivated);
	}

	public override void Tick(float pDeltaTime)
	{
		WeightSpringDamper(pDeltaTime);
	}

	private void WeightSpringDamper(float pDeltaTime)
	{
		m_Weight01 = Func.SpringDamper(m_Weight01, IsSliding ? 1.0f : 0.0f, ref m_WeightVelocity, m_WeightSpring, m_WeightDamper, pDeltaTime);
		Animator.SetWeight(m_Handle, 0.0f, m_Weight01);
	}

	private void OnAbilityActivated(Type pAbilityReferenceType)
	{
		if (pAbilityReferenceType == typeof(SOPlayerAbilitySlide))
		{
			m_IsSliding = true;
		}
	}

	private void OnAbilityDeactivated(Type pAbilityReferenceType)
	{
		if (pAbilityReferenceType == typeof(SOPlayerAbilitySlide))
		{
			m_IsSliding = false;
		}
	}
}