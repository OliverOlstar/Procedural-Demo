using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System;

namespace OliverLoescher
{
	public class OnGround : MonoBehaviour
	{
		[System.Serializable]
		private class Linecast
		{
			[SerializeField] private Vector3 startPosition = new Vector3();
			[SerializeField] private float distance = 1.0f;

			[HideInInspector] public RaycastHit hitInfo = new RaycastHit();

			public bool Check(Transform pTransform, LayerMask pLayerMask)
			{
				Vector3 start = pTransform.TransformPoint(startPosition);
				Vector3 end = start - (pTransform.up * distance);
				return Physics.Linecast(start, end, out hitInfo, pLayerMask);
			}

			public void OnDrawGizmos(Transform pTransform, LayerMask pLayerMask, Func<RaycastHit, bool> pIsValid = null)
			{
				Vector3 start = pTransform.TransformPoint(startPosition);
				Vector3 end = start - (pTransform.up * distance);
				if (Check(pTransform, pLayerMask))
				{
					Gizmos.color = (pIsValid != null && pIsValid.Invoke(hitInfo)) ? Color.green : Color.yellow;
				}
				else
				{
					Gizmos.color = Color.red;
				}
				Gizmos.DrawLine(start, end);
			}
		}

		[System.Serializable]
		private class Spherecast
		{
			[SerializeField] private Vector3 startPosition = new Vector3();
			[SerializeField] private float distance = 1.0f;
			[SerializeField] private float radius = 0.5f;

			[HideInInspector] public RaycastHit hitInfo = new RaycastHit();

			public bool Check(Transform pTransform, LayerMask pLayerMask)
			{
				Vector3 start = pTransform.TransformPoint(startPosition);
				return Physics.SphereCast(start, radius, -pTransform.up, out hitInfo, distance, pLayerMask);
			}

			public void OnDrawGizmos(Transform pTransform, LayerMask pLayerMask, Func<RaycastHit, bool> pIsValid = null)
			{
				Vector3 start = pTransform.TransformPoint(startPosition);
				Vector3 end = start - (pTransform.up * distance);
				Gizmos.DrawWireSphere(start, radius);
				if (Check(pTransform, pLayerMask))
				{
					Gizmos.color = (pIsValid != null && pIsValid.Invoke(hitInfo)) ? Color.green : Color.yellow;
				}
				else
				{
					Gizmos.color = Color.red;
				}
				Gizmos.DrawWireSphere(end, radius);
				Gizmos.DrawLine(start, end);
			}
		}

		[SerializeField]
		private Util.Mono.Updateable updateable = new(Util.Mono.Type.Fixed, Util.Mono.Priorities.OnGround);
		[SerializeField]
		private Transform myTransform;
		[SerializeField]
		private Linecast[] lines = new Linecast[0];
		[SerializeField]
		private Spherecast[] spheres = new Spherecast[1];
		[SerializeField]
		private LayerMask layerMask = new();
		[SerializeField]
		private float slopeLimit = 45;
		[SerializeField]
		private bool m_FollowGround = false;

		public bool IsGrounded { get; private set; }
		[FoldoutGroup("Events")]
		public UnityEvent OnEnterEvent;
		[FoldoutGroup("Events")]
		public UnityEvent OnExitEvent;

		private readonly TransformFollower m_Follower = new();
		private CharacterController m_Controller;
		private Rigidbody m_Rigidbody;

		private void Start()
		{
			if (myTransform == null)
			{
				myTransform = transform;
			}
			if (!myTransform.TryGetComponent(out m_Rigidbody))
				myTransform.TryGetComponent(out m_Controller);

			updateable.Register(Tick);
		}
		
		private void OnDestroy()
		{
			updateable.Deregister();
			m_Follower.OnDestroy();
		}

		private void Tick(float pDeltaTime)
		{
			if ((CastToGrounded() && IsGroundValid()) != IsGrounded)
			{
				IsGrounded = !IsGrounded;
				if (IsGrounded)
				{
					OnEnter();
				}
				else
				{
					OnExit();
				}
			}

			Transform groundTransform = GetFirstGroundTransform();
			if (m_Follower.IsStarted && m_Follower.ParentTransform != groundTransform)
			{
				m_Follower.ChangeParent(groundTransform);
			}
		}

		private bool CastToGrounded()
		{
			foreach (Linecast line in lines)
			{
				if (line.Check(myTransform, layerMask))
					return true;
			}
			foreach (Spherecast sphere in spheres)
			{
				if (sphere.Check(myTransform, layerMask))
					return true;
			}
			return false;
		}

		private bool IsGroundValid()
		{
			Vector3 normal = GetAverageNormal();
			return IsGroundValidInternal(normal);
		}

		private bool IsGroundValidInternal(Vector3 pNormal)
		{
			return Vector3.Angle(Vector3.up, pNormal) < slopeLimit;
		}

		public Vector3 GetAverageNormal()
		{
			Vector3 total = Vector3.zero;
			foreach (Linecast line in lines)
			{
				total += line.hitInfo.normal;
			}
			foreach (Spherecast sphere in spheres)
			{
				total += sphere.hitInfo.normal;
			}
			if (total.sqrMagnitude < 1.0f) // If no normal was added
			{
				return Vector3.up;
			}
			return total / (lines.Length + spheres.Length);
		}

		public Vector3 GetAveragePoint()
		{
			Vector3 total = Vector3.zero;
			foreach (Linecast line in lines)
			{
				total += line.hitInfo.point;
			}
			foreach (Spherecast sphere in spheres)
			{
				total += sphere.hitInfo.point;
			}
			return total / (lines.Length + spheres.Length);
		}

		public Transform GetFirstGroundTransform()
		{
			foreach (Linecast line in lines)
			{
				if (line.hitInfo.collider != null)
					return line.hitInfo.transform;
			}
			foreach (Spherecast sphere in spheres)
			{
				if (sphere.hitInfo.collider != null)
					return sphere.hitInfo.transform;
			}
			return null;
		}

		private void OnEnter()
		{
			OnEnterEvent?.Invoke();
			SetFollowTarget(GetFirstGroundTransform());
		}

		private void SetFollowTarget(Transform pTarget)
		{
			if (m_FollowGround)
			{
				if (m_Rigidbody)
				{
					m_Follower.Start(pTarget, m_Rigidbody, GetAveragePoint(), null, false, updateable.Type, updateable.Priority, this);
				}
				else if (m_Controller)
				{
					m_Follower.Start(pTarget, m_Controller, GetAveragePoint(), null, false, updateable.Type, updateable.Priority, this);
				}
				else
				{
					m_Follower.Start(pTarget, myTransform, GetAveragePoint(), null, false, updateable.Type, updateable.Priority, this);
				}
			}
		}

		private void OnExit()
		{
			OnExitEvent?.Invoke();
			if (m_FollowGround)
			{
				m_Follower.Stop();
			}
		}

		private void OnDrawGizmos()
		{
			if (myTransform == null)
			{
				myTransform = transform;
			}
			foreach (Linecast line in lines)
			{
				line.OnDrawGizmos(myTransform, layerMask, (RaycastHit pHit) => IsGroundValidInternal(pHit.normal));
			}
			foreach (Spherecast sphere in spheres)
			{
				sphere.OnDrawGizmos(myTransform, layerMask, (RaycastHit pHit) => IsGroundValidInternal(pHit.normal));
			}
			m_Follower.OnDrawGizmos();
		}
	}
}