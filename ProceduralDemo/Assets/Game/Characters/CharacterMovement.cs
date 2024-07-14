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

	[Space, SerializeField]
	private float m_Speed = 5.0f;
	[SerializeField]
	private float m_AirSpeed = 2.0f;

	[Space, SerializeField]
	private Vector3 m_AccelerationMovement;
	private Vector3 m_AccelerationGravity;
	private Vector3 Acceleration => m_AccelerationMovement + m_AccelerationGravity;

	Transform TransformFollower.IMotionReciver.Transform => transform;

	private void Reset()
	{
		gameObject.GetOrAddComponent(out m_Controller);
		gameObject.GetOrAddComponent(out m_Grounded);
	}

	private void Start()
	{
		m_Updateable.Register(Tick);
	}

	private void OnDestroy()
	{
		m_Updateable.Deregister();
	}

	void Tick(float pDeltaTime)
	{
		m_AccelerationGravity.y -= 9.81f * pDeltaTime;
		m_AccelerationGravity.y = Mathf.Max(m_AccelerationGravity.y, m_Grounded.IsGrounded ? -0.5f : -9.81f);

		MoveTick(pDeltaTime);
		
		m_Controller.Move((Acceleration * pDeltaTime) + m_RecivedMovement);
		m_RecivedMovement = Vector3.zero;
	}

	private void MoveTick(float pDeltaTime)
	{
		Vector3 normal = m_Grounded.GetAverageNormal();
		Vector3 input = m_Input.Move.Input.y * MainCamera.Camera.transform.forward.ProjectOnPlane(normal);
		input += m_Input.Move.Input.x * MainCamera.Camera.transform.right.ProjectOnPlane(normal);
		input = input.normalized;

		if (!m_Grounded.IsGrounded)
		{
			m_AccelerationMovement -= 0.99f * pDeltaTime * m_AccelerationMovement;
			m_AccelerationMovement += m_AirSpeed * pDeltaTime * input;
			return;
		}
		m_AccelerationMovement = input * m_Speed;
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

	public void AddVelocity(Vector3 pVelocity)
	{
		m_AccelerationGravity += pVelocity;
	}
	public void SetVelocity(Vector3 pVelocity)
	{
		m_AccelerationGravity = pVelocity;
	}
}
