using System.Collections;
using System.Collections.Generic;
using ODev.Util;
using UnityEngine;

public class BuildModeInstance : MonoBehaviour
{
	private int m_Id;
	private int Id => m_Id;

    public void Initalize(int pId, Vector3 pPosition, Quaternion pRotation)
	{
		m_Id = pId;
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

	public void Pickup()
	{
		this.Log();
	}

	public void OpenDetailPanel()
	{
		this.Log();
	}
}
