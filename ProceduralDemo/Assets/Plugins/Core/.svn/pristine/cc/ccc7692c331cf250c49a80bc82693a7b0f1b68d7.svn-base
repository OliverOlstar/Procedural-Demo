
using System.Collections.Generic;
using UnityEngine;

public abstract class FloatLambdaPole<TContext> : FloatLambdaBase where TContext : Act2.ITreeContext
{
	public sealed override bool RequiresEvent(out System.Type eventType)
	{
		eventType = null;
		return false;
	}

	public sealed override bool TryEvaluate(Act2.ITreeContext context, Act2.ITreeEvent treeEvent, out float value)
	{
		switch (context)
		{
			case TContext requiredContext:
				return TryEvaluatePoling(requiredContext, out value);
			default:
				value = 0.0f;
				return false;
		}
	}

	protected abstract bool TryEvaluatePoling(TContext context, out float value);
}
