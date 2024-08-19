using ODev.Input;
using ODev.Util;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Wall Jump Ability", menuName = "Character/Ability/Player Wall Jump")]
public class SOPlayerAbilityWallJump : SOCharacterAbility
{
	[SerializeField]
	private float m_JumpForce = 20.0f;
	[SerializeField]
	private float m_PushOffForce = 20.0f;

	public float JumpForce => m_JumpForce;
	public float PushOffForce => m_PushOffForce;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction<bool> pOnInputRecived) => new PlayerAbilityWallJump(pPlayer, this, pOnInputRecived);
}

public class PlayerAbilityWallJump : CharacterAbility<SOPlayerAbilityWallJump>
{
	public PlayerAbilityWallJump(PlayerRoot pPlayer, SOPlayerAbilityWallJump pData, UnityAction<bool> pOnInputRecived) : base(pPlayer, pData, pOnInputRecived) { }

	public override InputModule_Toggle InputActivate => Root.Input.Jump;

	protected override void Initalize()
	{

	}
	protected override void DestroyInternal()
	{

	}

	protected override bool CanActivate()
	{
		return !Root.OnGround.IsOnGround && Root.OnWall.IsOnWall;
	}

	protected override void ActivateInternal()
	{
		Root.Movement.SetVelocityY(Data.JumpForce);
		Vector3 direction = Vector3.Reflect(Root.Movement.VelocityXZ, Root.OnWall.HitInfo.normal);
		Root.Movement.SetVelocityXZ(direction.Horizontalize() * Data.PushOffForce);
	}
	protected override void DeactivateInternal()
	{
		if (Root.Movement.VelocityY <= 0.0f)
		{
			return;
		}
		Root.Movement.SetVelocityY(Root.Movement.VelocityY * 0.25f);
	}
}
