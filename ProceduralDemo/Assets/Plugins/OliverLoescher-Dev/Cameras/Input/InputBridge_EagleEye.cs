using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ODev.Input;
using ODev.Util;

namespace ODev
{
	public class InputBridge_EagleEye : InputBridge_Base
	{
		[SerializeField]
		private InputModule_Vector2 m_MoveInput = new();
		[SerializeField]
		private InputModule_ToggledInput<InputModule_Vector2> m_MoveDeltaInput = new();
		[SerializeField]
		private InputModule_Scroll m_ZoomInput = new();
		[SerializeField]
		private InputModule_Scroll m_ZoomDeltaInput = new();
		[SerializeField]
		private InputModule_Scroll m_RotateInput = new();

		public InputModule_Vector2 Move => m_MoveInput;
		public InputModule_ToggledInput<InputModule_Vector2> MoveDelta => m_MoveDeltaInput;
		public InputModule_Scroll Zoom => m_ZoomInput;
		public InputModule_Scroll ZoomDelta => m_ZoomDeltaInput;
		public InputModule_Scroll Rotate => m_RotateInput;

		public override InputActionMap Actions => InputSystem.Instance.EagleEye.Get();
		public override IEnumerable<IInputModule> GetAllInputModules()
		{
			yield return m_MoveInput;
			yield return m_MoveDeltaInput;
			yield return m_ZoomInput;
			yield return m_ZoomDeltaInput;
			yield return m_RotateInput;
		}

		protected override void Awake()
		{
			PlayerInput instace = InputSystem.Instance;
			PlayerInput.EagleEyeActions input = InputSystem.Instance.EagleEye;
			m_MoveInput.Initalize(input.Move, IsValid);
			m_MoveDeltaInput.Initalize(input.MoveDelta, input.MoveDeltaButton, IsValid);
			m_ZoomInput.Initalize(input.Zoom, IsValid);
			m_ZoomDeltaInput.Initalize(input.ZoomDelta, IsValid);
			m_RotateInput.Initalize(input.Rotate, IsValid);

			base.Awake();
		}

		protected override void OnEnabled()
		{
			Input.Cursor.AddConfined();
		}

		protected override void OnDisabled()
		{
			Input.Cursor.RemoveConfined();
		}
	}
}