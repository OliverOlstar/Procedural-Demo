using UnityEngine.Events;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

namespace OCore.Input
{
	[System.Serializable]
    public class InputModule_Trigger : InputModule_Base
	{
		[BoxGroup]
		public UnityEvent OnPerformed;

		public override void Enable()
		{
			m_InputAction.performed += OnPerformedInternal;
		}
		public override void Disable()
		{
			m_InputAction.performed -= OnPerformedInternal;
		}
		public override void Clear() { }

		private void OnPerformedInternal(InputAction.CallbackContext ctx)
		{
			if (!m_IsValid.Invoke())
			{
				return;
			}
			OnPerformed?.Invoke();
		}
	}
}
