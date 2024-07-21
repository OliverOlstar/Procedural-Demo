using System.Collections;
using System.Collections.Generic;
using ODev.Util;
using UnityEngine;

public class InteractableItem : InteractableBase
{
	[SerializeField]
	private Renderer m_Renderer = null;

	public override void Interact()
	{
		Destroy(gameObject);
	}

	protected override void OnSelectEnter()
	{
		m_Renderer.material.SetColor("_BaseColor", Color.green);
	}

	protected override void OnSelectExit()
	{
		m_Renderer.material.SetColor("_BaseColor", Color.grey);
	}
}
