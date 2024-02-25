
using System.Collections.Generic;
using UnityEngine;

public abstract class DirectionLambdaPole<TContext> : DirectionLambdaBase where TContext : Act2.ITreeContext
{
	public sealed override bool RequiresEvent(out System.Type eventType)
	{
		eventType = null;
		return false;
	}

	public override bool TryEvaluate(Act2.ITreeContext context, Act2.ITreeEvent treeEvent, out Vector3 direction)
	{
		switch (context)
		{
			case TContext requiredContext:
				return TryEvaluatePoling(requiredContext, out direction);
			default:
				direction = Vector3.forward;
				return false;
		}
	}

	protected abstract bool TryEvaluatePoling(TContext context, out Vector3 direction);
}
