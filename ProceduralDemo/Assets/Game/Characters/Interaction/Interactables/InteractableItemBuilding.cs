using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableItemBuilding : InteractableItem
{
	[SerializeField, ODev.Picker.AssetNonNull]
	private SOBuildingItem m_Item = null;

	public override void Interact(PlayerRoot pPlayer)
	{
		pPlayer.Buildings.AddItem(m_Item);
		base.Interact(pPlayer);
	}
}
