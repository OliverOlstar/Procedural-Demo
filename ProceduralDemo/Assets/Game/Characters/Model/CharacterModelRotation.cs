using ODev.Util;
using UnityEngine;

[System.Serializable]
public class CharacterModelRotation : CharacterModelControllerBase
{
	[SerializeField]
	private float m_Dampening = 8.0f;

	private Quaternion m_LastRotation = Quaternion.identity;
	public Quaternion CalculatedRotation => m_LastRotation;

	public override void Reset()
	{
		m_LastRotation = Quaternion.identity;
	}

	public override void Tick(float pDeltaTime)
	{
		Vector3 direction = Root.Movement.VelocityXZ.Horizontal();
		if (direction.IsNearZero())
		{
			return;
		}
		Quaternion targetRotation = Quaternion.LookRotation(direction);
		m_LastRotation = Quaternion.Lerp(m_LastRotation, targetRotation, pDeltaTime * m_Dampening);
	}
}