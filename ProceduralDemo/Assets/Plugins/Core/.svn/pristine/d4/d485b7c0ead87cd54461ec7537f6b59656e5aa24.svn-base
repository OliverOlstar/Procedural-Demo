
using System.Collections.Generic;
using UnityEngine;

public class EventCondition<T> : ActCondition where T : ActParams
{
	public override bool RequiresEvent() { return true; }

	public override bool Evaluate(ActParams param)
	{
		T requiredParams = param as T;
		if (requiredParams == null)
		{
			return false;
		}
		return EvaluateParams(requiredParams);
	}

	protected virtual bool EvaluateParams(T param)
	{
		return true;
	}
}
