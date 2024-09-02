using ODev.Util;
using UnityEngine;

[System.Serializable]
public class PlayerSpearThrow : PlayerSpearController
{
	[SerializeField]
	private float m_Speed = 5.0f;
	[SerializeField]
	private float m_Gravity = 5.0f;
	[SerializeField]
	private float m_GravitySeconds = 2.0f;
	[SerializeField]
	private float m_GravityDelay = 5.0f;

	[Header("Raycast")]
	[SerializeField]
	private float m_RaycastBackDistance = 0.5f;
	[SerializeField]
	private float m_RaycastForwardDistance = 0.5f;
	[SerializeField]
	private LayerMask m_Layers = new();

	private Vector3 m_Direction;
	private float m_TimeElapsed = 0.0f;

	internal override PlayerSpear.State State => PlayerSpear.State.Thrown;

	internal void Start(Vector3 pPoint, Vector3 pDirection/*, float pCharge01*/)
	{
		m_TimeElapsed = 0.0f;
		Transform.position = pPoint;
		Transform.forward = pDirection;
		m_Direction = pDirection;
	}

	internal override void Stop()
	{

	}

	internal override void Tick(float pDeltaTime)
	{
		m_TimeElapsed += pDeltaTime;

		Vector3 newPosition = Transform.position + (pDeltaTime * m_Speed * m_Direction);
		newPosition.y -= Mathf.Lerp(0.0f, pDeltaTime * m_Gravity, (m_TimeElapsed - m_GravityDelay) / m_GravitySeconds);
		Vector3 forward = newPosition - Transform.position;

		Vector3 startPoint = Transform.position - (forward * m_RaycastBackDistance);
		Vector3 endPoint = newPosition + (forward * m_RaycastForwardDistance);
		if (!Physics.Linecast(startPoint, endPoint, out RaycastHit hit, m_Layers))
		{
			Transform.forward = forward;
			Transform.position = newPosition;
			return;
		}
		Transform.forward = forward;
		Transform.position = hit.point /*+ (forward * m_RaycastForwardDistance)*/;
		// Transform.forward = hit.normal;
		Spear.Attach(hit.collider.transform, hit.point);
	}

	internal override void DrawGizmos()
	{
		Vector3 startPoint = Transform.position - (Transform.forward * m_RaycastBackDistance);
		Vector3 endPoint = Transform.position + (Transform.forward * m_RaycastForwardDistance);
		Gizmos.color = Colour.DarkRed;
		Gizmos.DrawLine(startPoint, endPoint);
	}
}
