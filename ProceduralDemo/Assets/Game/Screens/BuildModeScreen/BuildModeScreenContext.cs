using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BuildModeScreenContext
{
    private readonly Action<SOBuildingItem> m_OnSelectBuilding;
	public readonly Action<SOBuildingItem> SelectBuilding => m_OnSelectBuilding;

	public BuildModeScreenContext(Action<SOBuildingItem> pOnSelectBuilding)
	{
		m_OnSelectBuilding = pOnSelectBuilding;
	}
}
