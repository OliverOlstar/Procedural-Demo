using ODev.GameStats;
using ODev.Picker;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Crouch Ability", menuName = "Character/Ability/Player Crouch")]
public class SOPlayerAbilityCrouch : SOCharacterAbility
{
	[SerializeField, AssetNonNull]
	public FloatGameStatModifier m_SpeedModifier;
	public FloatGameStatModifier SpeedModifier => m_SpeedModifier;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilityCrouch(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilityCrouch : CharacterAbility<SOPlayerAbilityCrouch>
{
	public PlayerAbilityCrouch(PlayerRoot pPlayer, SOPlayerAbilityCrouch pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	protected override void Initalize()
	{
		Root.OnGround.OnAirEnterEvent.AddListener(OnAirEnter);
		// Root.OnGround.OnAirExitEvent.AddListener(OnAirExit);
	}

	protected override void DestroyInternal()
	{
		Root.OnGround.OnAirEnterEvent.RemoveListener(OnAirEnter);
		// Root.OnGround.OnAirExitEvent.RemoveListener(OnAirExit);
	}

	protected override bool CanActivateUpdate()
	{
		return Root.Input.Crouch.Input && !Root.OnGround.IsInAir;
	}

	protected override void ActivateInternal()
	{
		Data.SpeedModifier.Apply(Root.Movement.MaxVelocity);
		Root.Input.Crouch.RegisterOnCanceled(Deactivate);

		// TODO: Modify collision
		// TODO: Set animations
	}

	protected override void DeactivateInternal()
	{
		Data.SpeedModifier.Remove(Root.Movement.MaxVelocity);
		Root.Input.Crouch.DeregisterOnCanceled(Deactivate);
	}

	private void OnAirEnter()
	{
		Deactivate();
		if (Root.Input.Crouch.IsToggle)
		{
			Root.Input.Crouch.Clear();
		}
	}

	// private void OnAirExit()
	// {
	// 	if (!IsActive && Root.Input.Crouch.Input)
	// 	{
	// 		Activate();
	// 	}
	// }
}
