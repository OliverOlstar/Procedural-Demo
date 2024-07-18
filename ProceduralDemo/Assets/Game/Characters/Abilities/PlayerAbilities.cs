using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class PlayerAbilities
{
	[SerializeField, DisableInPlayMode, ODev.Picker.AssetNonNull]
	private SOCharacterAbility[] m_Abilities = new SOCharacterAbility[0];
	
	private readonly List<ICharacterAbility> m_AbilityInstances = new();

	public void Initalize(PlayerRoot pPlayer)
	{
		for (int i = 0; i < m_Abilities.Length; i++)
		{
			m_AbilityInstances.Add(m_Abilities[i].CreateInstance(pPlayer));
		}
	}

	public void Destroy()
	{
		for (int i = 0; i < m_AbilityInstances.Count; i++)
		{
			m_AbilityInstances[i].Destory();
		}
		m_AbilityInstances.Clear();
	}
}
