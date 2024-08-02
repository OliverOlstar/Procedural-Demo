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
	private float m_Dampening = 2.0f;

	private Quaternion m_LastRotation = Quaternion.identity;
	public Quaternion CalculatedRotation => m_LastRotation;

	public override void Reset()
	{
		m_LastRotation = Quaternion.identity;
	}

	public override void Tick(float pDeltaTime)
	{
		GetMovement(out Vector3 direction, out float amount01);
		Quaternion rotationToTarget = Quaternion.FromToRotation(Vector3.up, direction);
		rotationToTarget = Quaternion.Lerp(Quaternion.identity, rotationToTarget, amount01);
		m_LastRotation = Quaternion.Lerp(m_LastRotation, rotationToTarget, pDeltaTime * m_Dampening);
		// Quaternion targetRotation = Quaternion.identity * rotationToTarget;
		// m_Transform.localRotation = Quaternion.Lerp(m_Transform.localRotation, targetRotation, pDeltaTime * m_Dampening);
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
