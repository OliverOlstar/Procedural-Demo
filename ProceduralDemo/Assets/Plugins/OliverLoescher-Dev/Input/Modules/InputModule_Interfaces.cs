using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ODev.Input
{
	public interface IInputTrigger
	{
		public void RegisterOnPerformed(UnityAction pAction);
		public void DeregisterOnPerformed(UnityAction pAction);
	}

	public interface IInputBool : IInputTrigger
	{
		public bool Input { get; }

		public void RegisterOnChanged(UnityAction<bool> pAction);
		public void DeregisterOnChanged(UnityAction<bool> pAction);
		
		public void RegisterOnCanceled(UnityAction pAction);
		public void DeregisterOnCanceled(UnityAction pAction);
	}

	public interface IInputFloat
	{
		public float Input { get; }

		public void RegisterOnChanged(UnityAction<float> pAction);
		public void DeregisterOnChanged(UnityAction<float> pAction);
	}

	public interface IInputVector2
	{
		public Vector2 Input { get; }

		public void RegisterOnChanged(UnityAction<Vector2> pAction);
		public void DeregisterOnChanged(UnityAction<Vector2> pAction);
	}

	public interface IInputVector2Update : IInputVector2
	{
		public void RegisterOnUpdate(UnityAction<Vector2> pAction);
		public void DeregisterOnUpdate(UnityAction<Vector2> pAction);
	}
}
