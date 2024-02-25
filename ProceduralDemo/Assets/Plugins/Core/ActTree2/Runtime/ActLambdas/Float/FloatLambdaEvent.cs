
using System.Collections.Generic;
using UnityEngine;

public abstract class FloatLambdaEvent<TEvent, TContext> : FloatLambdaBase
	where TEvent : Act2.ITreeEvent
	where TContext : Act2.ITreeContext
{
	public sealed override bool RequiresEvent(out System.Type eventType)
	{
		eventType = typeof(TEvent);
		return true;
	}

	public sealed override bool TryEvaluate(Act2.ITreeContext context, Act2.ITreeEvent treeEvent, out float value)
	{
		if (context is TContext requiredContext &&
			treeEvent is TEvent requiredEventType)
		{
			return TryEvaluateEvent(requiredEventType, requiredContext, out value);
		}
		else
		{
			value = 0.0f;
			return false;
		}
	}

	protected abstract bool TryEvaluateEvent(TEvent treeEvent, TContext context, out float value);
}
