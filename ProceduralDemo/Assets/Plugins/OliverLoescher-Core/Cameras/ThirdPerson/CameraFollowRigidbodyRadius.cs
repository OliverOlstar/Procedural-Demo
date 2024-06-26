using UnityEngine;

namespace OCore
{
	public class CameraFollowRigidbodyRadius : MonoBehaviour
	{
		[SerializeField]
		private Rigidbody m_Target = null;
		[SerializeField]
		private Util.Mono.Updateable m_Updateable = new(Util.Mono.Type.Late, Util.Mono.Priorities.Camera);

		[Header("Look")]
		[SerializeField]
		private float m_LookVelocity = 1.0f;
		[SerializeField]
		private Vector3 m_LookOffset = new();
		[SerializeField]
		private float m_LookDampening = 5.0f;

		[Header("Follow")]
		[SerializeField]
		private float m_FollowDistance = 9.0f;
		[SerializeField]
		private float m_FollowHeight = 2.0f;
		[SerializeField]
		private float m_FollowDampening = 1.0f;

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
			Vector3 offset = Util.Math.Horizontalize(transform.position - m_Target.position) * m_FollowDistance; // x, z
			offset.y = m_FollowHeight; // y
			transform.position = Vector3.Lerp(transform.position, m_Target.position + offset, m_FollowDampening * pDeltaTime);

			Vector3 lookAtTarget = m_Target.transform.position + (m_Target.velocity * m_LookVelocity) + m_LookOffset;
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookAtTarget - transform.position), pDeltaTime * m_LookDampening);
		}

		private void OnDrawGizmosSelected()
		{
			if (m_Target == null)
			{
				return;
			}
			// transform.position = target.position + followOffset;
			transform.LookAt(m_Target.transform.position + (m_Target.velocity * m_LookVelocity) + m_LookOffset);
		}
	}
}