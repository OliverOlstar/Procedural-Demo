using UnityEngine;
using ODev.Util;
using ODev.GameStats;
using UnityEngine.Events;
using ODev.Input;

[CreateAssetMenu(fileName = "New Sprint Ability", menuName = "Character/Ability/Player Sprint")]
public class SOPlayerAbilitySprint : SOCharacterAbility
{
	[SerializeField]
	public FloatGameStatModifier m_Modifier = new();
	public FloatGameStatModifier Modifier => m_Modifier;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilitySprint(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilitySprint : CharacterAbility<SOPlayerAbilitySprint>
{
	private FloatGameStatModifier m_ModifierInstance;

	public PlayerAbilitySprint(PlayerRoot pPlayer, SOPlayerAbilitySprint pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	public override IInputTrigger InputActivate => Root.Input.Sprint;

	protected override void Initalize()
	{
		m_ModifierInstance = FloatGameStatModifier.CreateCopy(Data.Modifier);
	}
	protected override void DestroyInternal()
	{

	}

	protected override bool CanActivate()
	{
		return true;
	}

	protected override void ActivateInternal()
	{
		m_ModifierInstance.Apply(Root.Movement.MaxVelocity);
	}
	protected override void DeactivateInternal()
	{
		m_ModifierInstance.Remove(Root.Movement.MaxVelocity);
	}
}