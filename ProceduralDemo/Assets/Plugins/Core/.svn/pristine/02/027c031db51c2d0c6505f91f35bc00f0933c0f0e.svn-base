
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lambdas/Direction/World Space Constant")]
public class WorldDirectionLambda : DirectionLambdaPole<Act2.ITreeContext>
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

	protected override bool TryEvaluatePoling(Act2.ITreeContext context, out Vector3 direction)
	{
		direction = m_NormalizedDirection;
		return true;
	}
}
