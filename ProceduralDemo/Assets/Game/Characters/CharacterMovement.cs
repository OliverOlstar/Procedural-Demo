using System.Collections;
using System.Collections.Generic;
using OliverLoescher;
using OliverLoescher.Util;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
	[SerializeField]
	private OliverLoescher.Util.Mono.Updateable m_Updateable = new(OliverLoescher.Util.Mono.Type.Early, OliverLoescher.Util.Mono.Priorities.CharacterController);
	[SerializeField]
	private CharacterController m_Controller = null;
	[SerializeField]
	private OnGround m_Grounded = null;
	[SerializeField]
	private InputBridge_PlayerCharacter m_Input = null;

	[Header("Jump")]
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
	private Vector3 m_Velocity;

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
		MoveTick();
		m_Controller.Move(m_Velocity * pDeltaTime);
	}

	private void MoveTick()
	{
		if (!m_Grounded.IsGrounded)
		{
			return;
		}
		Vector3 input = m_Input.Move.Input.y * Math.Horizontalize(MainCamera.Camera.transform.forward);
		input += m_Input.Move.Input.x * Math.Horizontalize(MainCamera.Camera.transform.right);
		input.Horizontalize();

		m_Velocity.x = input.x * m_Speed;
		m_Velocity.z = input.z * m_Speed;
	}

	private void JumpTick()
	{
		if (!m_Grounded.IsGrounded)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (m_VelocityYRoutine != null)
			{
				StopCoroutine(m_VelocityYRoutine);
			}
			m_VelocityYRoutine = StartCoroutine(VelocityYCoroutine(m_JumpVelocityCurve, m_JumpVelocitySalar, m_JumpVelocitySeconds, () =>
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
		m_Velocity.y = -1.0f;
	}

	private void OnGroundExit()
	{
		m_VelocityYRoutine ??= StartCoroutine(VelocityYCoroutine(m_FallVelocityCurve, m_FallVelocitySalar, m_FallVelocitySeconds, null));
	}

	private IEnumerator VelocityYCoroutine(AnimationCurve pCurve, float pScalar, float pSeconds, System.Action pOnComplete)
	{
		float progress01 = 0.0f;
		float timeScale = 1 / pSeconds;

		m_Velocity.y = pCurve.Evaluate(0) * pScalar;
		while (progress01 < 1.0f)
		{
			yield return null;
			m_Velocity.y = pCurve.Evaluate(progress01) * pScalar;
			progress01 += Time.deltaTime * timeScale;
		}
		pOnComplete?.Invoke();
	}
}
