using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ODev.Util;
using UnityEngine;

public abstract class InteractableBase : MonoBehaviour
{
	[SerializeField]
	private bool m_LogInteractable = false;

	private int m_SelectCount = 0;

	public virtual Vector3 Position => transform.position;

	public abstract void Interact(PlayerRoot pPlayer);
	protected virtual void OnSelectEnter() { }
	protected virtual void OnSelectExit() { }
	protected virtual void OnHoverEnter() { }
	protected virtual void OnHoverExit() { }

	public virtual bool CanInteract() => true;
	public virtual bool CanSelect() => true;
	public virtual bool CanHover() => true;

	private readonly List<CharacterInteractor> m_Interactors = new();

	public void HoverEnter(CharacterInteractor pInteractor)
	{
		if (!m_Interactors.AddUniqueItem(pInteractor))
		{
			this.DevException($"Interactor {pInteractor.name} started hovering us more than once");
			return;
		}
		Log(m_Interactors.Count.ToString());
		if (m_Interactors.Count == 1)
		{
			OnHoverEnter();
		}
	}

	public void HoverExit(CharacterInteractor pInteractor)
	{
		if (!m_Interactors.Remove(pInteractor))
		{
			this.DevException($"Interactor {pInteractor.name} tried to stop hovering us but it wasn't hovering us.");
			return;
		}
		Log(m_Interactors.Count.ToString());
		if (m_Interactors.Count == 0)
		{
			OnHoverExit();
		}
	}

	public void SelectEnter()
	{
		m_SelectCount++;
		Log(m_SelectCount.ToString());
		if (m_SelectCount == 1)
		{
			OnSelectEnter();
		}
	}

	public void SelectExit()
	{
		m_SelectCount--;
		Log(m_SelectCount.ToString());
		if (m_SelectCount == 0)
		{
			OnSelectExit();
		}
	}

	protected virtual void OnDestroy()
	{
		while (m_Interactors.Count > 0)
		{
			m_Interactors[0].OnInteractableDestroy(this);
		}
		m_Interactors.Clear();
	}

	[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
	private void Log(string pMessage, [CallerMemberName] string pMethodName = "")
	{
		if (m_LogInteractable)
		{
			ODev.Util.Debug.Log(this, pMessage, pMethodName);
		}
	}
}
