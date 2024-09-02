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

	public PlayerSpear Spear => m_Spear;
	public Vector3 Position => m_Spear.transform.position;
	public Vector3 Forward => m_Spear.transform.forward;
	public PlayerSpear.State State => m_Spear.ActiveState;
	public void Throw(Vector3 pPoint, Vector3 pDirection/*, float pCharge01*/) => Spear.Throw(pPoint, pDirection);
	public void Attach(Transform pAttachTo, Vector3 pHitPoint) => Spear.Attach(pAttachTo, pHitPoint);
	public void Pull(Transform pToTarget) => Spear.Pull(pToTarget);
	public void Store() => Spear.Store();

	public void SetRopeActive(bool pActive)
	{
		m_Rope.IsGrappling = pActive;
	}

	public void Initalize(PlayerRoot pRoot)
	{
		m_Root = pRoot;
		m_Spear.OnTriggerEnterEvent.AddListener(OnSpearTrigger);
	}

	public void Destroy()
	{
		m_Spear.OnTriggerEnterEvent.RemoveListener(OnSpearTrigger);
	}

	private void OnSpearTrigger(Collider pOther)
	{
		if (pOther.gameObject != m_Root.Movement.gameObject)
		{
			return;
		}
		m_Root.Abilities.ActivateAbilityByTag(AbilityTags.SpearJump);
	}
}
