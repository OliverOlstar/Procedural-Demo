using System;
using ODev.GameStats;
using ODev.Input;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Crouch Ability", menuName = "Character/Ability/Player Crouch")]
public class SOPlayerAbilityCrouch : SOCharacterAbility
{
	[SerializeField]
	public FloatGameStatModifier m_SpeedModifier = new();
	public FloatGameStatModifier SpeedModifier => m_SpeedModifier;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilityCrouch(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilityCrouch : CharacterAbility<SOPlayerAbilityCrouch>
{
	private FloatGameStatModifier m_ModifierInstance;

	public PlayerAbilityCrouch(PlayerRoot pPlayer, SOPlayerAbilityCrouch pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	public override IInputTrigger InputActivate => Root.Input.Crouch;

	protected override void Initalize()
	{
		m_ModifierInstance = FloatGameStatModifier.CreateCopy(Data.SpeedModifier);
		
		Root.OnGround.OnAirEnterEvent.AddListener(OnAirEnter);
		Root.OnGround.OnAirExitEvent.AddListener(OnAirExit);
	}

	protected override void DestroyInternal()
	{
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

	protected override bool CanActivate()
	{
		return !Root.OnGround.IsInAir;
	}

	private void OnAirEnter()
	{
		Deactivate();
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
