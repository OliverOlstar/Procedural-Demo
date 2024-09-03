using System;
using UnityEngine;

[System.Serializable]
public class PlayerSpearBehaviour
{
	[SerializeField]
	private PlayerSpear m_Spear = null;
	[SerializeField]
	private SpringRope m_Rope = null;

	private PlayerRoot m_Root;
	private bool m_PlayerIsInTrigger = false;

	public PlayerSpear Spear => m_Spear;
	public Vector3 Position => m_Spear.transform.position;
	public Vector3 Forward => m_Spear.transform.forward;
	public PlayerSpear.State State => m_Spear.ActiveState;
	public bool PlayerIsInTrigger => m_PlayerIsInTrigger;

	public void Throw(Vector3 pPoint, Vector3 pDirection/*, float pCharge01*/) => Spear.Throw(pPoint, pDirection);
	public void Attach(Transform pAttachTo, Vector3 pHitPoint) => Spear.Attach(pAttachTo, pHitPoint);
	public void Pull(Transform pToTarget) => Spear.Pull(pToTarget);
	public void Store() => Spear.Store();

	public void RopeLaunch() => m_Rope.Launch();
	public void RopeReturn() => m_Rope.Return();
	public void SetRopeLength(float pLength) => m_Rope.SetMaxLength(pLength);

	public void Initalize(PlayerRoot pRoot)
	{
		m_Root = pRoot;
		m_Spear.OnStateChangeEvent.AddListener(OnStateChange);
		m_Spear.OnTriggerEnterEvent.AddListener(OnTriggerEnter);
		m_Spear.OnTriggerExitEvent.AddListener(OnTriggerExit);
	}

	public void Destroy()
	{
		m_Spear.OnStateChangeEvent.RemoveListener(OnStateChange);
		m_Spear.OnTriggerEnterEvent.RemoveListener(OnTriggerEnter);
		m_Spear.OnTriggerExitEvent.RemoveListener(OnTriggerExit);
	}

	private void OnTriggerEnter(Collider pOther)
	{
		if (pOther.gameObject != m_Root.Movement.gameObject)
		{
			return;
		}
		m_PlayerIsInTrigger = true;
	}

	private void OnTriggerExit(Collider pOther)
	{
		if (pOther.gameObject != m_Root.Movement.gameObject)
		{
			return;
		}
		m_PlayerIsInTrigger = false;
	}

	private void OnStateChange(PlayerSpear.State pState)
	{
		m_PlayerIsInTrigger = false;
	}
}
