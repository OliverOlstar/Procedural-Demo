using System.Collections;
using System.Collections.Generic;
using ODev.Util;
using UnityEngine;

public class BuildModeInstance : MonoBehaviour
{
	private int m_Id;
	private SOBuildingItem m_Data;

	public int Id => m_Id;
	public SOBuildingItem Data => m_Data;

	public void Initalize(int pId, SOBuildingItem pData, Vector3 pPosition, Quaternion pRotation)
	{
		m_Id = pId;
		m_Data = pData;
		transform.SetPositionAndRotation(pPosition, pRotation);
	}

	public void OnHoverEnter()
	{
		this.Log();
	}

	public void OnHoverExit()
	{
		this.Log();
	}

	public void OnMovingEnter()
	{
		gameObject.SetActive(false);
	}

	public void OnMovingExit()
	{
		gameObject.SetActive(true);
	}

	public void Move(Vector3 pPosition, Quaternion pRotation)
	{
		transform.SetPositionAndRotation(pPosition, pRotation);
	}

	public void OpenDetailPanel()
	{
		this.Log();
	}
}
