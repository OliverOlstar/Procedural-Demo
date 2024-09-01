using UnityEngine;

[System.Serializable]
public class PlayerSpearStore : PlayerSpearController
{
	internal override PlayerSpear.State State => PlayerSpear.State.Stored;

	private Vector3 m_ScaleDefault;

	internal void Start()
	{
		m_ScaleDefault = Transform.localScale;
		Transform.localScale = Vector3.one * 0.001f;
	}

	internal override void Stop()
	{
		Transform.localScale = m_ScaleDefault;
	}
}
