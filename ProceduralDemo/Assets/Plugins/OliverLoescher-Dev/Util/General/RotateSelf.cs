using UnityEngine;

public class RotateSelf : MonoBehaviour
{
	[SerializeField]
	private Vector3 m_RotateSpeed = new(0.0f, 1.0f, 0.0f);

	private void LateUpdate()
	{
		transform.Rotate(m_RotateSpeed * Time.deltaTime, Space.Self);
	}
}
