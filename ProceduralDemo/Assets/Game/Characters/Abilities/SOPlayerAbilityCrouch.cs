using ODev.GameStats;
using ODev.Picker;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Crouch Ability", menuName = "Character/Ability/Player Crouch")]
public class SOPlayerAbilityCrouch : SOCharacterAbility
{
	[AssetNonNull] public FloatGameStatModifier m_SpeedModifier;
	public FloatGameStatModifier SpeedModifier => m_SpeedModifier;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilityCrouch(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilityCrouch : CharacterAbility<SOPlayerAbilityCrouch>
{
	public PlayerAbilityCrouch(PlayerRoot pPlayer, SOPlayerAbilityCrouch pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	protected override void Initalize() { }

	protected override void DestroyInternal() { }

	protected override bool CanActivateUpdate()
	{
		return Root.Input.Crouch.Input;
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

		if (Root.Input.Crouch.IsToggle)
		{
			Root.Input.Crouch.Clear();
		}
	}
}
