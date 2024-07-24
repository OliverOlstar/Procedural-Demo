using System;
using System.Collections;
using System.Collections.Generic;
using ODev.Util;
using UnityEngine;

public class BuildModeRoot : MonoBehaviour, PlayerModeController.IMode
{
	[SerializeField]
	private PlayerModeController m_Mode = null;
	[SerializeField]
	private InputBridge_BuildMode m_Input = null;
	[SerializeField]
	private BuildModeController m_Controller = null;
	[SerializeField]
	private BuildModeRaycaster m_Raycaster = null;
	[SerializeField]
	private BuildModeDisplayer m_Displayer = null;

	public PlayerModeController Mode => m_Mode;
	public InputBridge_BuildMode Input => m_Input;
	public BuildModeController Contoller => m_Controller;
	public BuildModeRaycaster Raycaster => m_Raycaster;
	public BuildModeDisplayer Displayer => m_Displayer;

	void PlayerModeController.IMode.DisableMode()
	{
		gameObject.SetActive(false);
		if (ScreenManager.TryGet(out BuildModeScreen screen))
		{
			screen.Close();
		}
	}

	void PlayerModeController.IMode.EnableMode()
	{
		gameObject.SetActive(true);
		if (ScreenManager.TryGet(out BuildModeScreen screen))
		{
			screen.Open(new BuildModeScreenContext(m_Controller.SetSelectedItem));
		}
	}
}
