using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crouch Ability", menuName = "Character/Ability/Player Crouch")]
public class SOPlayerAbilityCrouch : SOCharacterAbility
{
	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer) => new PlayerAbilityCrouch(pPlayer, this);
}

public class PlayerAbilityCrouch : CharacterAbility<SOPlayerAbilityCrouch>
{
	public PlayerAbilityCrouch(PlayerRoot pPlayer, SOPlayerAbilityCrouch pData) : base(pPlayer, pData) { }

	protected override void Initalize()
	{
		Root.Input.Crouch.OnPerformed.AddListener(OnCrouchInputPerformed);
		Root.Input.Crouch.OnCanceled.AddListener(OnCrouchInputCanceled);
		Root.OnGround.OnAirEnterEvent.AddListener(OnAirEnter);
		Root.OnGround.OnAirExitEvent.AddListener(OnAirExit);
	}

	protected override void DestroyInternal()
	{
		Root.Input.Crouch.OnPerformed.RemoveListener(OnCrouchInputPerformed);
		Root.Input.Crouch.OnCanceled.RemoveListener(OnCrouchInputCanceled);
		Root.OnGround.OnAirEnterEvent.RemoveListener(OnAirEnter);
		Root.OnGround.OnAirExitEvent.RemoveListener(OnAirExit);
	}

	protected override void ActivateInternal()
	{
		// TODO: Modify speed
		// TODO: Modify collision
		// TODO: Set animations
	}

	protected override void DeactivateInternal()
	{
		
	}

	private void OnCrouchInputPerformed()
	{
		if (!Root.OnGround.IsInAir)
		{
			Activate();
		}
	}

	private void OnCrouchInputCanceled()
	{
		if (IsActive)
		{
			Deactivate();
		}
	}

	private void OnAirEnter()
	{
		if (IsActive)
		{
			Deactivate();
		}
		if (Root.Input.Crouch.IsToggle)
		{
			Root.Input.Crouch.Clear();
		}
	}

	private void OnAirExit()
	{
		if (!IsActive && Root.Input.Crouch.Input)
		{
			Activate();
		}
	}
}
