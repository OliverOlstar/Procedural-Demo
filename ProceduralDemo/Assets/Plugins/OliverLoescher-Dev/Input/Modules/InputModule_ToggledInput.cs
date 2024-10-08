using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace ODev.Input
{
	[Serializable]
    public class InputModule_ToggledInput<T> : IInputModule where T : InputModule_Base, new()
	{
		[SerializeField]
		private InputModule_Toggle m_Toggle = new();
		[SerializeField]
		private T m_Value = new();

		public InputModule_Toggle Toggle => m_Toggle;
		public T Value => m_Value;

		public void Initalize(InputAction pInputAction, InputAction pToggleInputAction, Func<bool> pIsValid)
		{
			m_Toggle.Initalize(pToggleInputAction, pIsValid);
			m_Value.Initalize(pInputAction, pIsValid);
		}

		public void Enable() // IInputModule
		{
			m_Toggle.Enable();

			m_Toggle.RegisterOnPerformed(EnableValue);
			m_Toggle.RegisterOnCanceled(DisableValue);
		}

		public void Disable() // IInputModule
		{
			m_Toggle.Disable();
			m_Value.Disable();

			m_Toggle.DeregisterOnPerformed(EnableValue);
			m_Toggle.DeregisterOnCanceled(DisableValue);
		}

		public void Clear() // IInputModule
		{
			m_Toggle.Clear();
			m_Value.Clear();
		}

		public void Update(in float pDeltaTime) { } // IInputModule

		private void DisableValue()
		{
			m_Value.Disable();
			m_Value.Clear();
		}
		private void EnableValue()
		{
			m_Value.Enable();
		}
	}
}
