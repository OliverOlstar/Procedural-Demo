using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
	[SerializeField, Min(0)]
	private float m_Seconds = 1.0f;

	void Start()
	{
		Destroy(gameObject, m_Seconds);
	}
}
