
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lambdas/Position/Game Object Position")]
public class GOPositionLambda : PolePositionLambda<Act2.IGOContext>
{
	protected override bool TryEvaluatePoling(Act2.IGOContext context, out Vector3 position)
	{
		position = context.Transform.position;
		return true;
	}
}
