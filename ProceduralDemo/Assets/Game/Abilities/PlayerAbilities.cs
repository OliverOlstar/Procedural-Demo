using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class PlayerAbilities
{
	[SerializeField, DisableInPlayMode, ODev.Picker.AssetNonNull]
	private SOCharacterAbility[] m_Abilities = new SOCharacterAbility[0];
	
	private readonly List<SOCharacterAbility> m_AbilityInstances = new();

	private PlayerRoot m_Player = null;

	public void Initalize(PlayerRoot pPlayer)
	{
		m_Player = pPlayer;
		for (int i = 0; i < m_Abilities.Length; i++)
		{
			m_AbilityInstances.Add(Object.Instantiate(m_Abilities[i]));
			m_AbilityInstances[^1].Initalize(m_Player);
		}
	}

	public void Destroy()
	{
		for (int i = 0; i < m_AbilityInstances.Count; i++)
		{
			m_AbilityInstances[i].Destory();
			Object.Destroy(m_AbilityInstances[i]);
		}
		m_AbilityInstances.Clear();
	}
}
