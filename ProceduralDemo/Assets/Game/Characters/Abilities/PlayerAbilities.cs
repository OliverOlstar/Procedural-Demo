using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using ODev.Util;

// TODO: Abilities canceling other abilities
// TODO: Abilities blocking other abilities

[System.Serializable]
public class PlayerAbilities
{
	[SerializeField]
	private Mono.Updateable m_Updateable = new(Mono.Type.Fixed, Mono.Priorities.CharacterAbility);
	[SerializeField, DisableInPlayMode, ODev.Picker.AssetNonNull]
	private SOCharacterAbility[] m_Abilities = new SOCharacterAbility[0];
	[SerializeField]
	private float m_InputBufferSeconds = 0.2f;

	private readonly List<ICharacterAbility> m_AbilityInstances = new();
	private readonly List<int> m_LastInputedAbilities = new(2);
	private float m_LastInputedSeconds = 0.0f;

	public void Initalize(PlayerRoot pRoot)
	{
		for (int i = 0; i < m_Abilities.Length; i++)
		{
			int index = i;
			m_AbilityInstances.Add(m_Abilities[i].CreateInstance(pRoot, (bool performed) => OnAbilityInputRecieved(index, performed)));
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
		if (m_LastInputedSeconds > 0.0f)
		{
			for (int i = 0; i < m_LastInputedAbilities.Count; i++)
			{
				if (!m_AbilityInstances[i].IsActive && m_AbilityInstances[i].TryActivate())
				{
					m_LastInputedSeconds = -1.0f;
					break;
				}
			}
			m_LastInputedSeconds -= pDeltaTime;
		}

		for (int i = 0; i < m_AbilityInstances.Count; i++)
		{
			ICharacterAbility ability = m_AbilityInstances[i];
			if (ability.IsActive || ability.TryActivateUpdate())
			{
				ability.ActiveTick(pDeltaTime);
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

	internal void OnAbilityInputRecieved(int pIndex, bool pPerformed)
	{
		//ODev.Util.Debug.Log($"{pIndex} {m_AbilityInstances[pIndex].GetType()} -> {pPerformed}", typeof(PlayerAbilities));
		if (pPerformed && !m_AbilityInstances[pIndex].IsActive)
		{
			if (!m_AbilityInstances[pIndex].TryActivate())
			{
				AddLastInputedAbility(pIndex);
			}
		}
		if (!pPerformed && m_AbilityInstances[pIndex].IsActive)
		{
			m_AbilityInstances[pIndex].Deactivate();
		}
	}

	private void AddLastInputedAbility(int pIndex)
	{
		if (!m_LastInputedSeconds.Approximately(m_InputBufferSeconds))
		{
			m_LastInputedAbilities.Clear();
		}
		m_LastInputedSeconds = m_InputBufferSeconds;
		m_LastInputedAbilities.Add(pIndex);
	}
}
