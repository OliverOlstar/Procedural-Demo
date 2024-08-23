using ODev;
using ODev.Util;
using ODev.GameStats;
using UnityEngine;

public class CharacterMovement : MonoBehaviour, TransformFollower.IMotionReciver
{
	[SerializeField]
	private Mono.Updateable m_Updateable = new(Mono.Type.Early, Mono.Priorities.CharacterController);
	[SerializeField]
	private CharacterController m_Controller = null;
	[SerializeField]
	private PlayerRoot m_Root = null;

	[Header("Stats")]
	public bool MovementEnabled = true;
	// public FloatGameStat Speed = new(10.0f);
	public FloatGameStat Acceleration = new(20.0f);
	public FloatGameStat Drag = new(1.0f);
	public FloatGameStat MaxVelocity = new(10.0f);

	[Space, SerializeField]
	private float m_SlopeAcceleration = 2.0f;
	[SerializeField]
	private float m_SlopeDrag = 1.0f;
	[SerializeField]
	private float m_SlopeMaxVelocity = 5.0f;

	[Space]
	public FloatGameStat AirAcceleration = new(20.0f);
	public FloatGameStat AirDrag = new(1.0f);
	public FloatGameStat AirMaxVelocity = new(10.0f);

	[Space]
	public bool GravityEnabled = true;
	public FloatGameStat UpGravity = new(-19.62f);
	public FloatGameStat DownGravity = new(-19.62f);
	public FloatGameStat TerminalGravity = new(-30.0f);

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

		m_Root.OnGround.OnGroundExitEvent.AddListener(OnGroundExit);
	}

	private void OnDestroy()
	{
		m_Root.OnGround.OnGroundExitEvent.RemoveListener(OnGroundExit);
	}

	private void OnGroundExit()
	{
		if (m_VelocityY.Approximately(-1.0f))
		{
			m_VelocityY = 1.0f;
		}
	}

	private void OnEnable()
	{
		m_Updateable.Register(Tick);
		m_Controller.enabled = true;
	}

	private void OnDisable()
	{
		m_Updateable.Deregister();
		m_Controller.enabled = false;
		m_VelocityXZ = Vector3.zero;
		m_VelocityY = 0.0f;
	}

	void Tick(float pDeltaTime)
	{
		GravityTick(pDeltaTime, out Vector3 gravityDown);
		MoveTick(pDeltaTime);

		Vector3 Velocity = m_VelocityXZ + (m_VelocityY * gravityDown);
		m_Controller.Move((Velocity * pDeltaTime) + Math.Horizontal(m_RecievedDisplacement));

		m_Controller.enabled = false;
		transform.position += m_RecievedDisplacement.y * Vector3.up;
		m_Controller.enabled = true;

		m_RecievedDisplacement = Vector3.zero;
	}

	private void GravityTick(float pDeltaTime, out Vector3 oGravityDirection)
	{
		m_VelocityY += (m_VelocityY > 0.0f ? UpGravity.Value : DownGravity.Value) * pDeltaTime;
		float maxVelocityY = GravityEnabled ? (m_Root.OnGround.IsOnGround ? -1.0f : TerminalGravity.Value) : 0.0f;
		m_VelocityY = Mathf.Max(m_VelocityY, maxVelocityY);

		oGravityDirection = Vector3.up;
		if (m_VelocityY < 0.0f && m_Root.OnGround.IsOnSlope)
		{
			oGravityDirection = Vector3.ProjectOnPlane(Vector3.up, m_Root.OnGround.GetAverageNormal()).normalized;
		}
	}

	private void MoveTick(float pDeltaTime)
	{
		Vector3 normal = m_Root.OnGround.IsOnGround ? m_Root.OnGround.GetAverageNormal() : Vector3.up;
		Vector3 input = Vector3.zero;
		if (MovementEnabled)
		{
			input = m_Root.Input.Move.Input.y * MainCamera.Camera.transform.forward.ProjectOnPlane(normal);
			input += m_Root.Input.Move.Input.x * MainCamera.Camera.transform.right.ProjectOnPlane(normal);
			input.Normalize();
		}

		GetStats(out float acceleration, out float drag, out float maxVelocity);
		m_VelocityXZ -= drag * pDeltaTime * m_VelocityXZ; // Drag
		if (maxVelocity > 0.0f)
		{
			maxVelocity = Mathf.Max(maxVelocity, m_VelocityXZ.magnitude);
		}
		m_VelocityXZ += acceleration * pDeltaTime * input; // Acceleration
		m_VelocityXZ = Vector3.ClampMagnitude(m_VelocityXZ, maxVelocity);
		return;
	}

	private void GetStats(out float oAcceleration, out float oDrag, out float oMaxVelocity)
	{
		if (m_Root.OnGround.IsOnGround)
		{
			oAcceleration = Acceleration.Value;
			oDrag = Drag.Value;
			oMaxVelocity = MaxVelocity.Value;
			return;
		}
		else if (m_Root.OnGround.IsInAir)
		{
			oAcceleration = AirAcceleration.Value;
			oDrag = AirDrag.Value;
			oMaxVelocity = AirMaxVelocity.Value;
			return;
		}
		oAcceleration = m_SlopeAcceleration;
		oDrag = m_SlopeDrag;
		oMaxVelocity = m_SlopeMaxVelocity;
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
