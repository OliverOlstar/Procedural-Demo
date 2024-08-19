using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : MonoBehaviour
{
	[SerializeField]
	private bool m_UseTargetFrameRate = false;
	[SerializeField]
	private int m_TargetFrameRate = 30;

	void Start()
	{
		if (m_UseTargetFrameRate)
		{
			Application.targetFrameRate = m_TargetFrameRate;
		}

		Destroy(gameObject);
	}
}
