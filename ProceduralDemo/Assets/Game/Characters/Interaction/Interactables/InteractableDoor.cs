using System.Collections;
using System.Collections.Generic;
using ODev.Util;
using UnityEngine;

public class InteractableDoor : InteractableBase
{
	[SerializeField]
	private Transform m_Transform = null;
	[SerializeField]
	private float m_UpDistance = 1.0f;

	private bool m_Up = false;

	protected override void OnSelectEnter()
	{
		// m_Transform.position += Vector3.up * m_UpDistance;
	}

	protected override void OnSelectExit()
	{
		// m_Transform.position -= Vector3.up * m_UpDistance;
	}

	public override void Interact(PlayerRoot _)
	{
		this.Log(m_Up.ToString());
		m_Up = !m_Up;
		if (m_Up)
		{
			m_Transform.position += Vector3.up * m_UpDistance;
		}
		else
		{
			m_Transform.position -= Vector3.up * m_UpDistance;
		}
	}
}
