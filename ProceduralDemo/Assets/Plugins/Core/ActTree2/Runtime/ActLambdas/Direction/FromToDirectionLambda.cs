
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lambdas/Direction/FromTo")]
public class FromToDirectionLambda : DirectionLambdaBase
{
	private enum Mode
	{
		XYZ,
		XZ
	}

	[SerializeField, UberPicker.AssetNonNull]
	private BasePositionLambda m_From = null;

	[SerializeField, UberPicker.AssetNonNull]
	private BasePositionLambda m_To = null;

	[SerializeField]
	private Mode m_Mode = Mode.XYZ;

	public override bool RequiresEvent(out System.Type eventType)
	{
		if (m_From != null && m_From.RequiresEvent(out eventType))
		{
			return true;
		}
		if (m_To != null && m_To.RequiresEvent(out eventType))
		{
			return true;
		}
		eventType = null;
		return false;
	}

	public override bool TryEvaluate(Act2.ITreeContext context, Act2.ITreeEvent treeEvent, out Vector3 direction)
	{
		if (!m_From.TryEvaluate(context, treeEvent, out Vector3 from))
		{
			direction = Vector3.forward;
			return false;
		}
		if (!m_To.TryEvaluate(context, treeEvent, out Vector3 to))
		{
			direction = Vector3.forward;
			return false;
		}
		direction = to - from;
		if (Core.Util.IsVectorZero(direction))
		{
			Debug.LogWarning($"FromToDirectionLambda.TryEvaluate() '{name}' couldn't evaluate direction " +
				$"because positions from '{m_From.name}' and to '{m_To.name}' are the same. This result is probably unexpected?");
			direction = Vector3.forward;
			return false;
		}
		switch (m_Mode)
		{
			case Mode.XZ:
				Core.Util.NormalizeXZ(ref direction);
				break;
			default:
				Core.Util.Normalize(ref direction);
				break;
		}
		return true;
	}
}
