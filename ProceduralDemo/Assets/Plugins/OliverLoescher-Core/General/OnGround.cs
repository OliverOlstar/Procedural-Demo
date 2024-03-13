using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

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

			public void OnDrawGizmos(Transform pTransform, LayerMask pLayerMask)
			{
				Vector3 start = pTransform.TransformPoint(startPosition);
				Vector3 end = start - (pTransform.up * distance);
				Gizmos.color = Check(pTransform, pLayerMask) ? Color.green : Color.red;
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

			public void OnDrawGizmos(Transform pTransform, LayerMask pLayerMask)
			{
				Vector3 start = pTransform.TransformPoint(startPosition);
				Vector3 end = start - (pTransform.up * distance);
				Gizmos.DrawWireSphere(start, radius);
				Gizmos.color = Check(pTransform, pLayerMask) ? Color.green : Color.red;
				Gizmos.DrawWireSphere(end, radius);
				Gizmos.DrawLine(start, end);
			}
		}

		[SerializeField]
		private Util.Mono.Updateable updateable = new Util.Mono.Updateable(Util.Mono.Type.Fixed, Util.Mono.Priorities.OnGround);
		[SerializeField]
		private Linecast[] lines = new Linecast[0];
		[SerializeField]
		private Spherecast[] spheres = new Spherecast[1];
		[SerializeField]
		private LayerMask layerMask = new LayerMask();
		[SerializeField]
		private bool followGround;

		public bool IsGrounded { get; private set; }
		[FoldoutGroup("Events")]
		public UnityEvent OnEnterEvent;
		[FoldoutGroup("Events")]
		public UnityEvent OnExitEvent;

		// private Transform groundFollowTransform = null;
		// private Vector3 groudFollowPosition = Vector3.zero;
		// private Quaternion groudFollowRotation = Quaternion.identity;

		private void Start()
		{
			// groundFollowTransform = new GameObject($"{gameObject.name}-GroundFollower").transform;

			updateable.Register(Tick);
		}
		
		private void OnDestroy()
		{
			updateable.Deregister();
		}

		private void Tick(float pDeltaTime)
		{
			if (CheckIsGrounded() != IsGrounded)
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
		}

		private bool CheckIsGrounded()
		{
			foreach (Linecast line in lines)
			{
				if (line.Check(transform, layerMask))
					return true;
			}
			foreach (Spherecast sphere in spheres)
			{
				if (sphere.Check(transform, layerMask))
					return true;
			}
			return false;
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
			return total / (lines.Length + spheres.Length);
		}

		private void OnEnter()
		{
			OnEnterEvent?.Invoke();
			if (followGround)
			{
				Util.Debug.DevException("followGround == true... NotImplemented", "OnEnter", this);
			}
		}

		private void OnExit()
		{
			OnExitEvent?.Invoke();
			if (followGround)
			{
				Util.Debug.DevException("followGround == true... NotImplemented", "OnEnter", this);
			}
		}

		private void OnDrawGizmosSelected()
		{
			foreach (Linecast line in lines)
			{
				line.OnDrawGizmos(transform, layerMask);
			}
			foreach (Spherecast sphere in spheres)
			{
				sphere.OnDrawGizmos(transform, layerMask);
			}
		}
	}
}