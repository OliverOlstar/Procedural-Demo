using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildModeControllerSelecting : BuildModeController.StateBase
{
	private readonly BuildModeRaycasterConstaints m_SelectingConstraints = new(pCanPlaceOnTerrain: false);

	private BuildModeInstance m_SelectedBuilding = null;

	internal void Enable()
	{
		Root.Raycaster.SetConstraints(m_SelectingConstraints);
	}

	internal override void Disable()
	{
		SetSelectedBuilding(null);
	}

	internal override void Tick(float pDeltaTime)
	{
		BuildModeRaycaster.Result result = Root.Raycaster.DoRaycast();
		SetSelectedBuilding(result.Other);
	}

	internal override void OnPositiveInput()
	{
		if (m_SelectedBuilding != null)
		{
			Root.Contoller.SwitchToStateMoving(m_SelectedBuilding);
		}
	}

	internal override void OnNegativeInput()
	{
		if (m_SelectedBuilding != null)
		{
			BuildModeManager.RemoveItem(m_SelectedBuilding.Id);
			m_SelectedBuilding = null;
			// m_SelectedBuilding.OpenDetailPanel();
		}
	}

	public void SetSelectedBuilding(BuildModeInstance pBuilding)
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
}
