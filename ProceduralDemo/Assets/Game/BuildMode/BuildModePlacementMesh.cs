using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using ODev.Util;

public class BuildModePlacementMesh : MonoBehaviour
{
	[SerializeField]
	private Material m_Material = null;

	private int m_OverlapCount = 0;

	public bool IsOverlapping => m_OverlapCount > 0;

	private void OnTriggerEnter(Collider pOther)
	{
		m_OverlapCount++;
	}

	private void OnTriggerExit(Collider pOther)
	{
		m_OverlapCount--;
	}

	[Button]
	private void ConfigurePrefab()
	{
		foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
		{
			renderer.material = m_Material;
		}
		foreach (Collider collider in transform.GetComponentsInChildren<Collider>())
		{
			collider.isTrigger = true;
		}
		foreach (Collider collider in transform.GetComponentsInChildren<Collider>())
		{
			collider.isTrigger = true;
		}
		foreach (Transform transform in transform.GetComponentsInChildren<Transform>())
		{
			transform.gameObject.layer = 27;
		}
		Rigidbody rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
		rigidbody.useGravity = false;
		rigidbody.isKinematic = true;
	}
}
