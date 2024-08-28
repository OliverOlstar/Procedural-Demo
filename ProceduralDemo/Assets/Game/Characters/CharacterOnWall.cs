using System.Collections;
using System.Collections.Generic;
using ODev;
using ODev.Util;
using UnityEngine;
using UnityEngine.Events;

public class CharacterOnWall : MonoBehaviour
{
	[SerializeField]
	private PlayerRoot m_Root = null;
	[SerializeField]
	private Mono.Updateable m_Updateable = new(Mono.Type.Fixed, Mono.Priorities.OnGround);

	[Header("Cast")]
	[SerializeField]
	private float m_Distance = 1.0f;
	[SerializeField]
	private LayerMask m_WallLayer = new();
	[SerializeField, Range(0, 90.0f)]
	private float m_MaxSlope = 45.0f;

	[Header("Capsule")]
	[SerializeField]
	private float m_Offset = 1.0f;
	[SerializeField]
	private float m_Radius = 0.5f;
	[SerializeField]
	private float m_Height = 2.0f;

	[Header("Events")]
	public UnityEventsUtil.BoolEvent OnWallChanged = new();
	public UnityEvent OnWallEnter = new();
	public UnityEvent OnWallExit = new();

	private RaycastHit m_HitInfo = new();
	private bool m_IsOnWall = false;

	private Transform Transform => m_Root.Movement.transform;
	private Vector3 VelocityXZ => m_Root.Movement.VelocityXZ;

	public RaycastHit HitInfo => m_HitInfo;
	public bool IsOnWall => m_IsOnWall;

	private void OnEnable()
	{
		m_Updateable.Register(Tick);
	}

	private void OnDisable()
	{
		m_Updateable.Deregister();
	}

	private void Tick(float pDeltaTime)
	{
		if (VelocityXZ.IsNearZero())
		{
			return;
		}
		bool isOnWall = Check() && IsValid();
		if (m_IsOnWall != isOnWall)
		{
			m_IsOnWall = isOnWall;
			if (isOnWall)
			{
				OnWallEnter.Invoke();
			}
			else
			{
				OnWallExit.Invoke();
			}
			OnWallChanged.Invoke(isOnWall);
		}
	}

	public bool Check()
	{
		Vector3 offset = new(0.0f, (m_Height * 0.5f) - m_Radius, 0.0f);
		Vector3 originOffset = new(0.0f, m_Offset, 0.0f);
		Vector3 pointA = Transform.position + offset + originOffset;
		Vector3 pointB = Transform.position - offset + originOffset;
		Vector3 direction = VelocityXZ.Horizontal();
		return Physics.CapsuleCast(pointA, pointB, m_Radius, direction, out m_HitInfo, m_Distance, m_WallLayer);
	}

	private bool IsValid()
	{
		float angle = Mathf.Abs(m_HitInfo.normal.y) * 90.0f;
		return angle < m_MaxSlope;
	}

	public void OnDrawGizmos()
	{
		if (VelocityXZ.IsNearZero())
		{
			return;
		}

		Vector3 offset = new(0.0f, (m_Height * 0.5f) - m_Radius, 0.0f);
		Vector3 originOffset = new(0.0f, m_Offset, 0.0f);
		Vector3 pointA = Transform.position + offset + originOffset;
		Vector3 pointB = Transform.position - offset + originOffset;
		Vector3 direction = VelocityXZ.Horizontal();
		Vector3 endOffset = direction.normalized * m_Distance;

		// ODev.Util.Debug.GizmoCapsule(pointA, pointB, m_Radius);
		Gizmos.color = Check() ? Color.red : Color.green;
		ODev.Util.Debug.GizmoCapsule(pointA + endOffset, pointB + endOffset, m_Radius);
		Gizmos.DrawLine(pointA, pointA + endOffset);
		Gizmos.DrawLine(pointB, pointB + endOffset);
	}
}
