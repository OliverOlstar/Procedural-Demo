
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public abstract class ConditionPoling<TContext> : ConditionGeneric<TContext> where TContext : ITreeContext
	{
		public override bool IsEventRequired(out System.Type eventType)
		{
			eventType = null;
			return false;
		}

		protected override bool OnEvaluate(ITreeEvent treeEvent)
		{
			return EvaluatePoling();
		}

		protected abstract bool EvaluatePoling();
	}
}
