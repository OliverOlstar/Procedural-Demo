using System;
using BandoWare.GameplayTags;
using ODev.Picker;
using ODev.PoseAnimator;
using ODev.Util;
using UnityEngine;

[Serializable]
public class PoseAnimatorSlide : PoseAnimatorControllerBase
{	
	[SerializeField, AssetNonNull] private SOPoseAnimation m_Animation = null;
	[SerializeField] private GameplayTag m_SlideTag;

	[Header("Overall Weight")]
	[SerializeField] private float m_WeightSpring = 100.0f;
	[SerializeField] private float m_WeightDamper = 10.0f;

	private bool m_IsSliding = false;
	private int m_Handle = -1;
	private float m_Weight01 = 0.0f;
	private float m_WeightVelocity = 0.0f;

	private bool IsSliding => m_IsSliding || Root.OnGround.IsOnSlope;

	protected override void Setup()
	{
		m_Handle = Animator.GetHandle(m_Animation);
		Controller.CenterOfMassBounce.AddBounce(m_Handle, 0.0f);

		Root.Abilities.OnAbilityActivated += OnAbilityActivated;
		Root.Abilities.OnAbilityDeactivated += OnAbilityDeactivated;
	}

	public override void Destroy()
	{
		Root.Abilities.OnAbilityActivated -= OnAbilityActivated;
		Root.Abilities.OnAbilityDeactivated -= OnAbilityDeactivated;
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

	private void OnAbilityActivated(GameplayTagContainer pTags)
	{
		if (pTags.HasTag(m_SlideTag))
		{
			m_IsSliding = true;
		}
	}

	private void OnAbilityDeactivated(GameplayTagContainer pTags)
	{
		if (pTags.HasTag(m_SlideTag))
		{
			m_IsSliding = false;
		}
	}
}