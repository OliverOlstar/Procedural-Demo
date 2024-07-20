using System.Collections;
using ODev.Util;
using UnityEngine;

public abstract class InteractableBase : MonoBehaviour
{
	private const float HOVER_EXIT_DELAY_SECONDS = 0.5f;

	private int m_HoverCount = 0;
	private Coroutine m_HoverExitCoroutine = null;

	public virtual Vector3 Position => transform.position;

	public abstract void Interact();
	protected abstract void OnHoverEnter();
	protected abstract void OnHoverExit();

	public virtual bool CanInteract() => true;
	public virtual bool CanHover() => true;


	public void HoverEnter()
	{
		m_HoverCount++;
		// this.Log(m_HoverCount.ToString());
		if (m_HoverCount == 1)
		{
			StopHoverExitCoroutine();
			OnHoverEnter();
		}
	}

	public void HoverExit()
	{
		m_HoverCount--;
		// this.Log(m_HoverCount.ToString());
		if (m_HoverCount == 0)
		{
			StopHoverExitCoroutine();
			m_HoverExitCoroutine = StartCoroutine(HoverExitDelayed());
		}
	}

	private IEnumerator HoverExitDelayed()
	{
		yield return new WaitForSeconds(HOVER_EXIT_DELAY_SECONDS);
		if (m_HoverCount == 0)
		{
			OnHoverExit();
		}
	}

	private void StopHoverExitCoroutine()
	{
		if (m_HoverExitCoroutine != null)
		{
			StopCoroutine(m_HoverExitCoroutine);
			m_HoverExitCoroutine = null;
		}
	}
}
