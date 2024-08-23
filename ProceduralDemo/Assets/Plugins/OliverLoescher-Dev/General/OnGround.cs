using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System;
using ODev.Util;

namespace ODev
{
	public class OnGround : MonoBehaviour
	{
		public enum State
		{
			None,
			InAir,
			OnSlope,
			OnGround,
		}

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

		[FoldoutGroup("Events")]
		public UnityEvent OnAirEnterEvent;
		[FoldoutGroup("Events")]
		public UnityEvent OnAirExitEvent;
		[FoldoutGroup("Events")]
		public UnityEvent OnSlopeEnterEvent;
		[FoldoutGroup("Events")]
		public UnityEvent OnSlopeExitEvent;
		[FoldoutGroup("Events")]
		public UnityEvent OnGroundEnterEvent;
		[FoldoutGroup("Events")]
		public UnityEvent OnGroundExitEvent;

		private State m_State = State.None;
		private readonly TransformFollower m_Follower = new();
		private TransformFollower.IMotionReciver m_MotionReciever;

		public bool IsInAir => m_State == State.InAir;
		public bool IsOnSlope => m_State == State.OnSlope;
		public bool IsOnGround => m_State == State.OnGround;

		private void Start()
		{
			if (m_Transform == null)
			{
				m_Transform = transform;
			}
			m_Transform.TryGetComponent(out m_MotionReciever);
		}

		private void OnDestroy()
		{
			m_Follower.OnDestroy();
		}

		private void OnEnable()
		{
			m_Updateable.Register(Tick);
		}

		private void OnDisable()
		{
			m_Updateable.Deregister();
			SetState(State.None);
		}


		private void Tick(float pDeltaTime)
		{
			if (CastToGrounded())
			{
				if (IsGroundValid())
				{
					SetState(State.OnGround);
				}
				else
				{
					SetState(State.OnSlope);
				}
			}
			else
			{
				SetState(State.InAir);
			}

			if (!m_Follower.IsStarted || !TryGetFirstGroundTransform(out Transform target) || m_Follower.ParentTransform == target)
			{
				return;
			}
			m_Follower.ChangeParent(target);
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
			if (total.sqrMagnitude.IsNearZero()) // If no normal was added
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

		public bool TryGetFirstGroundTransform(out Transform pTransform)
		{
			foreach (Linecast line in m_Lines)
			{
				if (line.HitInfo.collider != null)
				{
					pTransform = line.HitInfo.transform;
					return true;
				}
			}
			foreach (Spherecast sphere in m_Spheres)
			{
				if (sphere.HitInfo.collider != null)
				{
					pTransform = sphere.HitInfo.transform;
					return true;
				}
			}
			pTransform = null;
			return false;
		}

		private void SetState(State pToState)
		{
			if (m_State == pToState)
			{
				return;
			}
			// Util.Debug.Log($"Switching state from {m_State} to {pToState}", this);
			switch (m_State) // EXIT
			{
				case State.InAir:
					OnAirExitEvent?.Invoke();
					break;

				case State.OnSlope:
					OnSlopeExitEvent?.Invoke();
					break;

				case State.OnGround:
					OnGroundExitEvent?.Invoke();
					if (m_FollowGround)
					{
						m_Follower.Stop();
					}
					break;
			}
			m_State = pToState;
			switch (m_State) // ENTER
			{
				case State.InAir:
					OnAirEnterEvent?.Invoke();
					break;

				case State.OnSlope:
					OnSlopeEnterEvent?.Invoke();
					break;

				case State.OnGround:
					OnGroundEnterEvent?.Invoke();
					if (m_FollowGround && TryGetFirstGroundTransform(out Transform target))
					{
						m_Follower.Start(target, m_MotionReciever, GetAveragePoint(), false, m_Updateable.Type, m_Updateable.Priority - 1, this);
					}
					break;
			}
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