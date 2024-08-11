using System.Collections;
using System.Collections.Generic;
using ODev;
using ODev.Util;
using UnityEngine;
using UnityEngine.Events;

public class CardinalWheel : UpdateableMonoBehaviour
{
	public UnityEvent<float> OnAngleChanged = new();

	[SerializeField]
	private Transform m_Target = null;
	[SerializeField]
	private float m_Radius = 1.0f;
	[SerializeField]
	private float m_AnglePerMeter = 10.0f;

	private float m_Angle = 0.0f;
	private Vector3 m_LastPosition = Vector3.zero;

	private void Start()
	{
		m_LastPosition = transform.position;
	}

	protected override void Tick(float pDeltaTime)
	{
		Vector3 deltaPosition = m_Target.position - m_LastPosition;
		m_LastPosition = m_Target.position;
		deltaPosition = deltaPosition.ProjectOnPlane(m_Target.right);
		deltaPosition.y = 0.0f;
		Vector3 forward = m_Target.forward.Horizontalize();
		float negative = Vector3.Dot(forward, deltaPosition) > 0 ? 1.0f : -1.0f;

		float newAngle = m_Angle;
		newAngle += deltaPosition.magnitude * negative * m_AnglePerMeter / m_Radius;
		newAngle = newAngle.Loop(360.0f);
		
		if (newAngle != m_Angle)
		{
			m_Angle = newAngle;
			OnAngleChanged.Invoke(m_Angle);
		}
	}

	private void Reset()
	{
		m_Target = transform;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		Vector3 forward = m_Target.forward.Horizontalize();
		for (int i = 0; i < 4; i++)
		{
			Vector3 direction = Quaternion.AngleAxis((i * 90.0f) + m_Angle, m_Target.right) * forward;
			Vector3 pointA = m_Target.position + (direction * m_Radius);
			Vector3 pointB = pointA + (direction * 0.5f);
			Gizmos.DrawLine(pointA, pointB);
		}
		for (int i = 0; i < 4; i++)
		{
			Vector3 direction = Quaternion.AngleAxis((i * 90.0f) + 45.0f + m_Angle, m_Target.right) * forward;
			Vector3 pointA = m_Target.position + (direction * m_Radius);
			Vector3 pointB = pointA + (direction * 0.25f);
			Gizmos.DrawLine(pointA, pointB);
		}
	}
}
