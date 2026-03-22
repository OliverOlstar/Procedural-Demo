using System;
using BandoWare.GameplayTags;
using UnityEngine;
using UnityEngine.Events;

public abstract class SOCharacterAbility : ScriptableObject
{
	private enum CooldownTrigger
	{
		OnActivate,
		OnDeactive,
		Both
	}

	[Header("Tags")]
	[SerializeField] private GameplayTagContainer m_Tags;
	[Space, SerializeField] private GameplayTagContainer m_CancelTags;
	[SerializeField] private GameplayTagContainer m_BlockTags;
	[Space, SerializeField] private GameplayTagContainer m_CanceledByTags;
	[SerializeField] private GameplayTagContainer m_BlockedByTags;
	[SerializeField] private GameplayTagContainer m_RequiredTags; // Inverse of m_BlockedByTags

	[Header("Cooldown")]
	[SerializeField] private float m_Cooldown = 0.0f;
	[SerializeField] private CooldownTrigger m_CooldownTrigger = CooldownTrigger.OnActivate;

	[Header("Local")]
	[SerializeField] private bool m_LogSelf = false;

	public bool LogSelf => m_LogSelf;
	public float Cooldown => m_Cooldown;
	public GameplayTagContainer Tags => m_Tags;

	public bool IsCooldownTrigger(bool pOnActive) => m_CooldownTrigger switch
	{
		CooldownTrigger.OnActivate => pOnActive,
		CooldownTrigger.OnDeactive => !pOnActive,
		CooldownTrigger.Both => true,
		_ => throw new NotImplementedException(),
	};

	public bool ShouldCancel(IReadOnlyGameplayTagContainer othersTags, IReadOnlyGameplayTagContainer othersCancelTags)
	{
		return m_Tags.HasAny(othersCancelTags) || m_CanceledByTags.HasAny(othersTags);
	}

	public bool HasRequired(IReadOnlyGameplayTagContainer othersTags)
	{
		return m_RequiredTags.IsEmpty || othersTags.HasAll(m_RequiredTags);
	}

	public bool ShouldBlock(IReadOnlyGameplayTagContainer othersTags, IReadOnlyGameplayTagContainer othersBlockTags)
	{
		return othersBlockTags.HasAny(m_Tags) || othersTags.HasAny(m_BlockedByTags);
	}

	internal void GetTags(out IReadOnlyGameplayTagContainer oTags, out IReadOnlyGameplayTagContainer oCancelTags, out IReadOnlyGameplayTagContainer oBlockTags)
	{
		oTags = m_Tags;
		oCancelTags = m_CancelTags;
		oBlockTags = m_BlockTags;
	}

	public abstract ICharacterAbility CreateInstance(PlayerRoot pRoot, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled);
}