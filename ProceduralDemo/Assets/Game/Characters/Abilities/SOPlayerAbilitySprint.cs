using UnityEngine;

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

	public override void Initalize()
	{
		Root.Input.Sprint.OnChanged.AddListener(OnSprintInput);
	}

	public override void Destroy()
	{
		Root.Input.Sprint.OnChanged.RemoveListener(OnSprintInput);
	}

	private void OnSprintInput(bool pPerformed)
	{
		if (pPerformed)
		{
			if (m_ModifyKey.HasValue)
			{
				ODev.Util.Debug.DevException("Tried adding modify when we already have one added", Root);
				return;
			}
			m_ModifyKey = Root.Movement.Speed.AddPercentModify(Data.m_SprintPercent);
			return;
		}
		if (!m_ModifyKey.HasValue)
		{
			ODev.Util.Debug.DevException("Tried removing modify when don't have one", Root);
			return;
		}
		if (!Root.Movement.Speed.TryRemovePercentModify(m_ModifyKey.Value))
		{
			ODev.Util.Debug.DevException("Failed to remove modify", Root);
		}
		m_ModifyKey = null;
	}
}