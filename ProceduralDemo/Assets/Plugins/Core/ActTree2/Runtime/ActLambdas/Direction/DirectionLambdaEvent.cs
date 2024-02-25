using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DirectionLambdaEvent<TEvent, TContext> : DirectionLambdaBase 
	where TEvent : Act2.ITreeEvent
	where TContext : Act2.ITreeContext
{
	public sealed override bool RequiresEvent(out System.Type eventType)
	{
		eventType = typeof(TEvent);
		return true;
	}

	public sealed override bool TryEvaluate(Act2.ITreeContext context, Act2.ITreeEvent treeEvent, out Vector3 direction)
	{
		if (context is TContext requiredContext &&
			treeEvent is TEvent requiredEventType)
		{
			return TryEvaluateEvent(requiredEventType, requiredContext, out direction);
		}
		else
		{
			direction = Vector3.forward;
			return false;
		}
	}

	protected abstract bool TryEvaluateEvent(TEvent eventParams, TContext context, out Vector3 direction);
}
