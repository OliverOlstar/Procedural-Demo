using UnityEngine;

namespace OCore
{
	[RequireComponent(typeof(Animator))]
	public class RootMotionCharacter : MonoBehaviour
	{
		public bool IgnoreYValue = false;

		[Header("Root Motion")]
		[SerializeField]
		private CharacterController m_Character = null;
		[SerializeField]
		private OnGround m_Grounded = null;
		private Animator m_Animator = null;

		[Space, SerializeField]
		private float m_Gravity = 9.81f;
		[SerializeField]
		private float m_StepDown = 0.1f;

		private bool m_InAir = false;
		private Vector3 m_RootMotion = new();
		private Vector3 m_Velocity = Vector3.zero;

		[Space, SerializeField]
		private float m_PushPower = 2.0f;

		void Start()
		{
			m_Animator = GetComponent<Animator>();

			RootMotionCharacterReciever reciever = m_Character.gameObject.AddComponent<RootMotionCharacterReciever>();
			reciever.Init(this);
		}

		private void OnAnimatorMove()
		{
			m_RootMotion += m_Animator.deltaPosition;
			m_Character.transform.rotation *= m_Animator.deltaRotation;
		}

		private void FixedUpdate()
		{
			if (IgnoreYValue)
			{
				MoveRootMotion();
				m_InAir = true;
				return;
			}
			if (m_InAir)
			{
				UpdateInAir();
				return;
			}
			UpdateOnGround();
		}

		private void UpdateInAir()
		{
			m_Velocity.y -= m_Gravity * Time.fixedDeltaTime; // Gravity

			m_Character.Move(m_Velocity * Time.fixedDeltaTime);

			if (m_Character.isGrounded)
			{
				m_InAir = false;
				m_RootMotion = Vector3.zero;
			}
		}

		private void UpdateOnGround()
		{
			MoveRootMotion();

			if (m_Grounded.IsGrounded)
			{
				m_Character.Move(Vector3.down * m_StepDown);
			}

			if (!m_Character.isGrounded)
			{
				m_InAir = true;
				m_Velocity = m_Animator.velocity;
			}
		}

		private void MoveRootMotion()
		{
			m_Character.Move(m_RootMotion);
			m_RootMotion = Vector3.zero;
		}

		public void DoJump(float pUp, float pForward)
		{
			m_InAir = true;
			m_Velocity = Util.Math.Horizontalize(m_Animator.velocity) * pForward;
			m_Velocity.y = Mathf.Sqrt(2 * m_Gravity * pUp);
		}

		public void OnControllerColliderHit(ControllerColliderHit hit)
		{

			Rigidbody body = hit.collider.attachedRigidbody;
			if (body == null || body.isKinematic)
			{
				return; // No rigidbody
			}

			if (hit.moveDirection.y < -0.3f)
			{
				return; // We dont want to push objects below us
			}

			// Calculate push direction from move direction,
			// we only push objects to the sides never up and down
			Vector3 pushDir = new(hit.moveDirection.x, 0, hit.moveDirection.z);

			// If you know how fast your character is trying to move,
			// then you can also multiply the push velocity by that.
			body.velocity = pushDir * m_PushPower; // Apply the push
		}
	}
}