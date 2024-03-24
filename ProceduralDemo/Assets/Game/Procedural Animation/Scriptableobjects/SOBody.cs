using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverLoescher.Util;

namespace PA
{
	[CreateAssetMenu(fileName = "New PA Body", menuName = "Procedural Animation/Body")]
	public class SOBody : ScriptableObject
	{
		private PARoot2 m_Root;
		private IPACharacter Character => m_Root.Character;
		private List<IPAPoint> Points => m_Root.Points;

		[Header("Position")]
		[SerializeField]
		private float m_SpringXZ = 1.0f;
		[SerializeField]
		private float m_DamperXZ = 0.1f;
		private Vector2 m_VelocityXZ = Vector2.zero;
		private Vector3 m_PositionLocalOffset;

		[SerializeField]
		private float m_SpringY = 1.0f;
		[SerializeField]
		private float m_DamperY = 0.1f;
		private float m_VelocityY = 0.0f;

		[Header("Rotation")]
		[SerializeField]
		private float m_RotationDampening = 5.0f;
		[SerializeField, Range(0.0f, 5.0f)]
		private float m_RotationBlendXZ = 1.0f;
		[SerializeField, Range(0.0f, 5.0f)]
		private float m_RotationBlendY = 1.0f;

		public Transform Transform => m_Root.BodyTransform;

		public void Init(PARoot2 pRoot)
		{
			m_Root = pRoot;
			m_PositionLocalOffset = Transform.localPosition;
		}

		public void Tick(float pDeltaTime)
		{
			// Position
			Vector3 position = Vector3.zero;
			foreach (IPAPoint point in Points)
			{
				position += point.Position;
			}
			Vector3 targetPosition = (position / Points.Count) + m_PositionLocalOffset;
			Vector2 positionXZ = Func.SpringDamper(Math.Horizontal2D(Transform.position), Math.Horizontal2D(targetPosition), ref m_VelocityXZ, m_SpringXZ, m_DamperXZ, pDeltaTime);
			float positionY = Func.SpringDamper(Transform.position.y, targetPosition.y, ref m_VelocityY, m_SpringY, m_DamperY, pDeltaTime);
			Transform.position = Math.Combine(positionXZ, positionY);

			// Rotation
			Vector3 up = Vector3.zero;
			float angle = 0.0f;
			foreach (IPAPoint point in Points)
			{
				// Up
				Vector3 direction = (Transform.position - point.Position).normalized;
				Vector3 right = Vector3.Cross(direction, Character.Up);
				up += -Vector3.Cross(direction, right);

				// Angle
				Vector3 directionOriginal = Math.Horizontalize(Transform.position - point.RelativeOriginalPosition);
				direction = Math.Horizontalize(direction);
				angle += Vector3.SignedAngle(directionOriginal, direction, Character.Up);
			}

			// Apply Rotation
			Quaternion targetRotation = Math.UpForwardRotation(Character.Forward, up.normalized);
			targetRotation = Quaternion.LerpUnclamped(Quaternion.LookRotation(Character.Forward), targetRotation, m_RotationBlendY);
			targetRotation *= Quaternion.Euler(0.0f, m_RotationBlendXZ * (angle / Points.Count), 0.0f);

			Transform.localRotation = Quaternion.Lerp(Transform.localRotation, targetRotation, pDeltaTime * m_RotationDampening);

			// TODO Add Leaning
		}

		public void DrawGizmos()
		{
			foreach (IPAPoint point in Points)
			{
				Vector3 direction = (Transform.position - point.Position).normalized;
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
			Vector3 targetPosition = (position / Points.Count) + m_PositionLocalOffset;
			Gizmos.DrawSphere(targetPosition, 1.0f);
		}
	}
}
