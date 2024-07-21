using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildModeRoot : MonoBehaviour, PlayerModeController.IMode
{
	void PlayerModeController.IMode.DisableMode()
	{
		gameObject.SetActive(false);
	}

	void PlayerModeController.IMode.EnableMode()
	{
		gameObject.SetActive(true);
	}
}
