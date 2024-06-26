using UnityEngine;

public class BillboardToMainCamera : MonoBehaviour
{
	private Transform m_MyCamera = null;

	void Start()
	{
		m_MyCamera = Camera.main.transform;
	}

	void LateUpdate()
	{
		Vector3 dir = transform.position - m_MyCamera.transform.position;
		dir.y = 0;

		if (dir != Vector3.zero)
		{
			transform.rotation = Quaternion.LookRotation(dir);
		}
	}
}
