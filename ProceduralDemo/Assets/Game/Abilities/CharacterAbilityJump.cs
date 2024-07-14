using UnityEngine;

[CreateAssetMenu(fileName = "New Jump Ability", menuName = "Character/Ability/Jump")]
public class CharacterAbilityJump : SOCharacterAbility
{
	[SerializeField]
	private float m_JumpForce = 20.0f;

	private PlayerRoot m_Player = null;

	public override void Initalize(PlayerRoot pPlayer)
	{
		m_Player = pPlayer;
		m_Player.Input.Jump.OnPerformed.AddListener(OnJumpInput);
		// m_Player.OnGround.OnEnterEvent.AddListener(OnGroundEnter);
		// m_Player.OnGround.OnExitEvent.AddListener(OnGroundExit);
	}

	public override void Destory()
	{
		m_Player.Input.Jump.OnPerformed.RemoveListener(OnJumpInput);
		// m_Player.OnGround.OnEnterEvent.RemoveListener(OnGroundEnter);
		// m_Player.OnGround.OnExitEvent.RemoveListener(OnGroundExit);
	}

	// private void OnGroundEnter()
	// {

	// }

	// private void OnGroundExit()
	// {

	// }

	private void OnJumpInput()
	{
		if (!m_Player.OnGround.IsGrounded)
		{
			return;
		}
		m_Player.Movement.SetVelocity(Vector3.up * m_JumpForce);
	}
}
