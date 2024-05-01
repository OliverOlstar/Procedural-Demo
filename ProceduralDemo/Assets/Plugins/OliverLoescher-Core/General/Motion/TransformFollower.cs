using System.Collections;
using System.Collections.Generic;
using OliverLoescher.Util;
using UnityEngine;

namespace OliverLoescher
{
	/// <summary>
	/// A solution to follow transform's movement with an offset without childing your transform to it
	/// </summary>
	public class TransformFollower
	{
		private Util.Mono.Updateable updateable = new();
		private Transform parent;
		private Transform child;
		private CharacterController childController;
		private Rigidbody childRigidbody;
		private Object debugObject;
		private System.Action<Vector3> onMoved;
		private bool rotateChild;

		private Vector3 lastPosition = Vector3.zero;
		private Quaternion lastRotation = Quaternion.identity;
		private Vector3 localPosition;

		public bool IsStarted => updateable.IsRegistered;
		public Transform ParentTransform => parent;
		public Transform ChildTransform => child;

		public void Start(Transform pParent, Rigidbody pChild, Vector3 pPoint, System.Action<Vector3> pOnMoved, bool pRotateChild, Util.Mono.Type pUpdateType, Util.Mono.Priorities pUpdatePriority, Object pDebugParent)
		{
			debugObject = pDebugParent;
			if (pChild == null)
			{
				Util.Debug2.DevException("pChild (Rigidbody) is null", "Start", debugObject);
				return;
			}
			childRigidbody = pChild;
			Start(pParent, pChild.transform, pPoint, pOnMoved, pRotateChild, pUpdateType, pUpdatePriority, pDebugParent);
		}

		public void Start(Transform pParent, CharacterController pChild, Vector3 pPoint, System.Action<Vector3> pOnMoved, bool pRotateChild, Util.Mono.Type pUpdateType, Util.Mono.Priorities pUpdatePriority, Object pDebugParent)
		{
			debugObject = pDebugParent;
			if (pChild == null)
			{
				Util.Debug2.DevException("pChild (CharacterController) is null", "Start", debugObject);
				return;
			}
			childController = pChild;
			Start(pParent, pChild.transform, pPoint, pOnMoved, pRotateChild, pUpdateType, pUpdatePriority, pDebugParent);
		}

		public void Start(Transform pParent, Transform pChild, Vector3 pPoint, System.Action<Vector3> pOnMoved, bool pRotateChild, Util.Mono.Type pUpdateType, Util.Mono.Priorities pUpdatePriority, Object pDebugParent)
		{
			debugObject = pDebugParent;
			if (pChild == null)
			{
				Util.Debug2.DevException("pChild is null", "Start", debugObject);
				return;
			}
			if (pParent == null)
			{
				Util.Debug2.DevException("pParent is null", "Start", debugObject);
				return;
			}
			if (child != null) // Already started
			{
				if (child != pChild)
				{
					Util.Debug2.DevException("Already started but with a different child transform", "Start", debugObject);
				}
				else if (pParent != parent)
				{
					Util.Debug2.DevException("Already started but with a different parent transform", "Start", debugObject);
				}
				else
				{
					Util.Debug2.LogError("Already started", "Start", debugObject);
				}
				return;
			}
			parent = pParent;
			child = pChild;
			onMoved = pOnMoved;
			rotateChild = pRotateChild;

			lastPosition = pPoint;
			lastRotation = parent.rotation;
			localPosition = parent.InverseTransformPoint(pPoint);

			updateable.SetProperties(pUpdateType, pUpdatePriority);
			updateable.Register(Tick);
		}

		public void Stop()
		{
			if (child == null)
			{
				Util.Debug2.LogWarning("Not started, please start first", "Stop", debugObject);
				return;
			}
			parent = null;
			child = null;

			updateable.Deregister();
		}

		public void ChangeParent(Transform pParent)
		{
			if (pParent == null)
			{
				Util.Debug2.DevException("pParent is null", "ChangePoint", debugObject);
				return;
			}
			parent = pParent;
			
			lastRotation = parent.rotation;
			localPosition = parent.InverseTransformPoint(lastPosition);
		}

		private void Tick(float pDeltaTime)
		{
			Vector3 currPositon = parent.TransformPoint(localPosition);
			Vector3 deltaPosition = currPositon - lastPosition;
			if (deltaPosition.IsNearZero())
			{
				return;
			}

			SetControllerAcitve(false); // Disable so we can move the transform
			child.position += deltaPosition;
			if (childRigidbody != null)
			{
				childRigidbody.position += deltaPosition;
			}
			if (rotateChild)
			{
				Quaternion deltaRotation = lastRotation.Difference(parent.rotation);
				deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
				child.RotateAround(currPositon, axis, -angle);
				lastRotation = parent.rotation;
			}
			SetControllerAcitve(true);
			lastPosition = currPositon;

			if (currPositon != child.position)
			{
				lastPosition += child.position - currPositon;
				localPosition = parent.InverseTransformPoint(child.position);
			}

			onMoved?.Invoke(deltaPosition);
		}

		private void SetControllerAcitve(bool pEnabled)
		{
			// if (childRigidbody)
			// {
			// 	childRigidbody.isKinematic = !pEnabled;
			// }
			if (childController)
			{
				childController.enabled = pEnabled;
			}
		}

		public void OnDestroy()
		{
			if (child != null)
			{
				Stop();
			}
		}

		public void OnDrawGizmos()
		{
			if (child == null)
			{
				return;
			}
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(parent.TransformPoint(localPosition), 0.5f);
			Gizmos.DrawSphere(parent.TransformPoint(localPosition), 0.25f);
		}
	}
}
