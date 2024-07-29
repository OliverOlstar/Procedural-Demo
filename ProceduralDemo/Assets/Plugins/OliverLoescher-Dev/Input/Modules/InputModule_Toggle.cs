using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

namespace ODev.Input
{
	[System.Serializable]
    public class InputModule_Toggle : InputModule_Base
	{
		[SerializeField, BoxGroup]
		private bool m_IsToggle = false;
		public bool IsToggle => m_IsToggle;
		[Space, HideInEditorMode, SerializeField, BoxGroup]
		private bool m_Input = false;
		public bool Input => m_Input;

		// Events
		[BoxGroup]
		public UnityEventsUtil.BoolEvent OnChanged;
		[BoxGroup]
		public UnityEvent OnPerformed;
		[BoxGroup]
		public UnityEvent OnCanceled;

		public override void Enable()
		{
			m_InputAction.performed += OnPerformedEvent;
			m_InputAction.canceled += OnCanceledEvent;
		}
		public override void Disable()
		{
			m_InputAction.performed -= OnPerformedEvent;
			m_InputAction.canceled -= OnCanceledEvent;
		}
		public override void Clear()
		{
			Set(false);
		}

		private void OnPerformedEvent(InputAction.CallbackContext ctx)
		{
			if (!m_IsValid.Invoke())
			{
				return;
			}
			// True if not toggle || not currently pressed
			// False if toggle && currently pressed
			Set(!m_IsToggle || !m_Input);
		}
		private void OnCanceledEvent(InputAction.CallbackContext ctx)
		{
			if (!m_IsValid.Invoke())
			{
				return;
			}
			if (!m_IsToggle)
			{
				Set(false);
			}
		}

		private void Set(bool pValue)
		{
			if (m_Input != pValue)
			{
				m_Input = pValue;

				// Events
				OnChanged.Invoke(m_Input);
				if (m_Input)
				{
					OnPerformed?.Invoke();
				}
				else
				{
					OnCanceled?.Invoke();
				}
			}
		}
	}
}
