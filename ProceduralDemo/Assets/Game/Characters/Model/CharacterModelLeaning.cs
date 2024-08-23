using System.Collections;
using System.Collections.Generic;
using ODev.Util;
using UnityEngine;

[System.Serializable]
public class CharacterModelLeaning : CharacterModelControllerBase
{
	// [SerializeField]
	// private Transform m_Transform = null;
	[SerializeField, Range(0.0f, 1.0f)]
	private float m_Max01 = 0.5f;
	[SerializeField]
	private float m_MaxAcceleration = 15.0f;
	[SerializeField]
	private float m_Spring = 100.0f;
	[SerializeField]
	private float m_Damper = 10.0f;
	[SerializeField]
	private float m_Scalar = 2.0f;

	private Quaternion m_CurrRotation = Quaternion.identity;
	public Quaternion CalculatedRotation => m_CurrRotation;

	private Vector3 m_Direction = Vector3.zero;
	private Vector2 m_Velocity = Vector2.zero;

	public override void Reset()
	{
		m_CurrRotation = Quaternion.identity;
	}

	public override void Tick(float pDeltaTime)
	{
		GetMovement(out Vector3 direction, out float amount01);
		m_Velocity += amount01 * m_Scalar * new Vector2(direction.x, direction.z);
		m_Direction = new Vector3(
			Func.SpringDamper(m_Direction.x, 0, ref m_Velocity.x, m_Spring, m_Damper, pDeltaTime), 1.0f,
			Func.SpringDamper(m_Direction.z, 0, ref m_Velocity.y, m_Spring, m_Damper, pDeltaTime));
		m_CurrRotation = Quaternion.FromToRotation(Vector3.up, m_Direction.normalized);
	}

	private Vector3 m_LastVelocity = Vector3.zero;
	private void GetMovement(out Vector3 oDirection, out float oAmount01)
	{
		Vector3 velocity = Root.Movement.VelocityXZ.Horizontal();
		oDirection = velocity - m_LastVelocity;
		m_LastVelocity = velocity;

		oAmount01 = Mathf.Clamp01(oDirection.magnitude / m_MaxAcceleration) * m_Max01;
	}

	protected override void OnDrawGizmos()
	{
		if (Root == null)
		{
			return;
		}
	}
}
