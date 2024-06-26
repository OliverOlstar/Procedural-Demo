using OCore.Util;
using UnityEngine;

namespace OCore
{
	/// <summary>
	/// A solution to follow transform's movement with an offset without childing your transform to it
	/// </summary>
	public class TransformFollower
	{
		private Util.Mono.Updateable m_Updateable = new();
		private Transform m_Parent;
		private Transform m_Child;
		private CharacterController m_ChildController;
		private Rigidbody m_ChildRigidbody;
		private Object m_DebugObject;
		private System.Action<Vector3> m_OnMoved;
		private bool m_RotateChild;

		private Vector3 m_LastPosition = Vector3.zero;
		private Quaternion m_LastRotation = Quaternion.identity;
		private Vector3 m_LocalPosition;

		public bool IsStarted => m_Updateable.IsRegistered;
		public Transform ParentTransform => m_Parent;
		public Transform ChildTransform => m_Child;

		public void Start(Transform pParent, Rigidbody pChild, Vector3 pPoint, System.Action<Vector3> pOnMoved, bool pRotateChild, Util.Mono.Type pUpdateType, Util.Mono.Priorities pUpdatePriority, Object pDebugParent)
		{
			m_DebugObject = pDebugParent;
			if (pChild == null)
			{
				Util.Debug2.DevException("pChild (Rigidbody) is null", "Start", m_DebugObject);
				return;
			}
			m_ChildRigidbody = pChild;
			Start(pParent, pChild.transform, pPoint, pOnMoved, pRotateChild, pUpdateType, pUpdatePriority, pDebugParent);
		}

		public void Start(Transform pParent, CharacterController pChild, Vector3 pPoint, System.Action<Vector3> pOnMoved, bool pRotateChild, Util.Mono.Type pUpdateType, Util.Mono.Priorities pUpdatePriority, Object pDebugParent)
		{
			m_DebugObject = pDebugParent;
			if (pChild == null)
			{
				Util.Debug2.DevException("pChild (CharacterController) is null", "Start", m_DebugObject);
				return;
			}
			m_ChildController = pChild;
			Start(pParent, pChild.transform, pPoint, pOnMoved, pRotateChild, pUpdateType, pUpdatePriority, pDebugParent);
		}

		public void Start(Transform pParent, Transform pChild, Vector3 pPoint, System.Action<Vector3> pOnMoved, bool pRotateChild, Util.Mono.Type pUpdateType, Util.Mono.Priorities pUpdatePriority, Object pDebugParent)
		{
			m_DebugObject = pDebugParent;
			if (pChild == null)
			{
				Util.Debug2.DevException("pChild is null", "Start", m_DebugObject);
				return;
			}
			if (pParent == null)
			{
				Util.Debug2.DevException("pParent is null", "Start", m_DebugObject);
				return;
			}
			if (m_Child != null) // Already started
			{
				if (m_Child != pChild)
				{
					Util.Debug2.DevException("Already started but with a different child transform", "Start", m_DebugObject);
				}
				else if (pParent != m_Parent)
				{
					Util.Debug2.DevException("Already started but with a different parent transform", "Start", m_DebugObject);
				}
				else
				{
					Util.Debug2.LogError("Already started", "Start", m_DebugObject);
				}
				return;
			}
			m_Parent = pParent;
			m_Child = pChild;
			m_OnMoved = pOnMoved;
			m_RotateChild = pRotateChild;

			m_LastPosition = pPoint;
			m_LastRotation = m_Parent.rotation;
			m_LocalPosition = m_Parent.InverseTransformPoint(pPoint);

			m_Updateable.SetProperties(pUpdateType, pUpdatePriority);
			m_Updateable.Register(Tick);
		}

		public void Stop()
		{
			if (m_Child == null)
			{
				Util.Debug2.LogWarning("Not started, please start first", "Stop", m_DebugObject);
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
				Util.Debug2.DevException("pParent is null", "ChangePoint", m_DebugObject);
				return;
			}
			m_Parent = pParent;
			
			m_LastRotation = m_Parent.rotation;
			m_LocalPosition = m_Parent.InverseTransformPoint(m_LastPosition);
		}

		private void Tick(float pDeltaTime)
		{
			Vector3 currPositon = m_Parent.TransformPoint(m_LocalPosition);
			Vector3 deltaPosition = currPositon - m_LastPosition;
			if (deltaPosition.IsNearZero())
			{
				return;
			}

			SetControllerAcitve(false); // Disable so we can move the transform
			m_Child.position += deltaPosition;
			if (m_ChildRigidbody != null)
			{
				m_ChildRigidbody.position += deltaPosition;
			}
			if (m_RotateChild)
			{
				Quaternion deltaRotation = m_LastRotation.Difference(m_Parent.rotation);
				deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
				m_Child.RotateAround(currPositon, axis, -angle);
				m_LastRotation = m_Parent.rotation;
			}
			SetControllerAcitve(true);
			m_LastPosition = currPositon;

			if (currPositon != m_Child.position)
			{
				m_LastPosition += m_Child.position - currPositon;
				m_LocalPosition = m_Parent.InverseTransformPoint(m_Child.position);
			}

			m_OnMoved?.Invoke(deltaPosition);
		}

		private void SetControllerAcitve(bool pEnabled)
		{
			// if (childRigidbody)
			// {
			// 	childRigidbody.isKinematic = !pEnabled;
			// }
			if (m_ChildController)
			{
				m_ChildController.enabled = pEnabled;
			}
		}

		public void OnDestroy()
		{
			if (m_Child != null)
			{
				Stop();
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
