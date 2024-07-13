using System.Collections;
using ODev;
using ODev.Util;
using UnityEngine;

public class CharacterMovement : MonoBehaviour, TransformFollower.IMotionReciver
{
	[SerializeField]
	private ODev.Util.Mono.Updateable m_Updateable = new(ODev.Util.Mono.Type.Early, ODev.Util.Mono.Priorities.CharacterController);
	[SerializeField]
	private CharacterController m_Controller = null;
	[SerializeField]
	private OnGround m_Grounded = null;
	[SerializeField]
	private InputBridge_PlayerCharacter m_Input = null;

	[Space]
	[SerializeField]
	private AnimationCurve m_JumpVelocityCurve = new();
	[SerializeField]
	private float m_JumpVelocitySalar = 5.0f;
	[SerializeField]
	private float m_JumpVelocitySeconds = 2.0f;
	[SerializeField]
	private AnimationCurve m_FallVelocityCurve = new();
	[SerializeField]
	private float m_FallVelocitySalar = -5.0f;
	[SerializeField]
	private float m_FallVelocitySeconds = 2.0f;

	[Space, SerializeField]
	private float m_Speed = 5.0f;

	[Space, SerializeField]
	private Vector3 m_AccelerationMovement;
	private Vector3 m_AccelerationGravity;
	private Vector3 Acceleration => m_AccelerationMovement + m_AccelerationGravity;

	Transform TransformFollower.IMotionReciver.Transform => transform;

	private Coroutine m_VelocityYRoutine = null;

	private void Reset()
	{
		gameObject.GetOrAddComponent(out m_Controller);
		gameObject.GetOrAddComponent(out m_Grounded);
	}

	private void Start()
	{
		m_Updateable.Register(Tick);
		m_Grounded.OnEnterEvent.AddListener(OnGroundEnter);
		m_Grounded.OnExitEvent.AddListener(OnGroundExit);
	}

	private void OnDestroy()
	{
		m_Updateable.Deregister();
	}

	void Tick(float pDeltaTime)
	{
		JumpTick();
		MoveTick(pDeltaTime);

		// Vector3 center = m_Controller.transform.position + m_Controller.center;
		// float distance = m_Velocity.magnitude * pDeltaTime;
		// if (Physics.CapsuleCast(center + (Vector3.up * m_Controller.height), center + (Vector3.down * m_Controller.height), m_Controller.radius, m_Velocity, out RaycastHit hit, distance, m_GroundLayer))
		// {
		// 	float y = m_Velocity.y;
		// 	m_Velocity *= hit.distance / distance;
		// 	m_Velocity.y = y;
		// }
		m_Controller.Move((Acceleration * pDeltaTime) + m_RecivedMovement);
		m_RecivedMovement = Vector3.zero;
	}

	private void MoveTick(float pDeltaTime)
	{
		if (!m_Grounded.IsGrounded)
		{
			m_AccelerationMovement -= 0.99f * pDeltaTime * m_AccelerationMovement;
			return;
		}
		Vector3 normal = m_Grounded.GetAverageNormal();
		Vector3 input = m_Input.Move.Input.y * MainCamera.Camera.transform.forward.ProjectOnPlane(normal);
		input += m_Input.Move.Input.x * MainCamera.Camera.transform.right.ProjectOnPlane(normal);
		input = input.normalized;

		m_AccelerationMovement = input * m_Speed;
	}

	private void JumpTick()
	{
		if (!m_Grounded.IsGrounded)
		{
			return;
		}
		if (Input.GetKey(KeyCode.Space))
		{
			if (m_VelocityYRoutine != null)
			{
				StopCoroutine(m_VelocityYRoutine);
			}
			m_VelocityYRoutine = StartCoroutine(VelocityYCoroutine(m_JumpVelocityCurve, m_JumpVelocitySalar, m_JumpVelocitySeconds,
			() =>
			{
				m_VelocityYRoutine = StartCoroutine(VelocityYCoroutine(m_FallVelocityCurve, m_FallVelocitySalar, m_FallVelocitySeconds, null));
			}));
		}
	}

	private void OnGroundEnter()
	{
		if (m_VelocityYRoutine != null)
		{
			StopCoroutine(m_VelocityYRoutine);
			m_VelocityYRoutine = null;
		}
		m_AccelerationGravity.y = -1.0f;
	}

	private void OnGroundExit()
	{
		m_VelocityYRoutine ??= StartCoroutine(VelocityYCoroutine(m_FallVelocityCurve, m_FallVelocitySalar, m_FallVelocitySeconds, null));
	}

	private IEnumerator VelocityYCoroutine(AnimationCurve pCurve, float pScalar, float pSeconds, System.Action pOnComplete)
	{
		float progress01 = 0.0f;
		float timeScale = 1 / pSeconds;

		m_AccelerationGravity.y = pCurve.Evaluate(0) * pScalar;
		while (progress01 < 1.0f)
		{
			yield return null;
			m_AccelerationGravity.y = pCurve.Evaluate(progress01) * pScalar;
			progress01 += Time.deltaTime * timeScale;
		}
		pOnComplete?.Invoke();
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Vector3 start = transform.position + m_Controller.center;
		Gizmos.DrawLine(start, start + m_AccelerationGravity);
	}

	private Vector3 m_RecivedMovement = Vector3.zero;
	void TransformFollower.IMotionReciver.AddMovement(Vector3 pMovement, Quaternion pRotation)
	{
		m_RecivedMovement += pMovement;
		transform.rotation *= pRotation;
	}
}
