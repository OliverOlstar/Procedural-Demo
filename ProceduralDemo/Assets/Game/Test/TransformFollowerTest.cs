using System.Collections;
using System.Collections.Generic;
using ODev;
using ODev.Util;
using UnityEngine;

public class TransformFollowerTest : MonoBehaviour, TransformFollower.IMotionReciver
{
	[SerializeField]
	private Transform m_Target = null;
	[SerializeField]
	private CharacterController m_Controller = null;
	[SerializeField]
	private ODev.Util.Mono.Updateable m_Updateable = new();

	private readonly TransformFollower m_Follower = new();

	public Transform Transform => transform;

	private void Start()
	{
		m_Follower.Start(m_Target, this, transform.position, true, m_Updateable.Type, m_Updateable.Priority - 1, this);
		m_Updateable.Register(Tick);
	}

	private void OnDestroy()
	{
		m_Follower.Stop();
		m_Updateable.Deregister();
	}

	private void Tick(float pDeltaTime)
	{
		ODev.Util.Debug.Log(m_MovementRecieved.ToString(), this);
		if (m_Controller != null)
		{
			m_Controller.Move(Math.Horizontal(m_MovementRecieved) + (1f * pDeltaTime * Vector3.down));
			m_Controller.enabled = false;
			transform.position += m_MovementRecieved.y * Vector3.up;
			m_Controller.enabled = true;
		}
		else
		{
			transform.position += m_MovementRecieved;
		}
		m_MovementRecieved = Vector3.zero;
	}

	private Vector3 m_MovementRecieved = Vector3.zero;

	public void AddDisplacement(Vector3 pMovement, Quaternion pRotation)
	{
		ODev.Util.Debug.Log(pMovement.ToString(), this);
		m_MovementRecieved += pMovement;
		transform.rotation *= pRotation;
	}
}
