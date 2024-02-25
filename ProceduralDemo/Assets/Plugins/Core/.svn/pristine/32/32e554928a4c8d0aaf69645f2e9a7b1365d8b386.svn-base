using ActValue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lambdas/Direction/Act Var")]
public class ActVarDirectionLambda : DirectionLambdaPole<IActVarContext>
{
	[SerializeField, UberPicker.AssetNonNull]
	private SOActVar m_Var = null;

	protected override bool TryEvaluatePoling(IActVarContext context, out Vector3 direction)
	{
		if (m_Var == null)
		{
			direction = Vector3.forward;
			return false;
		}
		return context.ActVarBehaviour.TryGetVector(m_Var, out direction);
	}
}
