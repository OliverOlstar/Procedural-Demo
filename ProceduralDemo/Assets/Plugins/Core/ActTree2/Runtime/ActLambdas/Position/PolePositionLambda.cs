
using System.Collections.Generic;
using UnityEngine;

public abstract class PolePositionLambda<TContext> : BasePositionLambda where TContext : Act2.ITreeContext
{
	public sealed override bool RequiresEvent(out System.Type eventType)
	{
		eventType = null;
		return false;
	}

	public override bool TryEvaluate(Act2.ITreeContext context, Act2.ITreeEvent treeEvent, out Vector3 position)
	{
		switch (context)
		{
			case TContext requiredContext:
				return TryEvaluatePoling(requiredContext, out position);
			default:
				position = Vector3.forward;
				return false;
		}
	}

	protected abstract bool TryEvaluatePoling(TContext context, out Vector3 position);
}
