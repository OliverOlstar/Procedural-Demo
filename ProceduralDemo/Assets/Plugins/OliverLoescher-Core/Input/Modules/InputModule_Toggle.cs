using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

namespace OCore.Input
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
		public UnityEventsUtil.BoolEvent onChanged;
		[BoxGroup]
		public UnityEvent onPerformed;
		[BoxGroup]
		public UnityEvent onCanceled;

		public override void Enable()
		{
			m_InputAction.performed += OnPerformed;
			m_InputAction.canceled += OnCanceled;
		}
		public override void Disable()
		{
			m_InputAction.performed -= OnPerformed;
			m_InputAction.canceled -= OnCanceled;
		}
		public override void Clear()
		{
			Set(false);
		}

		private void OnPerformed(InputAction.CallbackContext ctx)
		{
			if (!m_IsValid.Invoke())
			{
				return;
			}
			// True if not toggle || not currently pressed
			// False if toggle && currently pressed
			Set(!m_IsToggle || !m_Input);
		}
		private void OnCanceled(InputAction.CallbackContext ctx)
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
				onChanged.Invoke(m_Input);
				if (m_Input)
				{
					onPerformed?.Invoke();
				}
				else
				{
					onCanceled?.Invoke();
				}
			}
		}
	}
}
