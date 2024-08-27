using System;
using ODev.Input;
using ODev.Util;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Jump Ability", menuName = "Character/Ability/Player Jump")]
public class SOPlayerAbilityJump : SOCharacterAbility
{
	[Space, SerializeField]
	private float m_Force = 11.0f;
	[SerializeField]
	private float m_GraceSeconds = 0.25f;
	[Space, SerializeField, Range(0.0f, 1.0f)]
	private float m_CancelVelocityPercent = 0.5f;
	[SerializeField]
	private float m_CancelMinVelocity = 5.0f;

	public float Force => m_Force;
	public float GraceSeconds => m_GraceSeconds;
	public float CancelVelocityPercent => m_CancelVelocityPercent;
	public float CancelMinVelocity => m_CancelMinVelocity;


	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilityJump(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilityJump : CharacterAbility<SOPlayerAbilityJump>
{
	private float m_LastGroundedTime = 0.0f;

	public PlayerAbilityJump(PlayerRoot pPlayer, SOPlayerAbilityJump pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	public override IInputTrigger InputActivate => Root.Input.Jump;

	protected override void Initalize()
	{
		Root.OnGround.OnGroundEnterEvent.AddListener(OnGroundEnter);
		Root.OnGround.OnGroundExitEvent.AddListener(OnGroundExit);
	}

	protected override void DestroyInternal()
	{
		Root.OnGround.OnGroundEnterEvent.RemoveListener(OnGroundEnter);
		Root.OnGround.OnGroundExitEvent.RemoveListener(OnGroundExit);
	}

	protected override bool CanActivate()
	{
		return Root.OnGround.IsOnGround || (Time.time - m_LastGroundedTime) < Data.GraceSeconds;
	}

	protected override void ActivateInternal()
	{
		Root.Movement.SetVelocityY(Data.Force);
	}
	protected override void DeactivateInternal()
	{
		if (Root.Movement.VelocityY <= Data.CancelMinVelocity)
		{
			return;
		}
		float velocity = Mathf.Max(Root.Movement.VelocityY * Data.CancelVelocityPercent, Data.CancelMinVelocity);
		Root.Movement.SetVelocityY(velocity);
	}

	private void OnGroundEnter()
	{
		Deactivate();
	}

	private void OnGroundExit()
	{
		m_LastGroundedTime = Time.time;
	}
}
