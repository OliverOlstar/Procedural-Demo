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

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction<bool> pOnInputRecived) => new PlayerAbilitySprint(pPlayer, this, pOnInputRecived);
}

public class PlayerAbilitySprint : CharacterAbility<SOPlayerAbilitySprint>
{
	private FloatGameStatModifier m_ModifierInstance;

	public PlayerAbilitySprint(PlayerRoot pPlayer, SOPlayerAbilitySprint pData, UnityAction<bool> pOnInputRecived) : base(pPlayer, pData, pOnInputRecived) { }

	public override InputModule_Toggle InputActivate => Root.Input.Sprint;

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