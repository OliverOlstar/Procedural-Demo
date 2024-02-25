using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lambdas/Direction/Game Object Direction")]
public class GODirectionLambda : DirectionLambdaPole<Act2.IGOContext>
{
	[SerializeField]
	private Vector3 m_Direction = Vector3.forward;

	[SerializeField, Core.ReadOnly]
	private Vector3 m_NormalizedDirection = Vector3.forward;

	private void OnValidate()
	{
		m_NormalizedDirection = m_Direction;
		Core.Util.Normalize(ref m_NormalizedDirection, true);
	}

	protected override bool TryEvaluatePoling(Act2.IGOContext context, out Vector3 direction)
	{
		direction = context.Transform.rotation * m_Direction;
		return true;
	}
}
