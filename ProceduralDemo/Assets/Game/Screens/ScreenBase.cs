using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScreenBase : MonoBehaviour
{
	private struct NullContext { }
	private readonly NullContext NULL_CONTEXT = new();

	private bool m_IsOpen = true;

	public bool IsOpen => m_IsOpen;

	protected virtual void OnOpen() { }
	protected virtual void OnOpenWithData<TContext>(in TContext pContext) where TContext : struct { }
	protected virtual void OnClose() { }

	public void Open<TContext>(in TContext pContext) where TContext : struct
	{
		if (m_IsOpen)
		{
			return;
		}
		m_IsOpen = true;

		OnOpen();
		if (pContext is not NullContext)
		{
			OnOpenWithData(pContext);
		}
		gameObject.SetActive(true);
	}
	public void Open() => Open(NULL_CONTEXT); // No Data

	public void Close()
	{
		if (!m_IsOpen)
		{
			return;
		}
		ForceClose();
	}

	public void ForceClose()
	{
		m_IsOpen = false;
		OnClose();
		gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		ScreenManager.DeregisterScreen(this);
	}
}
