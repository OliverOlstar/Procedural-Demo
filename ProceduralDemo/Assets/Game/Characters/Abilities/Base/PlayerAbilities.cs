using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using ODev.Util;
using ODev.Input;
using UnityEngine.Pool;
using ODev.Updateables;
using BandoWare.GameplayTags;

[Serializable]
public class PlayerAbilities
{
	public event Action<IReadOnlyGameplayTagContainer> OnAbilityActivated = delegate { };
	public event Action<IReadOnlyGameplayTagContainer> OnAbilityDeactivated = delegate { };

	[SerializeField] private Updateable m_Updateable = new(UpdateableType.Fixed, UpdateablePriority.CharacterAbility);
	[SerializeField, DisableInPlayMode, ODev.Picker.AssetNonNull] private SOCharacterAbility[] m_Abilities = new SOCharacterAbility[0];
	[SerializeField] private float m_InputBufferSeconds = 0.2f;

	private readonly List<ICharacterAbility> m_AbilityInstances = new();
	private readonly List<int> m_LastInputedAbilities = new(2);
	private float m_LastInputedSeconds = 0.0f;
	private bool m_InputActivatedThisFrame = false;
	private readonly List<ICharacterAbility> m_ActiveAbilities = new();

	[SerializeField] private GameObjectGameplayTagContainer m_ActiveTags;
	private readonly GameplayTagCountContainer m_BlockedTags = new();

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
		m_Updateable.UnRegister();
	}

	public void ActivateAbilityByTag(GameplayTag pTag)
	{
		foreach (ICharacterAbility ability in m_AbilityInstances)
		{
			ability.GetTags(out var tags, out _, out _);
			if (!tags.HasTag(pTag))
			{
				continue;
			}
			if (ability.TryActivate(m_ActiveTags.GameplayTagContainer, m_BlockedTags))
			{
				break;
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

	private void Tick(float pDeltaTime)
	{
		m_InputActivatedThisFrame = false;

		if (m_LastInputedSeconds > 0.0f)
		{
			for (int i = 0; i < m_LastInputedAbilities.Count; i++)
			{
				ICharacterAbility ability = m_AbilityInstances[m_LastInputedAbilities[i]];
				if (ability.IsActive || !ability.TryActivate(m_ActiveTags.GameplayTagContainer, m_BlockedTags))
				{
					continue;
				}
				if ((ability.InputActivate is IInputBool input) && !input.Input)
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
			if (ability.IsActive || ability.TryActivateUpdate(m_ActiveTags.GameplayTagContainer, m_BlockedTags))
			{
				ability.ActiveTick(pDeltaTime);
			}
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
			if (m_AbilityInstances[pIndex].TryActivate(m_ActiveTags.GameplayTagContainer, m_BlockedTags))
			{
				m_InputActivatedThisFrame = true;
				m_LastInputedAbilities.Clear();
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

	internal void RecievedAbilityActivated(ICharacterAbility pAbility)
	{
		pAbility.GetTags(out var tags, out var cancelTags, out var blockTags);
		OnAbilityActivated.Invoke(tags);

		List<ICharacterAbility> tempList = ListPool<ICharacterAbility>.Get();
		tempList.AddRange(m_ActiveAbilities);
		foreach (ICharacterAbility ability in tempList)
		{
			ability.TryCancel(tags, cancelTags);
		}
		ListPool<ICharacterAbility>.Release(tempList);

		m_ActiveAbilities.Add(pAbility);
		m_ActiveTags.GameplayTagContainer.AddTags(tags);
		m_BlockedTags.AddTags(blockTags);
	}

	internal void RecievedAbilityDeactivated(ICharacterAbility pAbility)
	{
		pAbility.GetTags(out var tags, out _, out var blockTags);
		OnAbilityDeactivated.Invoke(tags);
		m_ActiveAbilities.Remove(pAbility);

		m_ActiveTags.GameplayTagContainer.RemoveTags(tags);
		m_BlockedTags.RemoveTags(blockTags);
	}
}
