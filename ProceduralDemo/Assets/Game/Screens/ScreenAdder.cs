using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenAdder : MonoBehaviour
{
	[SerializeField]
	private ScreenBase[] m_Screens = new ScreenBase[0];

	private void Start()
	{
		foreach (ScreenBase screen in m_Screens)
		{
			screen.ForceClose();
			ScreenManager.RegisterScreen(screen);
		}
		Destroy(this);
	}
}
