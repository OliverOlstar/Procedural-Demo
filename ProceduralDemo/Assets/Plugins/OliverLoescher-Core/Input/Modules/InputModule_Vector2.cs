using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

namespace OCore.Input
{
	[System.Serializable]
    public class InputModule_Vector2 : InputModule_Base
	{
		[BoxGroup, HideInEditorMode, SerializeField]
		private Vector2 m_Input = new();
		public Vector2 Input => m_Input;
		public Vector3 InputHorizontal => new(m_Input.x, 0.0f, m_Input.y);

		[BoxGroup, SerializeField] 
		private Vector2 m_Scalar = Vector2.one;
		[BoxGroup, SerializeField]
		private bool m_Normalize = false;
		[BoxGroup, SerializeField]
		private bool m_InvertY = false;

		[BoxGroup]
		public UnityEventsUtil.Vector2Event OnChanged;

		public override void Enable()
		{
			m_InputAction.performed += OnPerformed;
			m_InputAction.canceled += OnPerformed;
		}
		public override void Disable()
		{
			m_InputAction.performed -= OnPerformed;
			m_InputAction.canceled += OnPerformed;
		}
		public override void Clear()
		{
			m_Input = Vector2.zero;
			OnChanged?.Invoke(m_Input);
		}

		private void OnPerformed(InputAction.CallbackContext ctx)
		{
			if (!m_IsValid.Invoke())
			{
				return;
			}
			m_Input = ctx.ReadValue<Vector2>();
			m_Input.x *= m_Scalar.x;
			m_Input.y *= m_Scalar.y * (m_InvertY ? -1 : 1);
			if (m_Normalize)
			{
				m_Input.Normalize();
			}
			OnChanged?.Invoke(m_Input);
		}
	}
}
