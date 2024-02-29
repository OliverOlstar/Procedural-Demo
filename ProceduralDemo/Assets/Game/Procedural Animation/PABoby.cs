using System.Collections;
using System.Collections.Generic;
using OliverLoescher.Util;
using UnityEngine;

public class PABoby : MonoBehaviour, IPABody
{
	private PARoot Root;
	private PACharacter Character => Root.Character;
	private IPAPoint[] Points => Root.Points;

	[Header("Position")]
	[SerializeField]
	private float Spring = 1.0f;
	[SerializeField]
	private float Damper = 10.0f;
	private Vector3 Velocity = Vector3.zero;

	[Header("Rotation")]
	[SerializeField]
	private float RotationDampening = 5.0f;
	[SerializeField, Range(0.0f, 5.0f)]
	private float VerticalRotationBlend = 1.0f;
	[SerializeField, Range(0.0f, 5.0f)]
	private float HorizontalRotationBlend = 1.0f;

	void IPABody.Init(PARoot pRoot) => Root = pRoot;

	void IPABody.Tick(float pDeltaTime)
	{
		// Position
		Vector3 position = Vector3.zero;
		foreach (IPAPoint point in Points)
		{
			position += point.Position;
		}
		Vector3 targetPosition = position / Points.Length;
		transform.position = Func.SpringDamper(transform.position, targetPosition, ref Velocity, Spring, Damper, pDeltaTime);

		// Rotation
		Vector3 up = Vector3.zero;
		float angle = 0.0f;
		foreach (IPAPoint point in Points)
		{
			// Up
			Vector3 direction = (transform.position - point.Position).normalized;
			Vector3 right = Vector3.Cross(direction, Character.Up);
			up += -Vector3.Cross(direction, right);
			
			// Angle
			Vector3 directionOriginal = Math.Horizontalize(transform.position - point.RelativeOriginalPosition);
			angle += Vector3.SignedAngle(directionOriginal, direction, Character.Up);
		}

		// Apply Rotation
		Quaternion targetRotation = Math.UpForwardRotation(Character.Forward, up.normalized);
		targetRotation = Quaternion.LerpUnclamped(Quaternion.LookRotation(Character.Forward), targetRotation, VerticalRotationBlend);
		targetRotation *= Quaternion.Euler(0.0f, (angle / Points.Length) * HorizontalRotationBlend, 0.0f);

		transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, pDeltaTime * RotationDampening);
	}

	void IPABody.DrawGizmos()
	{
		foreach (IPAPoint point in Points)
		{
			Vector3 direction = (transform.position - point.Position).normalized;
			Vector3 right = Vector3.Cross(direction, Character.Up);
			Vector3 up = -Vector3.Cross(direction, right);
			
			Gizmos.color = Color.red;
			Gizmos.DrawLine(point.Position, point.Position + direction);
			Gizmos.color = Color.green;
			Gizmos.DrawLine(point.Position, point.Position + Character.Up);
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(point.Position, point.Position + right);
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(point.Position, point.Position + up);
		}
	}
}
