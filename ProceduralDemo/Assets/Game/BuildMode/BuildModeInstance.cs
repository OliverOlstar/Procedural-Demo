using System.Collections;
using System.Collections.Generic;
using ODev.Util;
using UnityEngine;
using UnityEngine.Pool;

public class BuildModeInstance : MonoBehaviour
{
	private int m_Id;
	private SOBuildingItem m_Data;
	private BuildModeInstance m_Parent = null;
	private readonly List<BuildModeInstance> m_Children = new();

	public int Id => m_Id;
	public SOBuildingItem Data => m_Data;
	public BuildModeInstance Parent => m_Parent;
	public List<BuildModeInstance> Children => m_Children;

	public void Initalize(int pId, SOBuildingItem pData, Vector3 pPosition, Quaternion pRotation, BuildModeInstance pParent)
	{
		m_Id = pId;
		m_Data = pData;
		transform.SetPositionAndRotation(pPosition, pRotation);
		SetParent(pParent);
	}

	public void SetParent(BuildModeInstance pParent)
	{
		if (m_Parent == pParent)
		{
			return;
		}
		if (m_Parent != null)
		{
			m_Parent.RemoveChild(this);
		}
		m_Parent = pParent;
		if (m_Parent != null)
		{
			m_Parent.AddChild(this);
		}
	}

	private void AddChild(BuildModeInstance pChild)
	{
		if (!m_Children.AddUniqueItem(pChild))
		{
			this.DevException($"{pChild} is already added");
		}
	}

	private void RemoveChild(BuildModeInstance pChild)
	{
		if (!m_Children.Remove(pChild))
		{
			this.DevException($"{pChild} wasn't a child");
		}
	}

	private void AttachToParent()
	{
		transform.SetParent(m_Parent.transform);
		foreach (BuildModeInstance child in m_Children)
		{
			child.AttachToParent();
		}
	}

	private void DeattachFromParent()
	{
		transform.SetParent(null);
		foreach (BuildModeInstance child in m_Children)
		{
			child.DeattachFromParent();
		}
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
		foreach (BuildModeInstance child in m_Children)
		{
			child.AttachToParent();
		}
		transform.SetPositionAndRotation(pPosition, pRotation);
		foreach (BuildModeInstance child in m_Children)
		{
			child.DeattachFromParent();
		}
	}

	public void OpenDetailPanel()
	{
		this.Log();
	}
}
