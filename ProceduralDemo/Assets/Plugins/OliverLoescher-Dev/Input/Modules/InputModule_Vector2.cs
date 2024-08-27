using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace ODev.Input
{
	[System.Serializable]
    public class InputModule_Vector2 : InputModule_Base, IInputVector2
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

		[SerializeField, BoxGroup]
		private UnityEventsUtil.Vector2Event m_OnChanged;

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
			m_OnChanged?.Invoke(m_Input);
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
			m_OnChanged?.Invoke(m_Input);
		}

		public void RegisterOnChanged(UnityAction<Vector2> pAction) => m_OnChanged.AddListener(pAction);
		public void DeregisterOnChanged(UnityAction<Vector2> pAction) => m_OnChanged.RemoveListener(pAction);
	}
}
