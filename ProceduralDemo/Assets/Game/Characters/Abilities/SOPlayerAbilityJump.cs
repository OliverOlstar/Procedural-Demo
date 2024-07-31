using UnityEngine;

[CreateAssetMenu(fileName = "New Jump Ability", menuName = "Character/Ability/Player Jump")]
public class SOPlayerAbilityJump : SOCharacterAbility
{
	[SerializeField]
	private float m_JumpForce = 20.0f;
	public float JumpForce => m_JumpForce;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer) => new PlayerAbilityJump(pPlayer, this);
}

public class PlayerAbilityJump : CharacterAbility<SOPlayerAbilityJump>
{
	public PlayerAbilityJump(PlayerRoot pPlayer, SOPlayerAbilityJump pData) : base(pPlayer, pData) { }

	public override void Initalize()
	{
		Root.Input.Jump.OnPerformed.AddListener(OnJumpInput);
		// m_Player.OnGround.OnEnterEvent.AddListener(OnGroundEnter);
		// m_Player.OnGround.OnExitEvent.AddListener(OnGroundExit);
	}

	public override void Destroy()
	{
		Root.Input.Jump.OnPerformed.RemoveListener(OnJumpInput);
		// m_Player.OnGround.OnEnterEvent.RemoveListener(OnGroundEnter);
		// m_Player.OnGround.OnExitEvent.RemoveListener(OnGroundExit);
	}

	// private void OnGroundEnter()
	// private void OnGroundExit()

	private void OnJumpInput()
	{
		if (!Root.OnGround.IsOnGround)
		{
			return;
		}
		Root.Movement.SetVelocityY(Data.JumpForce);
	}
}
