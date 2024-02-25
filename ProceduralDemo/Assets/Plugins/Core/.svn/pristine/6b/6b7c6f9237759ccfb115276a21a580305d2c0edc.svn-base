using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lambdas/Direction/Lerp")]
public class LerpDirectionLambda : DirectionLambdaBase
{
	[SerializeField, UberPicker.AssetNonNull]
	private DirectionLambdaBase m_From = null;

	[SerializeField, UberPicker.AssetNonNull]
	private DirectionLambdaBase m_To = null;

	[SerializeField]
	private ActValue.Float m_LerpValue = new ActValue.Float(0.5f);

	[SerializeField]
	private bool m_Clamp = true;

	public override bool RequiresEvent(out System.Type eventType)
	{
		if (m_From.RequiresEvent(out eventType))
		{
			return true;
		}
		if (m_To.RequiresEvent(out eventType))
		{
			return true;
		}
		if (m_LerpValue.RequiresEvent(out eventType))
		{
			return true;
		}
		return false;
	}

	public override bool TryEvaluate(Act2.ITreeContext context, Act2.ITreeEvent treeEvent, out Vector3 direction)
	{
		bool fromValid = m_From.TryEvaluate(context, treeEvent, out Vector3 from);
		bool toValid = m_To.TryEvaluate(context, treeEvent, out Vector3 to);
		if (!fromValid && !toValid)
		{
			direction = Vector3.forward;
			return false;
		}
		if (!fromValid)
		{
			direction = to;
			return true;
		}
		if (!toValid)
		{
			direction = from;
			return true;
		}
		float lerp = m_LerpValue.GetValue(context, treeEvent);
		direction = m_Clamp ?
			Vector3.Slerp(from, to, lerp) :
			Vector3.SlerpUnclamped(from, to, lerp);
		return true;
	}
}
