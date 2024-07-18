using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableBase : MonoBehaviour
{
	private int m_HoverCount = 0;

	public abstract void Interact();
	protected abstract void OnHoverEnter();
	protected abstract void OnHoverExit();

	public virtual bool CanInteract() => true;
	public virtual bool CanHover() => true;

	public void HoverEnter()
	{
		m_HoverCount++;
		if (m_HoverCount == 1)
		{
			OnHoverEnter();
		}
	}

	public void HoverExit()
	{
		m_HoverCount--;
		if (m_HoverCount == 0)
		{
			OnHoverExit();
		}
	}
}
