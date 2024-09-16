using System.Collections;
using System.Collections.Generic;
using ODev.Util;
using UnityEngine;

public class InteractableBuildMode : InteractableBase
{
	[SerializeField]
	private Transform m_CameraTarget = null;

	public override void Interact(PlayerRoot pPlayer)
	{
		pPlayer.Mode.SwitchToBuild(m_CameraTarget.position);
	}
}
