using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using System;
using UnityEngine.Events;

namespace ODev.Input
{
	[Serializable]
    public class InputModule_Scroll : InputModule_Base, IInputFloat
	{
		[Space, SerializeField, BoxGroup]
		private float m_Input = 0.0f;
		public float Input => m_Input;

		// Events
		[SerializeField, BoxGroup]
		private UnityEventsUtil.FloatEvent m_OnChanged;

		public override void Initalize(InputAction pInputAction, Func<bool> pIsValid)
		{
			base.Initalize(pInputAction, pIsValid);
		}

		public override void Enable()
		{
			m_InputAction.performed += OnPerformed;
		}
		public override void Disable()
		{
			m_InputAction.performed -= OnPerformed;
		}
		public override void Clear() { }

		private void OnPerformed(InputAction.CallbackContext ctx)
		{
			if (!m_IsValid.Invoke())
			{
				return;
			}
			m_Input = ctx.ReadValue<float>();
			m_OnChanged?.Invoke(m_Input);
		}

		public void RegisterOnChanged(UnityAction<float> pAction) => m_OnChanged.AddListener(pAction);
		public void DeregisterOnChanged(UnityAction<float> pAction) => m_OnChanged.RemoveListener(pAction);
	}
}
