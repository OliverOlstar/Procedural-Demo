using ODev.Input;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Jump Ability", menuName = "Character/Ability/Player Jump")]
public class SOPlayerAbilityJump : SOCharacterAbility
{
	[SerializeField]
	private float m_JumpForce = 20.0f;
	public float JumpForce => m_JumpForce;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction<bool> pOnInputRecived) => new PlayerAbilityJump(pPlayer, this, pOnInputRecived);
}

public class PlayerAbilityJump : CharacterAbility<SOPlayerAbilityJump>
{
	public PlayerAbilityJump(PlayerRoot pPlayer, SOPlayerAbilityJump pData, UnityAction<bool> pOnInputRecived) : base(pPlayer, pData, pOnInputRecived) { }

	public override InputModule_Toggle InputActivate => Root.Input.Jump;

	protected override void Initalize()
	{

	}
	protected override void DestroyInternal()
	{

	}

	protected override bool CanActivate()
	{
		return Root.OnGround.IsOnGround;
	}

	protected override void ActivateInternal()
	{
		Root.Movement.SetVelocityY(Data.JumpForce);
		Deactivate();
	}
	protected override void DeactivateInternal() { }
}
