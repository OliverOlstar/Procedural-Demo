using UnityEngine;
using UnityEngine.Events;

public class UnityEventWithButton : MonoBehaviour
{
	[SerializeField]
	private UnityEvent m_MyEvent = null;

	public void InvokeEvent()
	{
		m_MyEvent.Invoke();
	}
}
