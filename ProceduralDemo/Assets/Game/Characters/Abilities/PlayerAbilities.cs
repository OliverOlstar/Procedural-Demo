using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class PlayerAbilities
{
	[SerializeField, DisableInPlayMode, ODev.Picker.AssetNonNull]
	private SOCharacterAbility[] m_Abilities = new SOCharacterAbility[0];
	
	private readonly List<ICharacterAbility> m_AbilityInstances = new();
	private PlayerRoot m_Root;

	public void Initalize(PlayerRoot pRoot)
	{
		for (int i = 0; i < m_Abilities.Length; i++)
		{
			m_AbilityInstances.Add(m_Abilities[i].CreateInstance(pRoot));
		}
		m_Root = pRoot;
		m_Root.Movement.PreCharacterMove.AddListener(PreCharacterTick);
		m_Root.Movement.PostCharacterMove.AddListener(PostCharacterTick);
	}

	public void PreCharacterTick(float pDeltaTime)
	{
		for (int i = 0; i < m_AbilityInstances.Count; i++)
		{
			m_AbilityInstances[i].PreCharacterTick(pDeltaTime);
		}
	}

	public void PostCharacterTick(float pDeltaTime)
	{
		for (int i = 0; i < m_AbilityInstances.Count; i++)
		{
			m_AbilityInstances[i].PostCharacterTick(pDeltaTime);
		}
	}

	public void Destroy()
	{
		m_Root.Movement.PreCharacterMove.RemoveListener(PreCharacterTick);
		m_Root.Movement.PostCharacterMove.RemoveListener(PostCharacterTick);

		for (int i = 0; i < m_AbilityInstances.Count; i++)
		{
			m_AbilityInstances[i].Destory();
		}
		m_AbilityInstances.Clear();
	}
}
