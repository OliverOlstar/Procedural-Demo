using System.Collections.Generic;
using UnityEngine;
using ODev.Util;

namespace PA
{
	public class PABody
    {
		private readonly PARoot m_Root;
		private readonly SOBody m_Data;
		private readonly Transform m_Transform;
		private IPACharacter Character => m_Root.Character;
		private IEnumerable<PAPoint> Points => m_Root.GetAllPoints();
		private int PointsCount => m_Root.PointsCount;
		
		private Vector2 m_VelocityXZ = Vector2.zero;
		private Vector3 m_PositionLocalOffset;
		private float m_VelocityY = 0.0f;

		public PABody(SOBody pData, Transform pTransform, PARoot pRoot)
		{
			m_Root = pRoot;
			m_Data = pData;
			m_Transform = pTransform;
			m_PositionLocalOffset = m_Transform.localPosition;
		}

		public void Tick(float pDeltaTime)
		{
			if (PointsCount == 0)
			{
				return;
			}
			
			// Position
			Vector3 position = Vector3.zero;
			foreach (PAPoint point in Points)
			{
				position += (point.Position * 2.0f) - point.OriginalPositionWorld();
			}
			Vector3 targetPosition = (position / PointsCount) + m_PositionLocalOffset;
			Vector2 positionXZ = Func.SpringDamper(Math.Horizontal2D(m_Transform.position), Math.Horizontal2D(targetPosition), ref m_VelocityXZ, m_Data.SpringXZ, m_Data.DamperXZ, pDeltaTime);
			float positionY = Func.SpringDamper(m_Transform.position.y, targetPosition.y, ref m_VelocityY, m_Data.SpringY, m_Data.DamperY, pDeltaTime);
			m_Transform.position = Math.Combine(positionXZ, positionY);

			// Rotation
			Vector3 up = Vector3.zero;
			float angle = 0.0f;
			foreach (PAPoint point in Points)
			{
				// Up
				Vector3 direction = (m_Transform.position - point.Position).normalized;
				Vector3 right = Vector3.Cross(direction, Character.Up);
				up += -Vector3.Cross(direction, right);

				// Angle
				Vector3 directionOriginal = Math.Horizontalize(m_Transform.position - point.OriginalPositionWorld());
				direction = Math.Horizontalize(direction);
				angle += Vector3.SignedAngle(directionOriginal, direction, Character.Up);
			}

			// Apply Rotation
			Quaternion targetRotation = Math.UpForwardRotation(Character.Forward, up.normalized);
			targetRotation = Quaternion.LerpUnclamped(Quaternion.LookRotation(Character.Forward), targetRotation, m_Data.RotationBlendY);
			targetRotation *= Quaternion.Euler(0.0f, m_Data.RotationBlendXZ * (angle / PointsCount), 0.0f);

			m_Transform.localRotation = Quaternion.Lerp(m_Transform.localRotation, targetRotation, pDeltaTime * m_Data.RotationDampening);

			// TODO Add Leaning
		}

		public void DrawGizmos()
		{
			foreach (PAPoint point in Points)
			{
				Vector3 direction = (m_Transform.position - point.Position).normalized;
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
			foreach (PAPoint point in Points)
			{
				position += point.Position;
			}
			Vector3 targetPosition = (position / PointsCount) + m_PositionLocalOffset;
			Gizmos.DrawSphere(targetPosition, 1.0f);
		}
    }
}
