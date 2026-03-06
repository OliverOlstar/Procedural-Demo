using System;
using BandoWare.GameplayTags;
using ODev;
using ODev.Util;
using UnityEngine;

public class OnGroundGameplayTags : MonoBehaviour
{
	[SerializeField] private OnGround m_OnGround;
	[SerializeField] private CharacterOnWall m_OnWall;
	[SerializeField] private GameObjectGameplayTagContainer m_GameplayTags;
	[Space, SerializeField] private GameplayTag m_OnGroundTag;
	[SerializeField] private GameplayTag m_OnSlopeTag;
	[SerializeField] private GameplayTag m_InAirTag;
	[SerializeField] private GameplayTag m_OnWallTag;

	void Start()
    {
		m_OnGround.OnGroundEnterEvent.AddListener(OnGroundEnter);
		m_OnGround.OnGroundExitEvent.AddListener(OnGroundExit);
		m_OnGround.OnSlopeEnterEvent.AddListener(OnSlopeEnter);
		m_OnGround.OnSlopeExitEvent.AddListener(OnSlopeExit);
		m_OnGround.OnAirEnterEvent.AddListener(OnAirEnter);
		m_OnGround.OnAirExitEvent.AddListener(OnAirExit);
		m_OnWall.OnWallEnter.AddListener(OnWallEnter);
		m_OnWall.OnWallEnter.AddListener(OnWallExit);

		if (m_OnGround.IsOnGround)
		{
			OnGroundEnter();
		}
		else if (m_OnGround.IsOnSlope)
		{
			OnSlopeEnter();
		}
		else if (m_OnGround.IsInAir)
		{
			OnAirEnter();
		}

		if (m_OnWall.IsOnWall)
		{
			OnWallEnter();
		}
	}

	private void OnGroundEnter()
	{
		m_GameplayTags.GameplayTagContainer.AddTag(m_OnGroundTag);
	}

	private void OnGroundExit()
	{
		m_GameplayTags.GameplayTagContainer.RemoveTag(m_OnGroundTag);
	}

	private void OnSlopeEnter()
	{
		m_GameplayTags.GameplayTagContainer.AddTag(m_OnSlopeTag);
	}

	private void OnSlopeExit()
	{
		m_GameplayTags.GameplayTagContainer.RemoveTag(m_OnSlopeTag);
	}

	private void OnAirEnter()
	{
		m_GameplayTags.GameplayTagContainer.AddTag(m_InAirTag);
	}

	private void OnAirExit()
	{
		m_GameplayTags.GameplayTagContainer.RemoveTag(m_InAirTag);
	}

	private void OnWallEnter()
	{
		m_GameplayTags.GameplayTagContainer.AddTag(m_OnWallTag);
	}

	private void OnWallExit()
	{
		m_GameplayTags.GameplayTagContainer.RemoveTag(m_OnWallTag);
	}
}
