using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public abstract class SOActConditionFab<TContext> : SOActConditionFabBase where TContext : ITreeContext
	{
		public override System.Type GetContextType() => typeof(TContext);
	}
}
