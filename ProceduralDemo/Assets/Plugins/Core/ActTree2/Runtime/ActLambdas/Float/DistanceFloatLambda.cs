
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lambdas/Float/Distance")]
public class DistanceFloatLambda : FloatLambdaBase
{
	public enum CheckType
	{
		XZ = 0,
		XYZ,
		Y
	}

	[SerializeField][UberPicker.AssetNonNull]
	BasePositionLambda m_Position1 = null;
	[SerializeField][UberPicker.AssetNonNull]
	BasePositionLambda m_Position2 = null;
	[SerializeField]
	CheckType m_CheckType = CheckType.XZ;

	public override bool RequiresEvent(out System.Type eventType)
	{
		// Note: If 1 and 2 require different event types we're going to have an issue, 
		// ideally we want to handle this case somehow even though it's nonsense scripting
		if (m_Position1 != null && m_Position1.RequiresEvent(out eventType))
		{
			return true;
		}
		if (m_Position2 != null && m_Position2.RequiresEvent(out eventType))
		{
			return true;
		}
		eventType = null;
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
		switch (m_CheckType)
		{
			case CheckType.XZ:
				value = Core.Util.DistanceXZ(position1, position2);
				break;
			case CheckType.Y:
				value = Mathf.Abs(position1.y - position2.y);
				break;
			default:
				value = Vector3.Distance(position1, position2);
				break;
		}
		return true;
	}
}
