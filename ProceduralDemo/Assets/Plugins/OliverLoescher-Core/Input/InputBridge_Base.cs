using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OliverLoescher.Input
{
    public abstract class InputBridge_Base : MonoBehaviour
	{
		public abstract InputActionMap Actions { get; }
		public abstract IEnumerable<IInputModule> GetAllInputModules();
		[SerializeField]
		private Util.Mono.Updateable updateable = new Util.Mono.Updateable(Util.Mono.Type.Early, Util.Mono.Priorities.Input);

		protected virtual void Awake()
		{
			foreach (IInputModule module in GetAllInputModules())
			{
				module.Enable();
			}
			PauseSystem.onPause += ClearInputs;
		}

		protected virtual void OnDestroy()
		{
			foreach (IInputModule module in GetAllInputModules())
			{
				module.Disable();
			}
			PauseSystem.onPause -= ClearInputs;
		}

		protected virtual void OnEnable()
		{
			Actions.Enable();
			updateable.Register(Tick);
		}

		protected virtual void OnDisable()
		{
			Actions.Disable();
			updateable.Deregister();
		}

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
			return PauseSystem.isPaused == false;
		}
	}
}
