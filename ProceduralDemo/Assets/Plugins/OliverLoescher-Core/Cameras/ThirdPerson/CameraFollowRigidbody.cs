using UnityEngine;

namespace OCore 
{
    public class CameraFollowRigidbody : MonoBehaviour
    {
        [SerializeField]
		private Rigidbody m_Target = null;

        [Header("Look")]
        [SerializeField]
		private float m_LookVelocity = 1.0f;
        [SerializeField]
		private Vector3 m_LookOffset = new();
        [SerializeField]
		private float m_LookDampening = 5.0f;

        [Header("Follow")]
        [SerializeField]
		private Vector3 m_FollowOffset = new();
        [SerializeField]
		private float m_FollowDampening = 1.0f;

        void Update()
        {
            transform.position = Vector3.Lerp(transform.position, m_Target.position + m_FollowOffset, m_FollowDampening * Time.deltaTime);

            Vector3 lookAtTarget = m_Target.transform.position + (m_Target.velocity * m_LookVelocity) + m_LookOffset;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookAtTarget - transform.position), Time.deltaTime * m_LookDampening);
        }

        private void OnDrawGizmosSelected() 
        {
            transform.position = m_Target.position + m_FollowOffset;
            transform.LookAt(m_Target.transform.position + (m_Target.velocity * m_LookVelocity) + m_LookOffset);
        }
    }
}