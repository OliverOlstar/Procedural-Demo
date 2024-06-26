using UnityEngine;

namespace OCore.Camera
{
	public class FirstPersonHandsInertia : MonoBehaviour
	{
		[Header("Tilt")]
		[SerializeField]
		private Vector2 m_TiltMagnitude = Vector2.one;
		[SerializeField, Min(0)]
		private Vector2 m_TiltMax = new(8.0f, 6.0f);
		[SerializeField, Min(0)]
		private Vector2 m_TiltDampening = Vector2.one;

		[Header("Movement")]
		[SerializeField]
		private Rigidbody m_Rigidbody = null;
		[SerializeField, Range(0, 0.01f)]
		private float m_TargetMagnitude = 5.0f;
		[SerializeField, Min(0)]
		private float m_MaxMagnitude = 1.0f;
		[SerializeField]
		private float m_MoveDampening = 1.0f;
		[SerializeField]
		private Vector3 m_MoveRelOffset = Vector3.zero;

		private Vector3 m_InitalRelOffset;
		private float m_MoveValue = 0.0f;

		[Space] // Bounce
		[SerializeField]
		private OnGround m_Grounded = null;
		[SerializeField]
		private AnimationCurve m_BounceCurve = new(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));
		[SerializeField]
		private float m_BounceFrequncy = 0.1f;

		private Vector3 m_LastRotation;
		private float m_BounceProgress = 0.0f;
		private bool m_DoBounce = true;

		private void Start()
		{
			m_LastRotation = transform.parent.eulerAngles;
			m_InitalRelOffset = transform.localPosition;

			if (m_Grounded == null)
			{
				return;
			}
			m_Grounded.OnEnterEvent.AddListener(delegate { m_DoBounce = true; });
			m_Grounded.OnExitEvent.AddListener(delegate { m_DoBounce = false; });
		}

		private void LateUpdate()
		{
			Vector3 motion = transform.parent.eulerAngles - m_LastRotation;
			m_LastRotation = transform.parent.eulerAngles;

			Vector3 rot = transform.localEulerAngles;
			rot.y = Calculate(Util.Func.SafeAngle(rot.y), motion.y * m_TiltMagnitude.x, m_TiltMax.x, m_TiltDampening.x);
			rot.x = Calculate(Util.Func.SafeAngle(rot.x), motion.x * m_TiltMagnitude.y, m_TiltMax.y, m_TiltDampening.y);
			transform.localRotation = Quaternion.Euler(rot);

			float v = Mathf.Min(m_MaxMagnitude, m_Rigidbody.velocity.sqrMagnitude) * m_TargetMagnitude;

			Vector3 bounceOffset = Vector3.zero;
			if (m_DoBounce)
			{
				m_BounceProgress += Time.deltaTime * m_BounceFrequncy * v;
				bounceOffset = Vector3.up * m_BounceCurve.Evaluate(m_BounceProgress);
			}

			m_MoveValue = Mathf.Lerp(m_MoveValue, Mathf.Clamp01(v), Time.deltaTime * m_MoveDampening);
			transform.localPosition = Vector3.Lerp(m_InitalRelOffset, m_MoveRelOffset + bounceOffset, m_MoveValue);

		}

		private float Calculate(float pValue, float pTarget, float pMax, float pDampening)
		{
			pTarget = Mathf.Clamp(pTarget, -pMax, pMax);
			pValue = Mathf.Lerp(pValue, pTarget, Time.deltaTime * m_TiltDampening.x);
			return pValue;
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(transform.position, transform.parent.position + m_MoveRelOffset);
			Gizmos.DrawCube(transform.parent.position + m_MoveRelOffset, Vector3.one * 0.1f);
		}
	}
}