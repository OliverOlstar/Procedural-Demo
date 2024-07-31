using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class PlayerAbilities
{
	[SerializeField]
	private ODev.Util.Mono.Updateable m_Updateable = new(ODev.Util.Mono.Type.Fixed, ODev.Util.Mono.Priorities.CharacterAbility);
	[SerializeField, DisableInPlayMode, ODev.Picker.AssetNonNull]
	private SOCharacterAbility[] m_Abilities = new SOCharacterAbility[0];

	private readonly List<ICharacterAbility> m_AbilityInstances = new();

	public void Initalize(PlayerRoot pRoot)
	{
		for (int i = 0; i < m_Abilities.Length; i++)
		{
			m_AbilityInstances.Add(m_Abilities[i].CreateInstance(pRoot));
		}
		m_Updateable.Register(Tick);
	}

	public void Destroy()
	{
		for (int i = 0; i < m_AbilityInstances.Count; i++)
		{
			m_AbilityInstances[i].Destory();
		}
		m_AbilityInstances.Clear();
		m_Updateable.Deregister();
	}

	private void Tick(float pDeltaTime)
	{
		for (int i = 0; i < m_AbilityInstances.Count; i++)
		{
			if (m_AbilityInstances[i].IsActive || m_AbilityInstances[i].TryActivate())
			{
				m_AbilityInstances[i].ActiveTick(pDeltaTime);
			}
		}
	}

	public void CancelAllAbilities()
	{
		for (int i = 0; i < m_AbilityInstances.Count; i++)
		{
			if (m_AbilityInstances[i].IsActive)
			{
				m_AbilityInstances[i].Deactivate();
			}
		}
	}
}
