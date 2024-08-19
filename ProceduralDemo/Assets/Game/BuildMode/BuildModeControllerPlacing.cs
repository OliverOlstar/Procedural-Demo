using System;
using System.Collections;
using System.Collections.Generic;
using ODev.Cue;
using ODev.Util;
using UnityEngine;

[System.Serializable]
public class BuildModeControllerPlacing : BuildModeController.StateBase
{
	[SerializeField]
	private float m_RotationSpeed = 0.0f;

	[Header("Cues")]
	[SerializeField, ODev.Picker.Asset]
	private SOCue m_InvalidPlacement = null;
	[SerializeField, ODev.Picker.Asset]
	private SOCue m_PlacedBuilding = null;

	private BuildModeRaycaster.Result m_LastResult;

	private SOBuildingItem m_SelectedItem = null;

	internal void Enable(SOBuildingItem pItem)
	{
		if (pItem == null)
		{
			Root.Contoller.DevException(new NullReferenceException("pItem cannot be null"));
			return;
		}
		m_SelectedItem = pItem;
		Root.Raycaster.SetConstraints(m_SelectedItem);
		Root.Displayer.SetModel(m_SelectedItem.ModelPrefab);
	}

	internal override void Disable()
	{
		Root.Displayer.SetModel(null);
	}

	internal override void Tick(float pDeltaTime)
	{
		if (!Root.Input.Rotate.Input.IsNearZero())
		{
			Root.Raycaster.Rotate(Root.Input.Rotate.Input * m_RotationSpeed * pDeltaTime);
		}
		BuildModeRaycaster.Result result = Root.Raycaster.DoRaycast();
		Root.Displayer.UpdateVisuals(result, pDeltaTime);
		m_LastResult = result;
	}

	internal override void OnPositiveInput()
	{
		if (!m_LastResult.IsValid)
		{
			SOCue.Play(m_InvalidPlacement, new CueContext(m_LastResult.Point));
			return;
		}
		
		if (!BuildModeManager.PlaceNewItem(m_SelectedItem, m_LastResult.Point, m_LastResult.Rotation, m_LastResult.Other))
		{
			Root.Contoller.SwitchToStateSelecting();
		}
		SOCue.Play(m_PlacedBuilding, new CueContext(m_LastResult.Point));
	}

	internal override void OnNegativeInput()
	{
		Root.Contoller.SwitchToStateSelecting();
	}
}
