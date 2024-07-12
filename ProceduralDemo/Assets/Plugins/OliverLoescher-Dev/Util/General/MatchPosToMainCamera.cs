using UnityEngine;

public class MatchPosToMainCamera : MonoBehaviour
{
	private Transform m_MainCamera;
	[SerializeField]
	private Vector3 m_Offset = new();

	void Start()
	{
		m_MainCamera = Camera.main.transform;
	}

	void FixedUpdate()
	{
		transform.position = m_MainCamera.position + m_Offset;
	}
}
