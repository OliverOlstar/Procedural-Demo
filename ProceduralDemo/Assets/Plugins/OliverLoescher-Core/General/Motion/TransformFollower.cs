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
		private Util.Mono.Updateable updateable = new Util.Mono.Updateable();
		private Transform parent;
		private Transform child;
		private Object debugObject;
		private System.Action onMoved;

		private Vector3 lastPosition = Vector3.zero;
		private Quaternion lastRotation = Quaternion.identity;
		private Vector3 localPosition;

		public void Start(Transform pParent, Transform pChild, Vector3 pPoint, System.Action pOnMoved, Util.Mono.Type pUpdateType, Util.Mono.Priorities pUpdatePriority, Object pDebugParent)
		{
			if (pChild == null)
			{
				Util.Debug.DevException("pChild is null", "Start", debugObject);
				return;
			}
			if (pParent == null)
			{
				Util.Debug.DevException("pParent is null", "Start", debugObject);
				return;
			}
			if (child != null) // Already started
			{
				if (child != pChild)
				{
					Util.Debug.DevException("Already started but with a different child transform", "Start", debugObject);
				}
				else if (pParent != parent)
				{
					Util.Debug.DevException("Already started but with a different parent transform", "Start", debugObject);
				}
				else
				{
					Util.Debug.LogError("Already started", "Start", debugObject);
				}
				return;
			}
			debugObject = pDebugParent;
			parent = pParent;
			child = pChild;
			onMoved = pOnMoved;

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
				Util.Debug.LogWarning("Not started, please start first", "Stop", debugObject);
				return;
			}
			parent = null;
			child = null;

			updateable.Deregister();
		}

		private void Tick(float _)
		{
			Vector3 currPositon = parent.TransformPoint(localPosition);
			child.position += currPositon - lastPosition;
			lastPosition = currPositon;

			Quaternion deltaRotation = lastRotation.Difference(parent.rotation);
			deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
			child.RotateAround(currPositon, axis, -angle);
			lastRotation = parent.rotation;

			onMoved?.Invoke();
		}

		public void OnDestroy()
		{
			Stop();
		}

		public void OnDrawGizmos()
		{
			if (parent == null)
			{
				return;
			}
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(parent.TransformPoint(localPosition), 0.5f);
			Gizmos.DrawSphere(parent.TransformPoint(localPosition), 0.25f);
		}
	}
}