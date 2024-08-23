using System.Collections;
using System.Collections.Generic;
using ODev.Cue;
using ODev.Util;
using UnityEngine;

[System.Serializable]
public class BuildModeControllerMoving : BuildModeController.StateBase
{
	[SerializeField]
	private float m_RotationSpeed = 0.0f;

	[Header("Cues")]
	[SerializeField, ODev.Picker.Asset]
	private SOCue m_InvalidPlacement = null;
	[SerializeField, ODev.Picker.Asset]
	private SOCue m_PlacedBuilding = null;

	private BuildModeRaycaster.Result m_LastResult;

	private BuildModeInstance m_SelectetBuilding = null;

	internal void Enable(BuildModeInstance pBuilding)
	{
		if (pBuilding == null)
		{
			Root.Contoller.DevException(new System.NullReferenceException("pBuilding cannot be null"));
			return;
		}
		m_SelectetBuilding = pBuilding;
		m_SelectetBuilding.OnMovingEnter();
		Root.Raycaster.SetConstraints(m_SelectetBuilding.Data); // TODO Set rotation to what the building was rotated at
		Root.Displayer.SetModel(m_SelectetBuilding.Data.ModelPrefab);
	}

	internal override void Disable()
	{
		m_SelectetBuilding.OnMovingExit();
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

		BuildModeManager.MoveItem(m_SelectetBuilding.Id, m_LastResult.Point, m_LastResult.Rotation, m_LastResult.Other);
		Root.Contoller.SwitchToStateSelecting();
		SOCue.Play(m_PlacedBuilding, new CueContext(m_LastResult.Point));
	}

	internal override void OnNegativeInput()
	{
		Root.Contoller.SwitchToStateSelecting();
	}
}
