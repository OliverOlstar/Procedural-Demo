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

	protected override void Initalize()
	{
		Root.Input.Jump.OnPerformed.AddListener(OnJumpInput);
	}
	protected override void DestroyInternal()
	{
		Root.Input.Jump.OnPerformed.RemoveListener(OnJumpInput);
	}

	protected override void ActivateInternal()
	{
		Root.Movement.SetVelocityY(Data.JumpForce);
		Deactivate();
	}
	protected override void DeactivateInternal() { }

	private void OnJumpInput()
	{
		if (Root.OnGround.IsOnGround)
		{
			Activate();
		}
	}
}
