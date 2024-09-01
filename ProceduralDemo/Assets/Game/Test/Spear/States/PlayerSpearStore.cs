using UnityEngine;

[System.Serializable]
public class PlayerSpearStore : PlayerSpearController
{
	public override PlayerSpear.State State => PlayerSpear.State.Stored;

	private Vector3 m_ScaleDefault;

	public void Start()
	{
		m_ScaleDefault = Transform.localScale;
		Transform.localScale = Vector3.one * 0.001f;
	}

	public override void Stop()
	{
		Transform.localScale = m_ScaleDefault;
	}
}
