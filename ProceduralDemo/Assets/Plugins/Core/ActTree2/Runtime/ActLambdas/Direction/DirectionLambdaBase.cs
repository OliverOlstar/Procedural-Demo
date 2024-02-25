
using System.Collections.Generic;
using UnityEngine;

public abstract class DirectionLambdaBase : ScriptableObject
{
	public abstract bool RequiresEvent(out System.Type eventType);

	public abstract bool TryEvaluate(Act2.ITreeContext context, Act2.ITreeEvent treeEvent, out Vector3 direction);
}
