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

	[Header("Cooldown")]
	[SerializeField] private float m_Cooldown = 0.0f;
	[SerializeField] private CooldownTrigger m_CooldownTrigger = CooldownTrigger.OnActivate;

	[Header("Local")]
	[SerializeField] private bool m_LogSelf = false;

	public bool LogSelf => m_LogSelf;
	public float Cooldown => m_Cooldown;

	public bool IsCooldownTrigger(bool pOnActive) => m_CooldownTrigger switch
	{
		CooldownTrigger.OnActivate => pOnActive,
		CooldownTrigger.OnDeactive => !pOnActive,
		CooldownTrigger.Both => true,
		_ => throw new NotImplementedException(),
	};

	public bool ShouldCancel(GameplayTagContainer othersTags, GameplayTagContainer othersCancelTags)
	{
		return m_Tags.HasAny(othersCancelTags) || m_CanceledByTags.HasAny(othersTags);
	}

	public bool ShouldBlock(GameplayTagContainer othersTags, GameplayTagContainer othersBlockTags)
	{
		return m_Tags.HasAny(othersBlockTags) || m_BlockedByTags.HasAny(othersTags);
	}

	public void AddTags(ref GameplayTagContainer rActiveTags, ref GameplayTagContainer rBlockedTags)
	{
		rActiveTags = GameplayTagContainer.Union(m_Tags, rActiveTags);
		rBlockedTags = GameplayTagContainer.Union(m_BlockTags, rBlockedTags);
	}

	internal void GetTags(out GameplayTagContainer oTags, out GameplayTagContainer oCancelTags)
	{
		oTags = m_Tags;
		oCancelTags = m_CancelTags;
	}

	public abstract ICharacterAbility CreateInstance(PlayerRoot pRoot, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled);
}