using UnityEngine;

namespace OCore
{
	public class BaseState : MonoBehaviour
	{
		protected StateMachine m_Machine; 

		public virtual void Init(StateMachine pMachine) { m_Machine = pMachine; }

		public virtual void OnEnter() { }
		public virtual void OnExit() { }
		public virtual bool CanEnter() { return false; }
		public virtual bool CanExit() { return false; }

		public virtual void OnFixedUpdate() { }
		public virtual void OnUpdate() { }
	}
}