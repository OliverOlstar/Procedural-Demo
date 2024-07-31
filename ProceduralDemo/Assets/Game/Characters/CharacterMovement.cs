using System.Collections;
using ODev;
using ODev.Util;
using ODev.GameStats;
using UnityEngine;
using UnityEngine.Events;

public class CharacterMovement : MonoBehaviour, TransformFollower.IMotionReciver
{
	public readonly UnityEvent<float> PreCharacterMove = new();
	public readonly UnityEvent<float> PostCharacterMove = new();

	[SerializeField]
	private ODev.Util.Mono.Updateable m_Updateable = new(ODev.Util.Mono.Type.Early, ODev.Util.Mono.Priorities.CharacterController);
	[SerializeField]
	private CharacterController m_Controller = null;
	[SerializeField]
	private PlayerRoot m_Player = null;

	[Header("Stats")]
	public FloatGameStat Speed = new(10.0f);

	[Space, SerializeField]
	private float m_SlopeAcceleration = 2.0f;
	[SerializeField]
	private float m_SlopeDrag = 1.0f;
	[SerializeField]
	private float m_SlopeMaxVelocity = 5.0f;

	[Space, SerializeField]
	private float m_AirAcceleration = 2.0f;
	[SerializeField]
	private float m_AirDrag = 1.0f;
	[SerializeField]
	private float m_AirMaxVelocity = 20.0f;

	[Space, SerializeField]
	private float m_Gravity = -9.81f;
	[SerializeField]
	private float m_TerminalGravity = -9.81f;

	private Vector3 m_RecievedDisplacement = Vector3.zero;
	private Vector3 m_VelocityXZ;
	private float m_VelocityY;

	public Vector3 VelocityXZ => m_VelocityXZ;
	public float VelocityY => m_VelocityY;
	public Vector3 Velocity => new(m_VelocityXZ.x, m_VelocityXZ.y + m_VelocityY, m_VelocityXZ.z);
	Transform TransformFollower.IMotionReciver.Transform => transform;

	private void Reset()
	{
		gameObject.GetOrAddComponent(out m_Controller);
	}

	private void Start()
	{
		m_Controller.enableOverlapRecovery = true;
		m_Controller.providesContacts = false;
	}

	private void OnEnable()
	{
		m_Updateable.Register(Tick);
	}

	private void OnDisable()
	{
		m_Updateable.Deregister();
	}

	void Tick(float pDeltaTime)
	{
		PreCharacterMove.Invoke(pDeltaTime);

		GravityTick(pDeltaTime, out Vector3 gravityDown);
		MoveTick(pDeltaTime);

		Vector3 Velocity = m_VelocityXZ + (m_VelocityY * gravityDown);
		m_Controller.Move((Velocity * pDeltaTime) + Math.Horizontal(m_RecievedDisplacement));

		m_Controller.enabled = false;
		transform.position += m_RecievedDisplacement.y * Vector3.up;
		m_Controller.enabled = true;

		m_RecievedDisplacement = Vector3.zero;

		PostCharacterMove.Invoke(pDeltaTime);
	}

	private void GravityTick(float pDeltaTime, out Vector3 oGravityDown)
	{
		m_VelocityY += m_Gravity * pDeltaTime;
		m_VelocityY = Mathf.Max(m_VelocityY, m_Player.OnGround.IsOnGround ? -1.0f : m_TerminalGravity);

		oGravityDown = Vector3.up;
		if (m_VelocityY < 0.0f && m_Player.OnGround.IsOnSlope)
		{
			oGravityDown = Vector3.ProjectOnPlane(Vector3.up, m_Player.OnGround.GetAverageNormal()).normalized;
		}
	}

	private void MoveTick(float pDeltaTime)
	{
		Vector3 normal = m_Player.OnGround.IsOnGround ? m_Player.OnGround.GetAverageNormal() : Vector3.up;
		Vector3 input = m_Player.Input.Move.Input.y * MainCamera.Camera.transform.forward.ProjectOnPlane(normal);
		input += m_Player.Input.Move.Input.x * MainCamera.Camera.transform.right.ProjectOnPlane(normal);
		input.Normalize();

		if (!m_Player.OnGround.IsOnGround /*|| m_VelocityY > Math.NEARZERO*/)
		{
			float drag = m_Player.OnGround.IsInAir ? m_AirDrag : m_SlopeDrag;
			float acceleration = m_Player.OnGround.IsInAir ? m_AirAcceleration : m_SlopeAcceleration;
			float maxVelocity = m_Player.OnGround.IsInAir ? m_AirMaxVelocity : m_SlopeMaxVelocity;

			if (maxVelocity > 0.0f)
			{
				maxVelocity = Mathf.Max(maxVelocity, m_VelocityXZ.magnitude);
			}
			m_VelocityXZ -= drag * pDeltaTime * m_VelocityXZ; // Drag
			m_VelocityXZ += acceleration * pDeltaTime * input; // Acceleration
			m_VelocityXZ = Vector3.ClampMagnitude(m_VelocityXZ, maxVelocity);
			return;
		}
		m_VelocityXZ = input * Speed.Value;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Vector3 start = transform.position + m_Controller.center;
		Gizmos.DrawLine(start, start + (Vector3.down * m_VelocityY));
	}

	public void AddVelocity(Vector3 pVelocity)
	{
		m_VelocityXZ.x += pVelocity.x;
		m_VelocityY += pVelocity.y;
		m_VelocityXZ.z += pVelocity.z;
	}
	public void AddVelocityXZ(Vector3 pVelocity)
	{
		m_VelocityXZ += pVelocity;
	}
	public void SetVelocityY(float pVelocity)
	{
		m_VelocityY = pVelocity;
	}
	public void SetVelocityXZ(Vector3 pVelocity)
	{
		m_VelocityXZ = pVelocity;
	}

	public void AddDisplacement(Vector3 pMovement, Quaternion pRotation) // IMotionReciver
	{
		m_RecievedDisplacement += pMovement;
		transform.rotation *= pRotation;
	}
}
