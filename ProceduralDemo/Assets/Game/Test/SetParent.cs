using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetParent : MonoBehaviour
{
	[SerializeField]
	private Transform m_Parent = null;
	[SerializeField]
	private Vector3 m_Offset = Vector3.up;

    private void Awake()
    {
        transform.SetParent(m_Parent);
		transform.localPosition = m_Offset;
		Destroy(this);
    }
}
