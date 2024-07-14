using UnityEngine;

[CreateAssetMenu(fileName = "New Sprint Ability", menuName = "Character/Ability/Sprint")]
public class CharacterAbilitySprint : SOCharacterAbility
{
	[SerializeField]
	private float m_SprintPercent = 1.0f;

	private PlayerRoot m_Player = null;
	private int? m_ModifyKey = null;

	public override void Initalize(PlayerRoot pPlayer)
	{
		m_Player = pPlayer;
		m_Player.Input.Sprint.onChanged.AddListener(OnSprintInput);
	}

	public override void Destory()
	{
		m_Player.Input.Sprint.onChanged.RemoveListener(OnSprintInput);
	}

	private void OnSprintInput(bool pPerformed)
	{
		if (pPerformed)
		{
			if (m_ModifyKey.HasValue)
			{
				ODev.Util.Debug.DevException("Tried adding modify when we already have one added", this);
				return;
			}
			m_ModifyKey = m_Player.Movement.Speed.AddPercentModify(m_SprintPercent);
			return;
		}
		if (!m_ModifyKey.HasValue)
		{
			ODev.Util.Debug.DevException("Tried removing modify when don't have one", this);
			return;
		}
		if (!m_Player.Movement.Speed.TryRemovePercentModify(m_ModifyKey.Value))
		{
			ODev.Util.Debug.DevException("Failed to remove modify", this);
		}
		m_ModifyKey = null;
	}
}
