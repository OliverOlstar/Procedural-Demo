using ODev.Util;
using UnityEngine;

namespace ODev
{
	/// <summary>
	/// A solution to follow transform's movement with an offset without childing your transform to it
	/// </summary>
	public class TransformFollower
	{
		public interface IMotionReciver
		{
			public Transform Transform { get; }
			public void AddDisplacement(Vector3 pMovement, Quaternion pRotation);
		}

		private class TransformMotionReciver : IMotionReciver
		{
			private readonly Transform m_Transform;
			Transform IMotionReciver.Transform => m_Transform;

			public TransformMotionReciver(Transform transform) => m_Transform = transform;

			void IMotionReciver.AddDisplacement(Vector3 pMovement, Quaternion pRotation)
			{
				m_Transform.position += pMovement;
				m_Transform.rotation *= pRotation;
			}
		}

		private Util.Mono.Updateable m_Updateable = new();
		private Transform m_Parent;
		private IMotionReciver m_Child;
		private Object m_DebugObject;
		private bool m_RotateChild;

		private Transform m_MathTransform = null;
		private Vector3 m_ParentLastPosition = Vector3.zero;
		private Quaternion m_LastRotation = Quaternion.identity;
		private Vector3 m_LocalPosition = Vector3.zero;

		public bool IsStarted => m_Updateable.IsRegistered;
		public Transform ParentTransform => m_Parent;
		public Transform ChildTransform => m_Child.Transform;

		public void Start(Transform pParent, IMotionReciver pChild, Vector3 pPoint, bool pRotateChild, Util.Mono.Type pUpdateType, Util.Mono.Priorities pUpdatePriority, Object pDebugParent)
		{
			m_DebugObject = pDebugParent;
			if (pChild == null)
			{
				Util.Debug.DevException("pChild is null", m_DebugObject);
				return;
			}
			if (pParent == null)
			{
				Util.Debug.DevException("pParent is null", m_DebugObject);
				return;
			}
			if (m_Child != null) // Already started
			{
				if (m_Child != pChild)
				{
					Util.Debug.DevException("Already started but with a different child transform", m_DebugObject);
				}
				else if (pParent != m_Parent)
				{
					Util.Debug.DevException("Already started but with a different parent transform", m_DebugObject);
				}
				else
				{
					Util.Debug.LogError("Already started", m_DebugObject);
				}
				return;
			}
			m_Parent = pParent;
			m_Child = pChild;
			m_RotateChild = pRotateChild;

			m_ParentLastPosition = pPoint;
			m_LastRotation = m_Parent.rotation;
			m_LocalPosition = m_Parent.InverseTransformPoint(pPoint);

			m_Updateable.SetProperties(pUpdateType, pUpdatePriority);
			m_Updateable.Register(Tick);
		}

		public void Start(Transform pParent, Transform pChild, Vector3 pPoint, bool pRotateChild, Util.Mono.Type pUpdateType, Util.Mono.Priorities pUpdatePriority, Object pDebugParent)
			=> Start(pParent, new TransformMotionReciver(pChild), pPoint, pRotateChild, pUpdateType, pUpdatePriority, pDebugParent);

		public void Stop()
		{
			if (m_Child == null)
			{
				Util.Debug.LogWarning("Not started, please start first", m_DebugObject);
				return;
			}
			m_Parent = null;
			m_Child = null;

			m_Updateable.Deregister();
		}

		public void ChangeParent(Transform pParent)
		{
			if (pParent == null)
			{
				Util.Debug.DevException("pParent is null", m_DebugObject);
				return;
			}
			m_Parent = pParent;

			m_LastRotation = m_Parent.rotation;
			m_LocalPosition = m_Parent.InverseTransformPoint(m_ParentLastPosition);
		}

		private void Tick(float pDeltaTime)
		{
			if (m_Parent == null || m_Child == null)
			{
				Util.Debug.LogWarning("We aren't registered, we shouldn't get here. Might be an order of operations issue", m_DebugObject);
				return;
			}
			Vector3 currPositon = m_Parent.TransformPoint(m_LocalPosition);
			Vector3 deltaPosition = currPositon - m_ParentLastPosition;
			if (deltaPosition.IsNearZero())
			{
				return;
			}

			if (m_MathTransform == null)
			{
				m_MathTransform = new GameObject($"{m_Child.Transform.name}-TransformFollower-Math").transform;
				m_MathTransform.SetParent(m_Parent);
			}
			m_MathTransform.position = m_Child.Transform.position;
			m_MathTransform.position += deltaPosition;
			if (m_RotateChild)
			{
				Quaternion deltaRotation = m_LastRotation.Difference(m_Parent.rotation);
				deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
				m_MathTransform.RotateAround(currPositon, axis, -angle);
				m_LastRotation = m_Parent.rotation;
			}
			m_ParentLastPosition = currPositon;

			if (currPositon != m_MathTransform.position)
			{
				m_ParentLastPosition += m_MathTransform.position - currPositon;
				m_LocalPosition = m_Parent.InverseTransformPoint(m_MathTransform.position);
			}

			m_Child.AddDisplacement(m_MathTransform.position - m_Child.Transform.position, Math.Difference(m_MathTransform.rotation, m_Child.Transform.rotation));
		}

		public void OnDestroy()
		{
			if (m_Child != null)
			{
				Stop();
			}
			if (m_MathTransform != null)
			{
				Object.Destroy(m_MathTransform.gameObject);
			}
		}

		public void OnDrawGizmos()
		{
			if (m_Child == null)
			{
				return;
			}
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(m_Parent.TransformPoint(m_LocalPosition), 0.5f);
			Gizmos.DrawSphere(m_Parent.TransformPoint(m_LocalPosition), 0.25f);
		}
	}
}
