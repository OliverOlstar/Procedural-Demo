using UnityEngine;

[CreateAssetMenu(fileName = "New Glide Ability", menuName = "Character/Ability/Player Glide")]
public class SOPlayerAbilityGlide : SOCharacterAbility
{
	[SerializeField]
	private float m_GlideForce = 20.0f;
	public float GlideForce => m_GlideForce;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer) => new PlayerAbilityGlide(pPlayer, this);
}

public class PlayerAbilityGlide : CharacterAbility<SOPlayerAbilityGlide>
{
	public PlayerAbilityGlide(PlayerRoot pPlayer, SOPlayerAbilityGlide pData) : base(pPlayer, pData) { }

	protected override void Initalize()
	{
		// Player.Input.Glide.OnPerformed.AddListener(OnGlideInput);
		// m_Player.OnGround.OnEnterEvent.AddListener(OnGroundEnter);
		// m_Player.OnGround.OnExitEvent.AddListener(OnGroundExit);
		throw new System.NotImplementedException();
	}

	protected override void DestroyInternal()
	{
		// Player.Input.Glide.OnPerformed.RemoveListener(OnGlideInput);
		// m_Player.OnGround.OnEnterEvent.RemoveListener(OnGroundEnter);
		// m_Player.OnGround.OnExitEvent.RemoveListener(OnGroundExit);
		throw new System.NotImplementedException();
	}

	protected override void ActivateInternal()
	{
		throw new System.NotImplementedException();
	}

	protected override void DeactivateInternal()
	{
		throw new System.NotImplementedException();
	}

	// private void OnGroundEnter()
	// private void OnGroundExit()
}
