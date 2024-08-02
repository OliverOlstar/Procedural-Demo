using UnityEngine;
using ODev.Util;
using ODev.GameStats;

[CreateAssetMenu(fileName = "New Sprint Ability", menuName = "Character/Ability/Player Sprint")]
public class SOPlayerAbilitySprint : SOCharacterAbility
{
	[SerializeField]
	public FloatGameStatModifier m_Modifier = new();
	public FloatGameStatModifier Modifier => m_Modifier;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer) => new PlayerAbilitySprint(pPlayer, this);
}

public class PlayerAbilitySprint : CharacterAbility<SOPlayerAbilitySprint>
{
	private FloatGameStatModifier m_ModifierInstance;

	public PlayerAbilitySprint(PlayerRoot pPlayer, SOPlayerAbilitySprint pData) : base(pPlayer, pData) { }

	protected override void Initalize()
	{
		Root.Input.Sprint.OnChanged.AddListener(OnSprintInput);
		m_ModifierInstance = FloatGameStatModifier.CreateCopy(Data.Modifier);
	}
	protected override void DestroyInternal()
	{
		Root.Input.Sprint.OnChanged.RemoveListener(OnSprintInput);
	}

	protected override void ActivateInternal()
	{
		m_ModifierInstance.Apply(Root.Movement.MaxVelocity);
	}
	protected override void DeactivateInternal()
	{
		m_ModifierInstance.Remove(Root.Movement.MaxVelocity);
	}

	private void OnSprintInput(bool pPerformed)
	{
		if (pPerformed)
		{
			Activate();
			return;
		}
		Deactivate();
	}
}