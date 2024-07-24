using System.Collections;
using System.Collections.Generic;
using ODev.Util;
using UnityEngine;

public class InteractableBuildMode : InteractableBase
{
	public override void Interact(PlayerRoot pPlayer)
	{
		pPlayer.Mode.SwitchToBuild();
	}
}
