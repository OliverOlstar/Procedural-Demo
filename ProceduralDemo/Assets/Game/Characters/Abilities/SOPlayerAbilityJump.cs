using System;
using ODev.Input;
using ODev.Util;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Jump Ability", menuName = "Character/Ability/Player Jump")]
public class SOPlayerAbilityJump : SOCharacterAbility
{
	[SerializeField]
	private float m_Force = 11.0f;
	[SerializeField]
	private float m_GraceSeconds = 0.25f;

	public float Force => m_Force;
	public float GraceSeconds => m_GraceSeconds;


	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction<bool> pOnInputRecived) => new PlayerAbilityJump(pPlayer, this, pOnInputRecived);
}

public class PlayerAbilityJump : CharacterAbility<SOPlayerAbilityJump>
{
	private float m_LastGroundedTime = 0.0f;

	public PlayerAbilityJump(PlayerRoot pPlayer, SOPlayerAbilityJump pData, UnityAction<bool> pOnInputRecived) : base(pPlayer, pData, pOnInputRecived) { }

	public override InputModule_Toggle InputActivate => Root.Input.Jump;

	protected override void Initalize()
	{
		Root.OnGround.OnGroundExitEvent.AddListener(OnGroundExit);
	}
	protected override void DestroyInternal()
	{
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
		if (Root.Movement.VelocityY <= 0.0f)
		{
			return;
		}
		Root.Movement.SetVelocityY(Root.Movement.VelocityY * 0.5f);
	}

	private void OnGroundExit()
	{
		m_LastGroundedTime = Time.time;
	}
}
