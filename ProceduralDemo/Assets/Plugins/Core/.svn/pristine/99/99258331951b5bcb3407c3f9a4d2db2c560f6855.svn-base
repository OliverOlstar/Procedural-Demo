using System.Collections.Generic;
using UnityEngine;

public class GlobalActVarBehaviour : ActVarBehaviour
{
	public static GlobalActVarBehaviour GetOrCreate()
	{
		if (Core.MonoDirector.TryGet(out GlobalActVarBehaviour actVars))
		{
			return actVars;
		}
		actVars = new GameObject("GlobalActVars").AddComponent<GlobalActVarBehaviour>();
		return actVars;
	}

	protected override void Awake()
	{
		Core.MonoDirector.Register(this);
	}

	private void OnDestroy()
	{
		Core.MonoDirector.Deregister(this);
	}
}
