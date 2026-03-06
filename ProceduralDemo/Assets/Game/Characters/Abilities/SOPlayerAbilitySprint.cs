using UnityEngine;
using UnityEngine.Events;
using ODev.Input;
using ODev.GameStats;
using ODev.Picker;

[CreateAssetMenu(fileName = "New Sprint Ability", menuName = "Character/Ability/Player Sprint")]
public class SOPlayerAbilitySprint : SOCharacterAbility
{
	[SerializeField, AssetNonNull]
	private FloatGameStatModifier m_Modifier;
	public FloatGameStatModifier Modifier => m_Modifier;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilitySprint(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilitySprint : CharacterAbility<SOPlayerAbilitySprint>
{
	public PlayerAbilitySprint(PlayerRoot pPlayer, SOPlayerAbilitySprint pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	public override IInputTrigger InputActivate => Root.Input.Sprint;

	protected override void Initalize() { }
	protected override void DestroyInternal() { }

	protected override bool CanActivate()
	{
		return true;
	}

	protected override void ActivateInternal()
	{
		Data.Modifier.Apply(Root.Movement.MaxVelocity);
	}
	protected override void DeactivateInternal()
	{
		Data.Modifier.Remove(Root.Movement.MaxVelocity);
	}
}