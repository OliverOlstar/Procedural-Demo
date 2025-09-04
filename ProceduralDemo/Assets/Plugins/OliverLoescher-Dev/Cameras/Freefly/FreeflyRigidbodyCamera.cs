using UnityEngine;

namespace ODev.Camera
{
	public class FreeflyRigidbodyCamera : FreeflyCamera
	{
		[SerializeField]
		private Rigidbody m_MoveRigidbody = null;

		private void Start()
		{
			m_MoveTransform = null;
		}

		private void OnEnable()
		{
			m_MoveRigidbody.linearVelocity = Vector3.zero;
		}

		protected override void DoMove(Vector2 pMovement, float pUp, float pMult)
		{
			Vector3 move = (pMovement.y * transform.forward) + (pMovement.x * transform.right) + (pUp * transform.up);
			m_MoveRigidbody.linearVelocity = move.normalized * pMult;
		}
	}
}