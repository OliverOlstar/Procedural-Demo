using System;
using ODev.Cue;
using ODev.Util;
using UnityEngine;

public class BuildModeController : MonoBehaviour
{
	private readonly BuildModeRaycasterConstaints m_SelectingConstraints = new(pCanPlaceOnTerrain: false);

	[SerializeField]
	private BuildModeRoot m_Root = null;
	[SerializeField]
	private ODev.Util.Mono.Updateable m_Updateable = new();

	[Space, SerializeField]
	private float m_RotationSpeed = 0.0f;

	[Header("Cues")]
	[SerializeField, ODev.Picker.Asset]
	private SOCue m_InvalidNoSelect = null;
	[SerializeField, ODev.Picker.Asset]
	private SOCue m_InvalidPlacement = null;
	[SerializeField, ODev.Picker.Asset]
	private SOCue m_PlacedBuilding = null;

	private SOBuildingItem m_SelectedItem = null;
	private BuildModeInstance m_SelectedBuilding = null;
	private BuildModeRaycaster.Result m_LastResult;

	private void OnEnable()
	{
		m_Updateable.Register(Tick);
	}
	private void OnDisable()
	{
		m_Updateable.Deregister();
	}

	private void Start()
	{
		m_Root.Input.Exit.OnPerformed.AddListener(ExitMode);
		m_Root.Input.Positive.OnPerformed.AddListener(OnPositiveInput);
		m_Root.Input.Negative.OnPerformed.AddListener(OnNegativeInput);
	}
	private void OnDestroy()
	{
		m_Root.Input.Exit.OnPerformed.RemoveListener(ExitMode);
		m_Root.Input.Positive.OnPerformed.RemoveListener(OnPositiveInput);
		m_Root.Input.Negative.OnPerformed.RemoveListener(OnNegativeInput);
	}

	private void Tick(float pDeltaTime)
	{
		if (m_SelectedItem == null)
		{
			SelectingTick(pDeltaTime);
		}
		else
		{
			PlacingTick(pDeltaTime);
		}
	}

	private void SelectingTick(float _)
	{
		BuildModeRaycaster.Result result = m_Root.Raycaster.DoRaycast();
		SetSelectedBuilding(result.Other);
	}

	private void SetSelectedBuilding(BuildModeInstance pBuilding)
	{
		if (pBuilding == m_SelectedBuilding)
		{
			return;
		}

		if (m_SelectedBuilding != null)
		{
			m_SelectedBuilding.OnHoverExit();
		}
		m_SelectedBuilding = pBuilding;
		if (m_SelectedBuilding != null)
		{
			m_SelectedBuilding.OnHoverEnter();
		}
	}

	private void PlacingTick(float pDeltaTime)
	{
		if (!m_Root.Input.Rotate.Input.IsNearZero())
		{
			m_Root.Raycaster.Rotate(m_Root.Input.Rotate.Input * m_RotationSpeed * pDeltaTime);
		}
		BuildModeRaycaster.Result result = m_Root.Raycaster.DoRaycast();
		m_Root.Displayer.UpdateVisuals(result, pDeltaTime);
		m_LastResult = result;
	}

	private void ExitMode() => m_Root.Mode.SwitchToPlayer();

	private void OnPositiveInput()
	{
		if (m_SelectedBuilding != null)
		{
			m_SelectedBuilding.Pickup();
			return;
		}
		else if (m_SelectedItem != null)
		{
			if (!m_LastResult.IsValid)
			{
				SOCue.Play(m_InvalidPlacement, new CueContext(m_LastResult.Point));
				return;
			}
			BuildModeManager.PlaceNewItem(m_SelectedItem, m_LastResult.Point, m_LastResult.Rotation);
			int remainingCount = PlayerBuildingInventory.Instance.RemoveItem(m_SelectedItem);
			if (remainingCount < 1)
			{
				SetSelectedItem(null);
			}
			SOCue.Play(m_PlacedBuilding, new CueContext(m_LastResult.Point));
			return;
		}
		SOCue.Play(m_InvalidNoSelect, new CueContext(m_LastResult.Point));
	}

	public void SetSelectedItem(SOBuildingItem pItem)
	{
		m_SelectedItem = pItem;
		if (pItem == null)
		{
			m_Root.Raycaster.SetConstraints(m_SelectingConstraints);
			m_Root.Displayer.SetModel(null);
			return;
		}
		m_Root.Raycaster.SetConstraints(pItem);
		m_Root.Displayer.SetModel(pItem.ModelPrefab);
		SetSelectedBuilding(null);
	}
	public void OnNegativeInput()
	{
		if (m_SelectedBuilding != null)
		{
			m_SelectedBuilding.OpenDetailPanel();
			return;
		}
		SetSelectedItem(null);
	}
}
