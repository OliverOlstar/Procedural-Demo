using System.Collections;
using System.Collections.Generic;
using ODev.Util;
using UnityEngine;

public class PlayerModeController : MonoBehaviour
{
	public interface IMode
	{
		void EnableMode();
		void DisableMode();
	}

	[SerializeField]
	private PlayerRoot m_PlayerMode = null;
	[SerializeField]
	private BuildModeRoot m_BuildMode = null;

	private IMode m_CurrMode;

	private void Awake()
	{
		(m_BuildMode as IMode).DisableMode();
		(m_PlayerMode as IMode).EnableMode();
		m_CurrMode = m_PlayerMode;
	}

	private void SwitchMode(IMode pToMode)
	{
		if (pToMode == null)
		{
			this.DevException("Mode can never be null");
			return;
		}
		if (m_CurrMode == pToMode)
		{
			return;
		}
		m_CurrMode.DisableMode();
		m_CurrMode = pToMode;
		m_CurrMode.EnableMode();
	}
	public void SwitchToBuild(Vector3 pCameraPosition)
	{
		m_BuildMode.SetupMode(pCameraPosition);
		SwitchMode(m_BuildMode);
	}

	public void SwitchToPlayer() => SwitchMode(m_PlayerMode);
}
