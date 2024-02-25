
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public abstract class ConditionEvent<TEvent, TContext> : ConditionGeneric<TContext>
		where TEvent : ITreeEvent
		where TContext : ITreeContext
	{
		public new static System.Type _EditorGetEvent() => typeof(TEvent);

		public sealed override bool IsEventRequired(out System.Type eventType)
		{
			eventType = typeof(TEvent);
			return true;
		}

		protected sealed override bool OnEvaluate(ITreeEvent treeEvent)
		{
			if (treeEvent is TEvent requiredEvent)
			{
				return EvaluateEvent(requiredEvent);
			}
			return false;
		}

		protected abstract bool EvaluateEvent(TEvent treeEvent);
	}
}
