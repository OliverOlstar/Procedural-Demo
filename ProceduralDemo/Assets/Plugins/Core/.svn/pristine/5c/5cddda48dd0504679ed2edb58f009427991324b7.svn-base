using ActValue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lambdas/Position/Act Var")]
public class ActVarPositionLambda : PolePositionLambda<IActVarContext>
{
	[SerializeField, UberPicker.AssetNonNull]
	private SOActVar m_Var = null;

	protected override bool TryEvaluatePoling(IActVarContext context, out Vector3 position)
	{
		if (m_Var == null)
		{
			position = Vector3.zero;
			return false;
		}
		return context.ActVarBehaviour.TryGetVector(m_Var, out position);
	}
}
