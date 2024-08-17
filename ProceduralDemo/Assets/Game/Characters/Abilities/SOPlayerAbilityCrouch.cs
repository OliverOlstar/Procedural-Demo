using System;
using ODev.GameStats;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crouch Ability", menuName = "Character/Ability/Player Crouch")]
public class SOPlayerAbilityCrouch : SOCharacterAbility
{
	[SerializeField]
	public FloatGameStatModifier m_SpeedModifier = new();
	public FloatGameStatModifier SpeedModifier => m_SpeedModifier;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer) => new PlayerAbilityCrouch(pPlayer, this);
}

public class PlayerAbilityCrouch : CharacterAbility<SOPlayerAbilityCrouch>
{
	private FloatGameStatModifier m_ModifierInstance;

	public PlayerAbilityCrouch(PlayerRoot pPlayer, SOPlayerAbilityCrouch pData) : base(pPlayer, pData) { }

	protected override void Initalize()
	{
		m_ModifierInstance = FloatGameStatModifier.CreateCopy(Data.SpeedModifier);
		
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
		m_ModifierInstance.Apply(Root.Movement.MaxVelocity);

		// TODO: Modify collision
		// TODO: Set animations
	}

	protected override void DeactivateInternal()
	{
		m_ModifierInstance.Remove(Root.Movement.MaxVelocity);
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
