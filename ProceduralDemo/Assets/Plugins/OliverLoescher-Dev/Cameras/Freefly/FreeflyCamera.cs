using UnityEngine;
using Sirenix.OdinInspector;

namespace ODev.Camera
{
	public class FreeflyCamera : MonoBehaviour
	{
		[Header("Look")]
		[SerializeField]
		protected Transform m_LookTransform = null;
		[SerializeField]
		private float m_SensitivityDelta = 1.0f;
		[SerializeField]
		private float m_SensitivityUpdate = 1.0f;
		[SerializeField, MinMaxSlider(-90, 90, true)]
		private Vector2 m_CameraYClamp = new(-40, 50);

		[Header("Movement")]
		[SerializeField]
		protected Transform m_MoveTransform = null;
		[SerializeField]
		private float m_MoveSpeed = 1.0f;
		[SerializeField]
		private float m_SprintSpeed = 2.0f;

		// Inputs
		private Vector2 m_MoveInputHorizontal = new();
		private float m_MoveInputVertical = 0.0f;
		private Vector2 m_LookInput = new();
		private bool m_SprintInput = false;

		private void FixedUpdate()
		{
			if (m_MoveInputHorizontal != Vector2.zero || m_MoveInputVertical != 0.0f)
			{
				DoMove(m_MoveInputHorizontal, m_MoveInputVertical, (m_SprintInput ? m_SprintSpeed : m_MoveSpeed) * Time.fixedDeltaTime);
			}

			if (m_LookInput != Vector2.zero)
			{
				DoRotateCamera(m_SensitivityUpdate * Time.fixedDeltaTime * m_LookInput);
			}
		}

		protected virtual void DoMove(Vector2 pMovement, float pUp, float pMult)
		{
			Vector3 move = (pMovement.y * transform.forward) + (pMovement.x * transform.right) + (pUp * transform.up);
			m_MoveTransform.position += move.normalized * pMult;
		}

		protected virtual void DoRotateCamera(Vector2 pInput)
		{
			Vector3 euler = m_LookTransform.eulerAngles;
			euler.x = Mathf.Clamp(Util.Func.SafeAngle(euler.x - pInput.y), m_CameraYClamp.x, m_CameraYClamp.y);
			euler.y += pInput.x;
			euler.z = 0.0f;
			m_LookTransform.rotation = Quaternion.Euler(euler);
		}

		#region Inputs
		public void OnMoveHorizontal(Vector2 pInput)
		{
			m_MoveInputHorizontal = pInput;
		}

		public void OnMoveVertical(float pInput)
		{
			m_MoveInputVertical = pInput;
		}

		public void OnLook(Vector2 pInput)
		{
			m_LookInput = pInput;
		}

		public void OnLookDelta(Vector2 pInput)
		{
			DoRotateCamera(pInput * m_SensitivityDelta);
		}

		public void OnSprint(bool pInput)
		{
			m_SprintInput = pInput;
		}
		#endregion
	}
}
