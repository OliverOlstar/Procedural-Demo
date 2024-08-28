using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using ODev.Util;
using ODev.Input;

// TODO: Abilities canceling other abilities
// TODO: Abilities blocking other abilities

[Serializable]
public class PlayerAbilities
{
	public UnityEvent<Type> OnAbilityActivated = new();
	public UnityEvent<Type> OnAbilityDeactivated = new();

	[SerializeField]
	private Mono.Updateable m_Updateable = new(Mono.Type.Fixed, Mono.Priorities.CharacterAbility);
	[SerializeField, DisableInPlayMode, ODev.Picker.AssetNonNull]
	private SOCharacterAbility[] m_Abilities = new SOCharacterAbility[0];
	[SerializeField]
	private float m_InputBufferSeconds = 0.2f;

	private readonly List<ICharacterAbility> m_AbilityInstances = new();
	private readonly List<int> m_LastInputedAbilities = new(2);
	private float m_LastInputedSeconds = 0.0f;
	private bool m_InputActivatedThisFrame = false;

	public void Initalize(PlayerRoot pRoot)
	{
		for (int i = 0; i < m_Abilities.Length; i++)
		{
			int index = i;
			m_AbilityInstances.Add(m_Abilities[i].CreateInstance(pRoot, () => OnAbilityInputRecieved(index, true), () => OnAbilityInputRecieved(index, false)));
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
		m_InputActivatedThisFrame = false;

		if (m_LastInputedSeconds > 0.0f)
		{
			for (int i = 0; i < m_LastInputedAbilities.Count; i++)
			{
				ICharacterAbility ability = m_AbilityInstances[m_LastInputedAbilities[i]];
				if (ability.IsActive || !ability.TryActivate())
				{
					continue;
				}
				if ((ability.InputActivate is IInputBool input) && input.Input)
				{
					ability.Deactivate();
				}
				m_LastInputedSeconds = -1.0f;
				break;
			}
			m_LastInputedSeconds -= pDeltaTime;
		}

		for (int i = 0; i < m_AbilityInstances.Count; i++)
		{
			ICharacterAbility ability = m_AbilityInstances[i];
			ability.SystemsTick(pDeltaTime);
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
			m_AbilityInstances[i].Deactivate();
		}
	}

	internal void OnAbilityInputRecieved(int pIndex, bool pPerformed)
	{
		// this.Log($"{pIndex} {m_AbilityInstances[pIndex].GetType()} -> {pPerformed}");
		if (pPerformed)
		{
			if (m_InputActivatedThisFrame || m_AbilityInstances[pIndex].IsActive)
			{
				return;
			}
			if (m_AbilityInstances[pIndex].TryActivate())
			{
				m_InputActivatedThisFrame = true;
				return;
			}
			AddLastInputedAbility(pIndex);
		}
		else
		{
			m_AbilityInstances[pIndex].Deactivate();
		}
	}

	private void AddLastInputedAbility(int pIndex)
	{
		// this.Log($"{pIndex} {m_AbilityInstances[pIndex].GetType()}");
		if (!m_LastInputedSeconds.Approximately(m_InputBufferSeconds))
		{
			m_LastInputedAbilities.Clear();
		}
		m_LastInputedSeconds = m_InputBufferSeconds;
		m_LastInputedAbilities.Add(pIndex);
	}
}
