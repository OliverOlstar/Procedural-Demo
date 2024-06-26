using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System;

namespace OCore
{
	public class OnGround : MonoBehaviour
	{
		[Serializable]
		private class Linecast
		{
			[SerializeField]
			private Vector3 m_StartPosition = new();
			[SerializeField]
			private float m_Distance = 1.0f;

			[HideInInspector]
			public RaycastHit HitInfo = new();

			public bool Check(Transform pTransform, LayerMask pLayerMask)
			{
				Vector3 start = pTransform.TransformPoint(m_StartPosition);
				Vector3 end = start - (pTransform.up * m_Distance);
				return Physics.Linecast(start, end, out HitInfo, pLayerMask);
			}

			public void OnDrawGizmos(Transform pTransform, LayerMask pLayerMask, Func<RaycastHit, bool> pIsValid = null)
			{
				Vector3 start = pTransform.TransformPoint(m_StartPosition);
				Vector3 end = start - (pTransform.up * m_Distance);
				if (Check(pTransform, pLayerMask))
				{
					Gizmos.color = (pIsValid != null && pIsValid.Invoke(HitInfo)) ? Color.green : Color.yellow;
				}
				else
				{
					Gizmos.color = Color.red;
				}
				Gizmos.DrawLine(start, end);
			}
		}

		[Serializable]
		private class Spherecast
		{
			[SerializeField]
			private Vector3 m_StartPosition = new();
			[SerializeField]
			private float m_Distance = 1.0f;
			[SerializeField]
			private float m_Radius = 0.5f;

			[HideInInspector]
			public RaycastHit HitInfo = new();

			public bool Check(Transform pTransform, LayerMask pLayerMask)
			{
				Vector3 start = pTransform.TransformPoint(m_StartPosition);
				return Physics.SphereCast(start, m_Radius, -pTransform.up, out HitInfo, m_Distance, pLayerMask);
			}

			public void OnDrawGizmos(Transform pTransform, LayerMask pLayerMask, Func<RaycastHit, bool> pIsValid = null)
			{
				Vector3 start = pTransform.TransformPoint(m_StartPosition);
				Vector3 end = start - (pTransform.up * m_Distance);
				Gizmos.DrawWireSphere(start, m_Radius);
				if (Check(pTransform, pLayerMask))
				{
					Gizmos.color = (pIsValid != null && pIsValid.Invoke(HitInfo)) ? Color.green : Color.yellow;
				}
				else
				{
					Gizmos.color = Color.red;
				}
				Gizmos.DrawWireSphere(end, m_Radius);
				Gizmos.DrawLine(start, end);
			}
		}

		[SerializeField]
		private Util.Mono.Updateable m_Updateable = new(Util.Mono.Type.Fixed, Util.Mono.Priorities.OnGround);
		[SerializeField]
		private Transform m_Transform;
		[SerializeField]
		private Linecast[] m_Lines = new Linecast[0];
		[SerializeField]
		private Spherecast[] m_Spheres = new Spherecast[1];
		[SerializeField]
		private LayerMask m_LayerMask = new();
		[SerializeField]
		private float m_SlopeLimit = 45;
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
			if (m_Transform == null)
			{
				m_Transform = transform;
			}
			if (!m_Transform.TryGetComponent(out m_Rigidbody))
			{
				m_Transform.TryGetComponent(out m_Controller);
			}
			m_Updateable.Register(Tick);
		}

		private void OnDestroy()
		{
			m_Updateable.Deregister();
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
			if (!m_Follower.IsStarted || m_Follower.ParentTransform == groundTransform)
			{
				return;
			}
			m_Follower.ChangeParent(groundTransform);
		}

		private bool CastToGrounded()
		{
			foreach (Linecast line in m_Lines)
			{
				if (line.Check(m_Transform, m_LayerMask))
				{
					return true;
				}
			}
			foreach (Spherecast sphere in m_Spheres)
			{
				if (sphere.Check(m_Transform, m_LayerMask))
				{
					return true;
				}
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
			return Vector3.Angle(Vector3.up, pNormal) < m_SlopeLimit;
		}

		public Vector3 GetAverageNormal()
		{
			Vector3 total = Vector3.zero;
			foreach (Linecast line in m_Lines)
			{
				total += line.HitInfo.normal;
			}
			foreach (Spherecast sphere in m_Spheres)
			{
				total += sphere.HitInfo.normal;
			}
			if (total.sqrMagnitude < 1.0f) // If no normal was added
			{
				return Vector3.up;
			}
			return total / (m_Lines.Length + m_Spheres.Length);
		}

		public Vector3 GetAveragePoint()
		{
			Vector3 total = Vector3.zero;
			foreach (Linecast line in m_Lines)
			{
				total += line.HitInfo.point;
			}
			foreach (Spherecast sphere in m_Spheres)
			{
				total += sphere.HitInfo.point;
			}
			return total / (m_Lines.Length + m_Spheres.Length);
		}

		public Transform GetFirstGroundTransform()
		{
			foreach (Linecast line in m_Lines)
			{
				if (line.HitInfo.collider != null)
				{
					return line.HitInfo.transform;
				}
			}
			foreach (Spherecast sphere in m_Spheres)
			{
				if (sphere.HitInfo.collider != null)
				{
					return sphere.HitInfo.transform;
				}
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
			if (!m_FollowGround)
			{
				return;
			}

			if (m_Rigidbody)
			{
				m_Follower.Start(pTarget, m_Rigidbody, GetAveragePoint(), null, false, m_Updateable.Type, m_Updateable.Priority, this);
			}
			else if (m_Controller)
			{
				m_Follower.Start(pTarget, m_Controller, GetAveragePoint(), null, false, m_Updateable.Type, m_Updateable.Priority, this);
			}
			else
			{
				m_Follower.Start(pTarget, m_Transform, GetAveragePoint(), null, false, m_Updateable.Type, m_Updateable.Priority, this);
			}
		}

		private void OnExit()
		{
			OnExitEvent?.Invoke();
			if (!m_FollowGround)
			{
				return;
			}
			m_Follower.Stop();
		}

		private void OnDrawGizmos()
		{
			if (m_Transform == null)
			{
				m_Transform = transform;
			}
			foreach (Linecast line in m_Lines)
			{
				line.OnDrawGizmos(m_Transform, m_LayerMask, (RaycastHit pHit) => IsGroundValidInternal(pHit.normal));
			}
			foreach (Spherecast sphere in m_Spheres)
			{
				sphere.OnDrawGizmos(m_Transform, m_LayerMask, (RaycastHit pHit) => IsGroundValidInternal(pHit.normal));
			}
			m_Follower.OnDrawGizmos();
		}
	}
}