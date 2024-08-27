using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

namespace ODev.Input
{
	[System.Serializable]
    public class InputModule_Trigger : InputModule_Base, IInputTrigger
	{
		[SerializeField, BoxGroup]
		private UnityEvent m_OnPerformed;

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
			m_OnPerformed?.Invoke();
		}

		public void RegisterOnPerformed(UnityAction pAction) => m_OnPerformed.AddListener(pAction);
		public void DeregisterOnPerformed(UnityAction pAction) => m_OnPerformed.RemoveListener(pAction);
	}
}
