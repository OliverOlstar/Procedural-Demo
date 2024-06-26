using Sirenix.OdinInspector;
using UnityEngine;

namespace OCore
{
	[System.Serializable]
	public struct Capsule : IPrimitive
	{
		public Vector3 Center;
		public float Radius;
		public float Height;
		public Transform UpTransform;

		public readonly Vector3 Up => UpTransform != null ? UpTransform.up : Vector3.up;

		public Capsule(Transform pUpTransform)
		{
			Center = default;
			Radius = 0.5f;
			Height = 2.0f;
			UpTransform = pUpTransform;
			m_RaycastHits = new RaycastHit[0];
		}
		public Capsule(Vector3 pCenter, float pRadius, float pHeight, Transform pUpTransform = null)
		{
			Center = pCenter;
			Radius = pRadius;
			Height = pHeight;
			UpTransform = pUpTransform;
			m_RaycastHits = new RaycastHit[0];
		}

		public readonly bool PointIntersects(Vector3 pPoint, Vector3 pPosition)
		{
			Vector3 worldSpaceCenter = pPosition + Center;
			float sphereHeight = Height - (Radius * 2.0f);
			if (sphereHeight <= 0.0f) // Is a sphere
			{
				return Util.Math.DistanceGreaterThan(pPoint, worldSpaceCenter, Radius);
			}
			else if (pPoint.y > worldSpaceCenter.y + sphereHeight * 0.5f) // Is above
			{
				return Util.Math.DistanceGreaterThan(pPoint, worldSpaceCenter + (0.5f * sphereHeight * Up), Radius);
			}
			else if (pPoint.y < worldSpaceCenter.y - sphereHeight * 0.5f) // If below
			{
				return Util.Math.DistanceGreaterThan(pPoint, worldSpaceCenter + (0.5f * sphereHeight * -Up), Radius);
			}
			else // Is at same height
			{
				return Util.Math.DistanceOnPlaneEqualGreaterThan(pPoint, worldSpaceCenter, Radius, Up);
			}
		}

		private readonly RaycastHit[] m_RaycastHits;
		public readonly bool CheckCollisions(Vector3 pMovement, Vector3 pPosition, LayerMask pLayerMask, out Vector3 pResultPosition, out Vector3 pCollisionNormal)
		{
			pResultPosition = pPosition + pMovement;
			pCollisionNormal = Up; // Default result

			if (pMovement.sqrMagnitude == 0)
			{
				return false;
			}

			// Raycast
			RaycastHit? nearestHit = null;
			Vector3 up = Up * ((Height * 0.5f) - Radius);
			if (Physics.CapsuleCastNonAlloc(pPosition + Center + up, pPosition + Center - up, Radius, pMovement, m_RaycastHits, pMovement.magnitude, pLayerMask) <= 0)
			{
				return false;
			}

			foreach (RaycastHit hit in m_RaycastHits)
			{
				if (!nearestHit.HasValue || nearestHit.Value.distance > hit.distance)
				{
					nearestHit = hit;
				}
			}

			// Collision
			if (nearestHit.HasValue)
			{
				Vector3 movementToTarget = pMovement.normalized * (nearestHit.Value.distance - Util.Math.NEARZERO);
				pResultPosition = pPosition + movementToTarget;
				pCollisionNormal = /*IsValidGround(nearestHit.Value.normal) ?*/ nearestHit.Value.normal /*: Util.Horizontalize(nearestHit.Value.normal, Up, true)*/; // Slope you can't walk up or down, consider them as just flat walls
				return true;
			}

			return false;
		}

		public readonly bool CheckCollisions(Vector3 pMovement, Vector3 pPosition, int pBounces, LayerMask pLayerMask, out Vector3 resultPosition, out Vector3 collisionNormal)
		{
			bool didCollide = false;
			do
			{
				if (!CheckCollisions(pMovement, pPosition, pLayerMask, out resultPosition, out collisionNormal))
				{
					break; // No collision, stop bouncing
				}
				didCollide = true;

				// Bounce
				Vector3 movementToTarget = resultPosition - pPosition;
				pMovement = Vector3.ProjectOnPlane(pMovement - movementToTarget, collisionNormal);
				pPosition = resultPosition;
				pBounces--;
			}
			while (pBounces >= 0);
			return didCollide;
		}

		public readonly void DrawGizmos(Vector3 pPosition)
		{
			pPosition += Center;
			Vector3 up = Up * ((Height * 0.5f) - Radius);
			Util.Debug2.GizmoCapsule(pPosition + up, pPosition - up, Radius);
		}
	}

	[System.Serializable]
	public struct CharacterControllerCapsule : IPrimitive
	{
		public CharacterController Controller;
		[DisableInPlayMode]
		public Transform UpTransform; // Copy so we can set in inspector but not have to show our capsule member
		[HideInInspector]
		public Capsule Capsule;

		public CharacterControllerCapsule(CharacterController pController, Transform pUpTransform = null)
		{
			Controller = pController;
			UpTransform = pUpTransform;
			Capsule = new Capsule(Controller.center, Controller.radius, Controller.height, UpTransform);
		}

		public readonly Vector3 Up => Capsule.Up;

		private void UpdateCapsuleValues()
		{
			if (Controller == null)
			{
				Debug.LogError("CharacterControllerCapsule.UpdateCapsuleValues() But has no characterController, this should never happen.");
				return;
			}
			Capsule.Center = Controller.center;
			Capsule.Radius = Controller.radius;
			Capsule.Height = Controller.height;
		}

		public bool PointIntersects(Vector3 pPoint, Vector3 pPosition)
		{
			UpdateCapsuleValues();
			return Capsule.PointIntersects(pPoint, pPosition);
		}

		public bool CheckCollisions(Vector3 pMovement, Vector3 pPosition, int pBounces, LayerMask pLayerMask, out Vector3 resultPosition, out Vector3 collisionNormal) // Returns resulting position
		{
			UpdateCapsuleValues();
			return Capsule.CheckCollisions(pMovement, pPosition, pBounces, pLayerMask, out resultPosition, out collisionNormal);
		}

		public void DrawGizmos(Vector3 pPosition)
		{
			UpdateCapsuleValues();
			Capsule.DrawGizmos(pPosition);
		}
	}
}
