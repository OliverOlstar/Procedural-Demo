using OliverLoescher.Util;
using Sirenix.Utilities;
using UnityEngine;

public class PABoby : MonoBehaviour, IPABody
{
	private PARoot Root;
	private PACharacter Character => Root.Character;
	private IPAPoint[] Points => Root.Points;

	[Header("Position")]
	[SerializeField]
	private float SpringXZ = 1.0f;
	[SerializeField]
	private float DamperXZ = 0.1f;
	private Vector2 VelocityXZ = Vector2.zero;
	private Vector3 PositionLocalOffset;

	[SerializeField]
	private float SpringY = 1.0f;
	[SerializeField]
	private float DamperY = 0.1f;
	private float VelocityY = 0.0f;

	[Header("Rotation")]
	[SerializeField]
	private float RotationDampening = 5.0f;
	[SerializeField, Range(0.0f, 5.0f)]
	private float RotationBlendXZ = 1.0f;
	[SerializeField, Range(0.0f, 5.0f)]
	private float RotationBlendY = 1.0f;

	//[Header("Leaning")]
	//[SerializeField]
	//private float LeanMagnitude = 5.0f;
	//[SerializeField]
	//private float LeanSmoothTime = 0.2f;

	void IPABody.Init(PARoot pRoot)
	{
		Root = pRoot;
		PositionLocalOffset = transform.localPosition;
	}

	void IPABody.Tick(float pDeltaTime)
	{
		// Position
		Vector3 position = Vector3.zero;
		foreach (IPAPoint point in Points)
		{
			position += point.Position;
		}
		Vector3 targetPosition = (position / Points.Length) + PositionLocalOffset;
		Vector2 positionXZ = Func.SpringDamper(Math.Horizontal2D(transform.position), Math.Horizontal2D(targetPosition), ref VelocityXZ, SpringXZ, DamperXZ, pDeltaTime);
		float positionY = Func.SpringDamper(transform.position.y, targetPosition.y, ref VelocityY, SpringY, DamperY, pDeltaTime);
		transform.position = Math.Combine(positionXZ, positionY);

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
			direction = Math.Horizontalize(direction);
			angle += Vector3.SignedAngle(directionOriginal, direction, Character.Up);
		}

		// Apply Rotation
		Quaternion targetRotation = Math.UpForwardRotation(Character.Forward, up.normalized);
		targetRotation = Quaternion.LerpUnclamped(Quaternion.LookRotation(Character.Forward), targetRotation, RotationBlendY);
		targetRotation *= Quaternion.Euler(0.0f, (angle / Points.Length) * RotationBlendXZ, 0.0f);

		transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, pDeltaTime * RotationDampening);

		// TODO Add Leaning
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

		// Target Pos
		Gizmos.color = Color.green;
		Vector3 position = Vector3.zero;
		foreach (IPAPoint point in Points)
		{
			Gizmos.DrawSphere(point.Position, 2.0f);
			position += point.Position;
		}
		Vector3 targetPosition = (position / Points.Length) + PositionLocalOffset;
		Gizmos.DrawSphere(targetPosition, 1.0f);
	}
}
