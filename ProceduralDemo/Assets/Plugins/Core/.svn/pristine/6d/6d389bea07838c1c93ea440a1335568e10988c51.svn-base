using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lambdas/Direction/First Valid")]
public class FirstValidDirectionLambda : DirectionLambdaBase
{
	[System.Serializable]
	public class Directions : Core.ReorderableListGeneric<DirectionLambdaBase>
	{
		[SerializeField, UberPicker.AssetNonNull]
		private List<DirectionLambdaBase> m_List = new List<DirectionLambdaBase> { null, null };
		protected override List<DirectionLambdaBase> List => m_List;
	}

#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.DrawWithUnity]
#endif
	[SerializeField, Tooltip("Queries the list in order and returns the first direction that is valid")]
	private Directions m_Directions = new Directions();
	
	public override bool RequiresEvent(out System.Type eventType)
	{
		for (int i = 0; i < m_Directions.Count; i++)
		{
			if (m_Directions[i].RequiresEvent(out eventType))
			{
				return true;
			}
		}
		eventType = null;
		return false;
	}

	public override bool TryEvaluate(Act2.ITreeContext context, Act2.ITreeEvent treeEvent, out Vector3 direction)
	{
		for (int i = 0; i < m_Directions.Count; i++)
		{
			DirectionLambdaBase lambda = m_Directions[i];
			if (lambda.TryEvaluate(context, treeEvent, out direction))
			{
				return true;
			}
		}
		direction = Vector3.forward;
		return false;
	}
}
