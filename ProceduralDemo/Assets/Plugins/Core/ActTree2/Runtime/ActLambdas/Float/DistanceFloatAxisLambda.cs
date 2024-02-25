
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lambdas/Float/DistanceAxis")]
public class DistanceFloatAxisLambda : FloatLambdaBase
{
	[SerializeField][UberPicker.AssetNonNull]
	BasePositionLambda m_Position1 = null;
	[SerializeField][UberPicker.AssetNonNull]
	BasePositionLambda m_Position2 = null;
	[SerializeField]
	DistanceAxis m_Axis = DistanceAxis.XAxis;
	[SerializeField]
	ResultType m_ResultType = ResultType.Absolute;

	private enum DistanceAxis
    {
		XAxis,
		YAxis,
		ZAxis
    }
	private enum ResultType
	{
		Relative,
		Absolute,
	}

	public override bool RequiresEvent(out System.Type eventType)
	{
		// Note: If 1 and 2 require different event types we're going to have an issue, 
		// ideally we want to handle this case somehow even though it's nonsense scripting
		if (m_Position1.RequiresEvent(out eventType))
		{
			return true;
		}
		if (m_Position2.RequiresEvent(out eventType))
		{
			return true;
		}
		return false;
	}

	public override bool TryEvaluate(Act2.ITreeContext context, Act2.ITreeEvent treeEvent, out float value)
	{
		if (!m_Position1.TryEvaluate(context, treeEvent, out Vector3 position1))
		{
			value = 0.0f;
			return false;
		}
		if (!m_Position2.TryEvaluate(context, treeEvent, out Vector3 position2))
		{
			value = 0.0f;
			return false;
		}
		value = GetDistanceOnAxis(position1, position2);
		return true;
	}

	private float GetDistanceOnAxis(Vector3 position1, Vector3 position2)
    {
		Vector3 direction = Vector3.zero;
		switch (m_Axis)
        {
			case DistanceAxis.XAxis:
				direction = Vector3.right;
				break;
			case DistanceAxis.YAxis:
				direction = Vector3.up;
				break;
			case DistanceAxis.ZAxis:
				direction = Vector3.forward;
				break;
        }

		float distance = Vector3.Dot(direction, position2 - position1);
		if(m_ResultType == ResultType.Relative)
        {
			return distance;
        }

		return distance < 0 ? -distance : distance;
	}
}
