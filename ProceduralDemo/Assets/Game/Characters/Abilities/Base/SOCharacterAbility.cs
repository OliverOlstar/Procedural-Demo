using System;
using ODev.Util;
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
	[SerializeField]
	private AbilityTags m_Tags;
	[Space, SerializeField]
	private AbilityTags m_CancelTags;
	[SerializeField]
	private AbilityTags m_BlockTags;
	[Space, SerializeField]
	private AbilityTags m_CanceledByTags;
	[SerializeField]
	private AbilityTags m_BlockedByTags;

	[Header("Cooldown")]
	[SerializeField]
	private float m_Cooldown = 0.0f;
	[SerializeField]
	private CooldownTrigger m_CooldownTrigger = CooldownTrigger.OnActivate;

	[Header("Local")]
	[SerializeField]
	private bool m_LogSelf = false;

	public bool LogSelf => m_LogSelf;
	public float Cooldown => m_Cooldown;
	public bool IsCooldownTrigger(bool pOnActive) => m_CooldownTrigger switch
	{
		CooldownTrigger.OnActivate => pOnActive,
		CooldownTrigger.OnDeactive => !pOnActive,
		CooldownTrigger.Both => true,
		_ => throw new NotImplementedException(),
	};

	public bool ShouldCancel(AbilityTags othersTags, AbilityTags othersCancelTags)
	{
		return m_Tags.HasAnyFlag(othersCancelTags) || m_CanceledByTags.HasAnyFlag(othersTags);
	}
	public bool ShouldBlock(AbilityTags othersTags, AbilityTags othersBlockTags)
	{
		return m_Tags.HasAnyFlag(othersBlockTags) || m_BlockedByTags.HasAnyFlag(othersTags);
	}
	public void AddTags(ref AbilityTags rActiveTags, ref AbilityTags rBlockedTags)
	{
		rActiveTags |= m_Tags; rBlockedTags |= m_BlockTags;
	}
	internal void GetTags(out AbilityTags oTags, out AbilityTags oCancelTags)
	{
		oTags = m_Tags; oCancelTags = m_CancelTags;
	}

	public abstract ICharacterAbility CreateInstance(PlayerRoot pRoot, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled);
}