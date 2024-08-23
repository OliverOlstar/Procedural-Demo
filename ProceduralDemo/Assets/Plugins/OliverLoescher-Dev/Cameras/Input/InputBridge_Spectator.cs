using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ODev.Input;

namespace ODev.Camera
{
	public class InputBridge_Spectator : InputBridge_Base
	{
		[SerializeField]
		private InputModule_Vector2Update m_LookInput = new();
		[SerializeField]
		private InputModule_Vector2 m_LookDeltaInput = new();
		[SerializeField]
		private InputModule_Vector2 m_MoveInput = new();
		[SerializeField]
		private InputModule_Scroll m_MoveVerticalInput = new();
		[SerializeField]
		private InputModule_Scroll m_ZoomInput = new();
		[SerializeField]
		private InputModule_Toggle m_ModeInput = new();
		[SerializeField]
		private InputModule_Toggle m_TargetInput = new();

		public InputModule_Vector2Update Look => m_LookInput;
		public InputModule_Vector2 LookDelta => m_LookDeltaInput;
		public InputModule_Vector2 Move => m_MoveInput;
		public InputModule_Scroll MoveVertical => m_MoveVerticalInput;
		public InputModule_Scroll Zoom => m_ZoomInput;
		public InputModule_Toggle Mode => m_ModeInput;
		public InputModule_Toggle Target => m_TargetInput;

		public override InputActionMap Actions => InputSystem.Instance.SpectatorCamera.Get();
		public override IEnumerable<IInputModule> GetAllInputModules()
		{
			yield return m_LookInput;
			yield return m_LookDeltaInput;
			yield return m_MoveInput;
			yield return m_MoveVerticalInput;
			yield return m_ZoomInput;
			yield return m_ModeInput;
			yield return m_TargetInput;
		}

		protected override void Awake()
		{
			PlayerInput.SpectatorCameraActions input = InputSystem.Instance.SpectatorCamera;
			m_LookInput.Initalize(input.Look, IsValid);
			m_LookDeltaInput.Initalize(input.LookDelta, IsValid);
			m_MoveInput.Initalize(input.MoveHorizontal, IsValid);
			m_MoveVerticalInput.Initalize(input.MoveVertical, IsValid);
			m_ZoomInput.Initalize(input.Zoom, IsValid);
			m_ModeInput.Initalize(input.ModeToggle, IsValid);
			m_TargetInput.Initalize(input.TargetToggle, IsValid);

			base.Awake();
		}

		protected override void OnEnabled()
		{
			Input.Cursor.AddLocked();
		}

		protected override void OnDisabled()
		{
			Input.Cursor.RemoveLocked();
		}
	}
}