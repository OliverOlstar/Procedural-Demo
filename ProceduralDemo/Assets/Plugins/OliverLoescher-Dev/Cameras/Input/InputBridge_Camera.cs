using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ODev.Input;

namespace ODev
{
	public class InputBridge_Camera : InputBridge_Base
	{
		[SerializeField]
		private InputModule_Vector2Update m_LookInput = new();
		[SerializeField]
		private InputModule_Vector2 m_LookDeltaInput = new();
		[SerializeField]
		private InputModule_Scroll m_ZoomInput = new();

		public InputModule_Vector2Update Look => m_LookInput;
		public InputModule_Vector2 LookDelta => m_LookDeltaInput;
		public InputModule_Scroll Zoom => m_ZoomInput;

		public override InputActionMap Actions => InputSystem.Instance.Camera.Get();
		public override IEnumerable<IInputModule> GetAllInputModules()
		{
			yield return m_LookInput;
			yield return m_LookDeltaInput;
			yield return m_ZoomInput;
		}

		protected override void Awake()
		{
			m_LookInput.Initalize(InputSystem.Instance.Camera.Look, IsValid);
			m_LookDeltaInput.Initalize(InputSystem.Instance.Camera.LookDelta, IsValid);
			m_ZoomInput.Initalize(InputSystem.Instance.Camera.Zoom, IsValid);

			base.Awake();
		}

		protected override void OnEnable()
		{
			Cursor.lockState = CursorLockMode.Locked;

			base.OnEnable();
		}

		protected override void OnDisable()
		{
			Cursor.lockState = CursorLockMode.None;

			base.OnDisable();
		}
	}
}