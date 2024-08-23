using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class UnityEventOnTrigger : MonoBehaviour
{
	[SerializeField]
	private LayerMask m_AllowedLayers = new();
	[SerializeField]
	private string[] m_AllowedTags = new string[0];

	[Header("Events")]
	public UnityEvent OnTriggerEnterEvent;

	private void OnTriggerEnter(Collider other)
	{
		if (IsValid(other))
		{
			OnTriggerEnterEvent.Invoke();
		}
	}

	private bool IsValid(Collider other)
	{
		// Other is trigger
		if (other.isTrigger)
		{
			return false;
		}

		// Layers
		if ((m_AllowedLayers | (1 << other.gameObject.layer)) != 0)
		{
			return true;
		}

		// Tags
		foreach (string tag in m_AllowedTags)
		{
			if (other.CompareTag(tag))
			{
				return true;
			}
		}

		// Default
		return false;
	}
}
