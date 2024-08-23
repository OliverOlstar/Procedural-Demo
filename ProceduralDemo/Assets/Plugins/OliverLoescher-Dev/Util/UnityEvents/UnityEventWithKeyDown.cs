using UnityEngine;

[ExecuteInEditMode]
public class UnityEventWithKeyDown : UnityEventWithButton
{
	[SerializeField]
	private KeyCode m_KeyDown = KeyCode.None;
	[SerializeField]
	private bool m_ExecuteInEditMode = false;

	private void Update()
	{
		if (Input.GetKeyDown(m_KeyDown) && (m_ExecuteInEditMode || Application.isPlaying))
		{
			InvokeEvent();
		}
	}
}
