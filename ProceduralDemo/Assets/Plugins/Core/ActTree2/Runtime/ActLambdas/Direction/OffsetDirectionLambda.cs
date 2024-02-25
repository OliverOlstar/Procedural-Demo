
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lambdas/Direction/Offset")]
public class OffsetDirectionLambda : DirectionLambdaBase
{
	[SerializeField]
	private float m_XDegrees = 0.0f;
	[SerializeField]
	private float m_YDegrees = 0.0f;
	[SerializeField]
	private float m_ZDegrees = 0.0f;

	[SerializeField, UberPicker.AssetNonNull]
	private DirectionLambdaBase m_Direction = null;

	public override bool RequiresEvent(out System.Type eventType)
	{
		return m_Direction.RequiresEvent(out eventType);
	}

	public override bool TryEvaluate(Act2.ITreeContext context, Act2.ITreeEvent treeEvent, out Vector3 direction)
	{
		if (!m_Direction.TryEvaluate(context, treeEvent, out direction))
		{
			return false;
		}
		direction = Quaternion.Euler(m_XDegrees, m_YDegrees, m_ZDegrees) * direction;
		return true;
	}
}
