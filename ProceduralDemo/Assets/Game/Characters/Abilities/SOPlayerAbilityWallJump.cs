using ODev.Util;
using UnityEngine;

[CreateAssetMenu(fileName = "New Wall Jump Ability", menuName = "Character/Ability/Player Wall Jump")]
public class SOPlayerAbilityWallJump : SOCharacterAbility
{
	[SerializeField]
	private float m_JumpForce = 20.0f;
	[SerializeField]
	private float m_PushOffForce = 20.0f;

	public float JumpForce => m_JumpForce;
	public float PushOffForce => m_PushOffForce;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer) => new PlayerAbilityWallJump(pPlayer, this);
}

public class PlayerAbilityWallJump : CharacterAbility<SOPlayerAbilityWallJump>
{
	public PlayerAbilityWallJump(PlayerRoot pPlayer, SOPlayerAbilityWallJump pData) : base(pPlayer, pData) { }

	protected override void Initalize()
	{
		Root.Input.Jump.OnPerformed.AddListener(OnJumpInput);
	}
	protected override void DestroyInternal()
	{
		Root.Input.Jump.OnPerformed.RemoveListener(OnJumpInput);
	}

	private void OnJumpInput()
	{
		if (!Root.OnGround.IsOnGround && Root.OnWall.IsOnWall)
		{
			Activate();
		}
	}

	protected override void ActivateInternal()
	{
		Root.Movement.SetVelocityY(Data.JumpForce);
		Vector3 direction = Vector3.Reflect(Root.Movement.VelocityXZ, Root.OnWall.HitInfo.normal);
		Root.Movement.SetVelocityXZ(direction.Horizontalize() * Data.PushOffForce);
		Deactivate();
	}
	protected override void DeactivateInternal() { }
}
