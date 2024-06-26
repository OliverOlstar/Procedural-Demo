using System;
using UnityEngine.InputSystem;

namespace OCore.Input
{
	public abstract class InputModule_Base : IInputModule
	{
		protected Func<bool> m_IsValid = null;
		protected InputAction m_InputAction = null;

		public virtual void Initalize(InputAction pInputAction, Func<bool> pIsValid)
		{
			m_InputAction = pInputAction;
			m_IsValid = pIsValid;
		}

		public abstract void Enable();
		public abstract void Disable();
		public abstract void Clear();
		public virtual void Update(in float pDeltaTime) { }
	}
}

public interface IInputModule
{
	public void Enable();
	public void Disable();
	public void Clear();
	public void Update(in float pDeltaTime);
}