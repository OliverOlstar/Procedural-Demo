using System;
using System.Collections;
using System.Collections.Generic;
using ODev.Cue;
using ODev.Util;
using UnityEngine;

public class BuildModeController : MonoBehaviour
{
	[SerializeField]
	private BuildModeRoot m_Root = null;
	[SerializeField]
	private ODev.Util.Mono.Updateable m_Updateable = new();

	[Space, SerializeField]
	private float m_RotationSpeed = 0.0f;

	[Header("Cues")]
	[SerializeField]
	private SOCue m_InvalidNoSelect = null;
	[SerializeField]
	private SOCue m_InvalidPlacement = null;

	private SOBuildingItem m_SelectedItem = null;
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
		m_Root.Input.Place.OnPerformed.AddListener(PlaceItem);
	}

	private void OnDestroy()
	{
		m_Root.Input.Exit.OnPerformed.RemoveListener(ExitMode);
		m_Root.Input.Place.OnPerformed.RemoveListener(PlaceItem);
	}
	
	private void ExitMode() => m_Root.Mode.SwitchToPlayer();

	private void PlaceItem()
	{
		if (m_SelectedItem == null)
		{
			SOCue.Play(m_InvalidNoSelect, new CueContext(m_LastResult.Point));
			return;
		}
		if (!m_LastResult.IsValid)
		{
			SOCue.Play(m_InvalidPlacement, new CueContext(m_LastResult.Point));
			return;
		}
		GameObject building = Instantiate(m_SelectedItem.Prefab);
		building.transform.SetPositionAndRotation(m_LastResult.Point, m_LastResult.Rotation);
		int remainingCount = PlayerBuildingInventory.Instance.RemoveItem(m_SelectedItem);
		if (remainingCount < 1)
		{
			SetSelectedItem(null);
		}
	}

	private void Tick(float pDeltaTime)
	{
		if (m_SelectedItem != null)
		{
			if (!m_Root.Input.Rotate.Input.IsNearZero())
			{
				m_Root.Raycaster.Rotate(m_Root.Input.Rotate.Input * m_RotationSpeed * pDeltaTime);
			}
			BuildModeRaycaster.Result result = m_Root.Raycaster.DoRaycast();
			m_Root.Displayer.UpdateVisuals(result, pDeltaTime);
			m_LastResult = result;
		}
	}

	public void SetSelectedItem(SOBuildingItem pItem)
	{
		m_SelectedItem = pItem;
		if (pItem == null)
		{
			m_Root.Displayer.SetModel(null);
			return;
		}
		m_Root.Raycaster.SetBuilding(pItem);
		m_Root.Displayer.SetModel(pItem.ModelPrefab);
	}
	public void ClearSelectedItem() => SetSelectedItem(null);
}
