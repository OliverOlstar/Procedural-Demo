
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lambdas/Float/Position Component")]
public class PositionComponentFloatLambda : FloatLambdaBase
{
	public enum Component
	{
		X = 0,
		Y,
		Z
	}

	[SerializeField, UberPicker.AssetNonNull]
	BasePositionLambda m_Position = null;
	[SerializeField]
	Component m_Component = Component.X;

	public override bool RequiresEvent(out System.Type eventType)
	{
		if (m_Position != null && m_Position.RequiresEvent(out eventType))
		{
			return true;
		}
		eventType = null;
		return false;
	}

	public override bool TryEvaluate(Act2.ITreeContext context, Act2.ITreeEvent treeEvent, out float value)
	{
		if (!m_Position.TryEvaluate(context, treeEvent, out Vector3 position))
		{
			value = 0.0f;
			return false;
		}
		switch (m_Component)
		{
			case Component.X:
				value = position.x;
				break;
			case Component.Y:
				value = position.y;
				break;
			default:
				value = position.z;
				break;
		}
		return true;
	}
}
