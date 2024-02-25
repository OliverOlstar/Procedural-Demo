
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lambdas/Direction/Clamped")]
public class ClampDirectionLambda : DirectionLambdaBase
{
	[SerializeField, UberPicker.AssetNonNull]
	private DirectionLambdaBase m_BaseDirection = null;

	[SerializeField, UberPicker.AssetNonNull]
	private DirectionLambdaBase m_ArcDirection = null;

	[SerializeField]
	private ActValue.Float m_ArcDegrees = new ActValue.Float(90.0f);

	public override bool TryEvaluate(Act2.ITreeContext context, Act2.ITreeEvent treeEvent, out Vector3 returnDirection)
	{
		if (!m_BaseDirection.TryEvaluate(context, treeEvent, out Vector3 direction))
		{
			returnDirection = Vector3.forward;
			return false;
		}

		if (!m_ArcDirection.TryEvaluate(context, treeEvent, out Vector3 arcDirection))
		{
			returnDirection = Vector3.forward;
			return false;
		}
		float degrees = m_ArcDegrees.GetValue(context, treeEvent);
		float halfArc = 0.5f * degrees;

		float cos = Core.Util.Cos(halfArc);
		float dot = Vector3.Dot(direction, arcDirection);
		if (dot > cos) // Direction is within arc
		{
			returnDirection = direction;
			return true;
		}

		// Clamp direction to arc
		Vector3 clampedDir = Vector3.RotateTowards(arcDirection, direction, Mathf.Deg2Rad * halfArc, 0.0f);
		returnDirection = clampedDir;
		return true;
	}

	public override bool RequiresEvent(out System.Type eventType)
	{
		if (m_ArcDegrees.RequiresEvent(out eventType))
		{
			return true;
		}
		if (m_BaseDirection != null && m_BaseDirection.RequiresEvent(out eventType))
		{
			return true;
		}
		if (m_ArcDirection != null && m_ArcDirection.RequiresEvent(out eventType))
		{
			return true;
		}
		eventType = null;
		return false;
	}
}
