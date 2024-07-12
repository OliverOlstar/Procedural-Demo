using Sirenix.OdinInspector;
using UnityEngine;
using ODev.Util;

namespace ODev.Camera
{
	public class EagleEyeCamera : MonoBehaviour
    {
		[SerializeField]
		private InputBridge_EagleEye m_Input = null;
		[SerializeField, DisableInPlayMode]
		private Util.Mono.Updateable m_Updateable = new(Util.Mono.Type.Late, Util.Mono.Priorities.Camera);

		[Header("Follow")]
		public Transform CameraTransform = null; // Should be child
		[SerializeField]
		private Vector3 m_ChildOffset = new(0.0f, 2.0f, -5.0f);

		[Header("Move")]
		[SerializeField]
		private float m_MoveSpeed = 1.0f;
		[SerializeField]
		private float m_MoveDeltaSpeed = 1.0f;

		[Header("Look")]
		[SerializeField]
		private Transform m_LookTransform = null;
		[SerializeField]
		private float m_RotateSpeed = 1.0f;

		[Header("Zoom")]
		[SerializeField]
		private float m_ZoomSpeed = 1.0f;
		[SerializeField]
		private Vector2 m_ZoomDistanceClamp = new(1.0f, 5.0f);

		[Header("Collision")]
		[SerializeField]
		private LayerMask m_CollisionLayers = new();
		[SerializeField]
		private float m_CollisionRadius = 0.2f;
		[SerializeField]
		private float m_CollisionZoomSpacing = 1.0f;

		private Vector3 m_TargetPosition;
		private float currZoom = 0.5f;

		private float RotateInput => m_Input.Rotate.Input;

		private void Reset()
		{
			m_LookTransform = transform;
			if (transform.childCount > 0)
			{
				CameraTransform = transform.GetChild(0);
			}
		}

		private void Start()
		{
			m_TargetPosition = m_LookTransform.position;
			currZoom = m_ChildOffset.magnitude;
			CameraTransform.localPosition = m_ChildOffset;
			CameraTransform.LookAt(transform.position);

			if (m_Input != null)
			{
				m_Input.MoveDelta.Value.OnChanged.AddListener(OnMoveDelta);
				m_Input.Zoom.onChanged.AddListener(OnZoom);
			}

			m_Updateable.Register(Tick);
		}

		private void OnDestroy()
		{
			m_Updateable.Deregister();
		}

		private void Tick(float pDeltaTime)
		{
			Move(m_MoveSpeed * pDeltaTime * m_Input.Move.Input);
			DoMoveUpdate(pDeltaTime);
			DoZoomUpdate(pDeltaTime);
			RotateCamera(RotateInput * m_RotateSpeed * pDeltaTime);
			DoCollision();
		}

		private void DoMoveUpdate(in float pDeltaTime)
		{
			m_LookTransform.position = Vector3.Lerp(m_LookTransform.position, m_TargetPosition, pDeltaTime * 10.0f);
		}

		private void Move(Vector2 pInput)
		{
			if (pInput.sqrMagnitude > Math.NEARZERO)
			{
				m_TargetPosition += pInput.x * Math.Horizontalize(CameraTransform.right);
				m_TargetPosition += pInput.y * Math.Horizontalize(CameraTransform.forward);
			}
		}

		private void DoZoomUpdate(in float pDeltaTime)
		{
			CameraTransform.localPosition = Vector3.Lerp(CameraTransform.localPosition, m_ChildOffset.normalized * currZoom, pDeltaTime * 13.0f);
		}

		private void RotateCamera(float pInput)
		{
			if (m_LookTransform == null || pInput == 0.0f)
			{
				return;
			}
			Vector3 euler = m_LookTransform.eulerAngles;
			euler.y -= pInput;
			m_LookTransform.rotation = Quaternion.Euler(euler);
		}

		private void DoCollision()
		{
			Vector3 direction = m_LookTransform.TransformDirection(m_ChildOffset.normalized);
			if (Physics.Linecast(m_LookTransform.position + (direction * m_ZoomDistanceClamp.y), CameraTransform.position - (direction * m_CollisionRadius), out RaycastHit hit, m_CollisionLayers))
			{
				float magnitude = (m_ZoomDistanceClamp.y - hit.distance) + m_CollisionRadius;
				CameraTransform.localPosition = m_ChildOffset.normalized * magnitude;
				currZoom = magnitude + m_CollisionZoomSpacing;
			}
		}

		#region Input
		public void OnMoveDelta(Vector2 pInput)
		{
			Move((1 + currZoom - m_ZoomDistanceClamp.x) * m_MoveDeltaSpeed * pInput);
		}

		public void OnZoom(float pInput)
		{
			currZoom += pInput * m_ZoomSpeed;
			currZoom = Mathf.Clamp(currZoom, m_ZoomDistanceClamp.x, m_ZoomDistanceClamp.y);
		}
		#endregion

		private void OnDrawGizmosSelected()
		{
			if (Application.isPlaying || CameraTransform == null)
			{
				return;
			}
			CameraTransform.localPosition = m_ChildOffset;
			CameraTransform.LookAt(transform.position);
		}

		private void OnDrawGizmos()
		{
			if (CameraTransform == null)
			{
				return;
			}
			Gizmos.color = Color.green;
			Gizmos.DrawLine(m_LookTransform.position, m_LookTransform.position + m_LookTransform.TransformDirection(m_ChildOffset) * (CameraTransform.localPosition.magnitude + m_CollisionRadius));
		}
	}
}
