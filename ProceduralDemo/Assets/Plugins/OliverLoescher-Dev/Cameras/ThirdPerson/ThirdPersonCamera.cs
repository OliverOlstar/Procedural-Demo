using UnityEngine;
using Sirenix.OdinInspector;
using ODev.Util;

namespace ODev
{
	public class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] 
		private InputBridge_Camera m_Input = null;
		[SerializeField]
		private Util.Mono.Updateable m_Updateable = new(Util.Mono.Type.Late, Util.Mono.Priorities.Camera);

		[Header("Follow")]
        public Transform FollowTransform = null;
        [SerializeField]
		private Vector3 m_Offset = new(0.0f, 0.5f, 0.0f);
        public Transform CameraTransform = null; // Should be child
        [SerializeField]
		private Vector3 m_ChildOffset = new(0.0f, 2.0f, -5.0f);
		[SerializeField]
		private float m_YSmoothTime = 0.2f;
		private float m_YVelocity = 0.0f;
        
        [Header("Look")]
        [SerializeField]
		private Transform m_LookTransform = null;
        [SerializeField, MinMaxSlider(-90, 90, true)] 
		private Vector2 m_LookYClamp = new(-40, 50);
		[SerializeField]
		private Vector2 m_LookDampening = Vector2.one;

		private Vector2 m_LookInput = new();

        [Header("Zoom")]
        [SerializeField]
		private float m_ZoomSpeed = 1.0f;
        [SerializeField]
		private float m_ZoomSmoothTime = 0.1f;
        [SerializeField]
		private Vector2 m_ZoomDistanceClamp = new(1.0f, 5.0f);
		private Vector3 m_ZoomVelocity = Vector3.zero;

        private float m_CurrZoom = 0.5f;

        [Header("Collision")]
        [SerializeField] 
		private LayerMask m_CollisionLayers = new();
        [SerializeField] 
		private float m_CollisionRadius = 0.2f;

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
            DoFollow();

            m_CurrZoom = m_ChildOffset.magnitude;
			m_TargetEuler = m_LookTransform.eulerAngles;
			CameraTransform.localPosition = m_ChildOffset;

            if (m_Input != null)
			{
				m_Input.Look.OnChanged.AddListener(OnLook);
				m_Input.LookDelta.OnChanged.AddListener(OnLookDelta);
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
            DoFollow();
            
            if (!m_LookInput.IsNearZero())
            {
				ApplyRotateInput(pDeltaTime * m_LookInput);
            }
			RotateCamera(pDeltaTime);

            DoZoomUpdate(pDeltaTime);
            DoCollision();
        }

        private void DoFollow()
		{
			if (FollowTransform == null)
			{
				return;
			}
			Vector3 position = FollowTransform.position + m_Offset;
			if (m_YSmoothTime > Math.NEARZERO)
			{
				position.y = Mathf.SmoothDamp(transform.position.y, position.y, ref m_YVelocity, m_YSmoothTime);
			}
			transform.position = position;
		}

		private Vector2 m_TargetEuler = new();
		private void RotateCamera(float pDeltaTime)
		{
			if (m_LookTransform == null)
			{
				return;
			}
			Vector3 euler = m_LookTransform.eulerAngles;
			euler.x = LookAngleDampening(euler.x, m_TargetEuler.x, m_LookDampening.x, pDeltaTime);
			euler.y = LookAngleDampening(euler.y, m_TargetEuler.y, m_LookDampening.y, pDeltaTime);
			euler.z = 0.0f;
			m_LookTransform.rotation = Quaternion.Euler(euler);
		}
		private float LookAngleDampening(float pValue, float pTarget, float pDampening, float pDeltaTime)
		{
			if (Math.IsNearZero(pDampening))
			{
				return pTarget;
			}
			return Mathf.LerpAngle(pValue, pTarget, pDampening * pDeltaTime);
		}

		private void ApplyRotateInput(Vector2 pInput)
		{
			m_TargetEuler.x = Mathf.Clamp(Func.SafeAngle(m_TargetEuler.x - pInput.y), m_LookYClamp.x, m_LookYClamp.y);
			m_TargetEuler.y += pInput.x;
		}

        private void DoZoom(float pInput)
        {
            m_CurrZoom += pInput * m_ZoomSpeed;
            m_CurrZoom.Clamp(m_ZoomDistanceClamp);
        }

        private void DoZoomUpdate(in float pDeltaTime)
        {
            CameraTransform.localPosition = Vector3.SmoothDamp(CameraTransform.localPosition, m_ChildOffset.normalized * m_CurrZoom, ref m_ZoomVelocity, 
				m_ZoomSmoothTime, float.PositiveInfinity, deltaTime: pDeltaTime);
        }

        private void DoCollision()
		{
			if (!Physics.Raycast(transform.position, transform.TransformDirection(m_ChildOffset.normalized), out RaycastHit hit, 
				CameraTransform.localPosition.magnitude + m_CollisionRadius, m_CollisionLayers))
			{
				return;
			}
			CameraTransform.localPosition = m_ChildOffset.normalized * (hit.distance - m_CollisionRadius);
		}

		#region Input
		public void OnLook(Vector2 pInput)
        {
            m_LookInput = pInput;
        }

        public void OnLookDelta(Vector2 pInput)
        {
			ApplyRotateInput(pInput);
        }

        public void OnZoom(float pInput)
        {
            DoZoom(pInput);
        }
#endregion

        private void OnDrawGizmosSelected()
		{
			if (Application.isPlaying || CameraTransform == null)
			{
				return;
			}
			DoFollow();
			CameraTransform.localPosition = m_ChildOffset;
		}

		private void OnDrawGizmos()
        {
            if (CameraTransform == null)
			{
				return;
			}
			Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + transform.TransformDirection(m_ChildOffset) * (CameraTransform.localPosition.magnitude + m_CollisionRadius));
        }
    }
}