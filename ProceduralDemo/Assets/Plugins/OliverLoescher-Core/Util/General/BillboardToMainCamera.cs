using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardToMainCamera : MonoBehaviour
{
	private Transform myCamera = null;

	void Start()
	{
		myCamera = Camera.main.transform;
	}

	void LateUpdate()
	{
		Vector3 dir = transform.position - myCamera.transform.position;
		dir.y = 0;

		if (dir != Vector3.zero)
			transform.rotation = Quaternion.LookRotation(dir);
	}
}
