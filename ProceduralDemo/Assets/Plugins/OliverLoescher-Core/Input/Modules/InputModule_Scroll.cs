using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using System;

namespace OliverLoescher.Input
{
	[System.Serializable]
    public class InputModule_Scroll : InputModule_Base
	{
		[Space, SerializeField, BoxGroup]
		private float m_Input = 0.0f;
		public float Input => m_Input;

		// Events
		[BoxGroup]
		public UnityEventsUtil.FloatEvent onChanged;

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
			onChanged?.Invoke(m_Input);
		}
	}
}
