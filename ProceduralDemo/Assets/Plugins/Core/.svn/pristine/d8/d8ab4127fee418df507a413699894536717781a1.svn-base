
using System.Collections.Generic;
using UnityEngine;

public abstract class EventPositionLambda<TEvent, TContext> : BasePositionLambda
	where TEvent : Act2.ITreeEvent
	where TContext : Act2.ITreeContext
{
	public sealed override bool RequiresEvent(out System.Type eventType)
	{
		eventType = typeof(TEvent);
		return true;
	}

	public sealed override bool TryEvaluate(Act2.ITreeContext context, Act2.ITreeEvent treeEvent, out Vector3 position)
	{
		if (context is TContext requiredContext &&
			treeEvent is TEvent requiredEventType)
		{
			return TryEvaluateEvent(requiredEventType, requiredContext, out position);
		}
		else
		{
			position = Vector3.zero;
			return false;
		}
	}

	protected abstract bool TryEvaluateEvent(TEvent treeEvent, TContext context, out Vector3 position);
}
