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
		[Space, HideInEditorMode, SerializeField, BoxGroup]
		private float m_Input = 0.0f;
		[SerializeField, HideInEditorMode, BoxGroup]
		private bool m_IsInputing = false;

		[BoxGroup, SerializeField]
		private float m_Scalar = 1.0f;

		// Events
		[SerializeField, BoxGroup]
		private UnityEventsUtil.FloatEvent m_OnStarted = new();
		[SerializeField, BoxGroup]
		private UnityEventsUtil.FloatEvent m_OnStopped = new();
		[SerializeField, BoxGroup]
		private UnityEventsUtil.FloatEvent m_OnChanged = new();

		public float Input => m_Input;
		public bool IsInputing => m_IsInputing;

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
			m_Input = ctx.ReadValue<float>() * m_Scalar;

			if (!m_IsInputing)
			{
				m_IsInputing = true;
				m_OnStarted.Invoke(m_Input);
			}
			m_OnChanged.Invoke(m_Input);
		}

		private void OnCanceled(InputAction.CallbackContext ctx)
		{
			if (!m_IsValid.Invoke())
			{
				return;
			}
			m_Input = ctx.ReadValue<float>() * m_Scalar;

			m_OnChanged.Invoke(m_Input);
			m_OnStopped.Invoke(m_Input);
		}

		public void RegisterOnStarted(UnityAction<float> pAction) => m_OnStarted.AddListener(pAction);
		public void DeregisterOnStarted(UnityAction<float> pAction) => m_OnStarted.RemoveListener(pAction);
		public void RegisterOnStopped(UnityAction<float> pAction) => m_OnStopped.AddListener(pAction);
		public void DeregisterOnStopped(UnityAction<float> pAction) => m_OnStopped.RemoveListener(pAction);
		public void RegisterOnChanged(UnityAction<float> pAction) => m_OnChanged.AddListener(pAction);
		public void DeregisterOnChanged(UnityAction<float> pAction) => m_OnChanged.RemoveListener(pAction);
	}
}
