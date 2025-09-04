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
		[BoxGroup, HideInEditorMode, SerializeField]
		private bool m_IsInputing = false;

		[BoxGroup, SerializeField] 
		private Vector2 m_Scalar = Vector2.one;
		[BoxGroup, SerializeField]
		private bool m_Normalize = false;
		[BoxGroup, SerializeField]
		private bool m_InvertY = false;

		[SerializeField, BoxGroup]
		private UnityEventsUtil.Vector2Event m_OnStarted = new();
		[SerializeField, BoxGroup]
		private UnityEventsUtil.Vector2Event m_OnStopped = new();
		[SerializeField, BoxGroup]
		private UnityEventsUtil.Vector2Event m_OnChanged = new();

		public Vector2 Input => m_Input;
		public bool IsInputing => m_IsInputing;
		public Vector3 InputHorizontal => new(m_Input.x, 0.0f, m_Input.y);

		public override void Enable()
		{
			m_InputAction.performed += OnPerformed;
			m_InputAction.canceled += OnCanceled;
		}
		public override void Disable()
		{
			m_InputAction.performed -= OnPerformed;
			m_InputAction.canceled += OnCanceled;
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
			Evaluate(ctx);
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
			Evaluate(ctx);
			m_IsInputing = false;
			m_OnChanged.Invoke(m_Input);
			m_OnStopped.Invoke(m_Input);
		}

		private void Evaluate(InputAction.CallbackContext ctx)
		{
			m_Input = ctx.ReadValue<Vector2>();
			if (m_Normalize)
			{
				m_Input.Normalize();
			}
			m_Input.x *= m_Scalar.x;
			m_Input.y *= m_Scalar.y * (m_InvertY ? -1 : 1);
		}

		public void RegisterOnStarted(UnityAction<Vector2> pAction) => m_OnStarted.AddListener(pAction);
		public void DeregisterOnStarted(UnityAction<Vector2> pAction) => m_OnStarted.RemoveListener(pAction);
		public void RegisterOnStopped(UnityAction<Vector2> pAction) => m_OnStopped.AddListener(pAction);
		public void DeregisterOnStopped(UnityAction<Vector2> pAction) => m_OnStopped.RemoveListener(pAction);
		public void RegisterOnChanged(UnityAction<Vector2> pAction) => m_OnChanged.AddListener(pAction);
		public void DeregisterOnChanged(UnityAction<Vector2> pAction) => m_OnChanged.RemoveListener(pAction);
	}
}
