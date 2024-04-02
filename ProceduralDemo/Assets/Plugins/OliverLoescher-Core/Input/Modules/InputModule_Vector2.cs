using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

namespace OliverLoescher.Input
{
	[System.Serializable]
    public class InputModule_Vector2 : InputModule_Base
	{
		[BoxGroup, HideInEditorMode, SerializeField]
		private Vector2 input = new();
		public Vector2 Input => input;
		public Vector3 InputHorizontal => new(input.x, 0.0f, input.y);

		[BoxGroup, SerializeField] 
		private Vector2 scalar = Vector2.one;
		[BoxGroup, SerializeField]
		private bool normalize = false;
		[BoxGroup, SerializeField]
		private bool invertY = false;

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
			input = Vector2.zero;
			OnChanged?.Invoke(input);
		}

		private void OnPerformed(InputAction.CallbackContext ctx)
		{
			if (!m_IsValid.Invoke())
			{
				return;
			}
			input = ctx.ReadValue<Vector2>();
			input.x *= scalar.x;
			input.y *= scalar.y * (invertY ? -1 : 1);
			if (normalize)
			{
				input.Normalize();
			}
			OnChanged?.Invoke(input);
		}
	}
}
