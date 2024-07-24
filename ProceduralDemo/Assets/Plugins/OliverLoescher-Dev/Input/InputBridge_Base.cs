using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ODev.Input
{
	public abstract class InputBridge_Base : MonoBehaviour
	{
		[SerializeField]
		private Util.Mono.Updateable m_Updateable = new(Util.Mono.Type.Early, Util.Mono.Priorities.Input);

		public abstract InputActionMap Actions { get; }
		public abstract IEnumerable<IInputModule> GetAllInputModules();

		protected virtual void Awake()
		{
			foreach (IInputModule module in GetAllInputModules())
			{
				module.Enable();
			}
			PauseSystem.s_OnPause += ClearInputs;
		}

		protected virtual void OnDestroy()
		{
			foreach (IInputModule module in GetAllInputModules())
			{
				module.Disable();
			}
			PauseSystem.s_OnPause -= ClearInputs;
		}

		private void OnEnable()
		{
			if (!Util.Func.IsApplicationQuitting)
			{
				Actions.Enable();
			}
			m_Updateable.Register(Tick);
			OnEnabled();
		}
		protected virtual void OnEnabled() { }

		private void OnDisable()
		{
			if (!Util.Func.IsApplicationQuitting)
			{
				Actions.Disable();
			}
			m_Updateable.Deregister();
			OnDisabled();
		}
		protected virtual void OnDisabled() { }

		public virtual void Tick(float pDeltaTime)
		{
			foreach (IInputModule module in GetAllInputModules())
			{
				module.Update(pDeltaTime);
			}
		}

		public virtual void ClearInputs()
		{
			foreach (IInputModule module in GetAllInputModules())
			{
				module.Clear();
			}
		}

		public virtual bool IsValid()
		{
			return !PauseSystem.IsPaused;
		}
	}
}
