using UnityEngine;
using ODev.Util;

[CreateAssetMenu(fileName = "New Sprint Ability", menuName = "Character/Ability/Player Sprint")]
public class SOPlayerAbilitySprint : SOCharacterAbility
{
	[SerializeField]
	public float m_SprintPercent = 1.0f;
	public float SprintPercent => m_SprintPercent;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer) => new PlayerAbilitySprint(pPlayer, this);
}

public class PlayerAbilitySprint : CharacterAbility<SOPlayerAbilitySprint>
{
	private int? m_ModifyKey = null;

	public PlayerAbilitySprint(PlayerRoot pPlayer, SOPlayerAbilitySprint pData) : base(pPlayer, pData) { }

	protected override void Initalize()
	{
		Root.Input.Sprint.OnChanged.AddListener(OnSprintInput);
	}
	protected override void DestroyInternal()
	{
		Root.Input.Sprint.OnChanged.RemoveListener(OnSprintInput);
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

	protected override void ActivateInternal()
	{
		if (m_ModifyKey.HasValue)
		{
			Root.DevException("Tried adding modify when we already have one added");
			return;
		}
		m_ModifyKey = Root.Movement.Speed.AddPercentModify(Data.m_SprintPercent);
	}

	protected override void DeactivateInternal()
	{
		if (!m_ModifyKey.HasValue)
		{
			Root.DevException("Tried removing modify when don't have one");
			return;
		}
		if (!Root.Movement.Speed.TryRemovePercentModify(m_ModifyKey.Value))
		{
			Root.DevException("Failed to remove modify");
		}
		m_ModifyKey = null;
	}
}