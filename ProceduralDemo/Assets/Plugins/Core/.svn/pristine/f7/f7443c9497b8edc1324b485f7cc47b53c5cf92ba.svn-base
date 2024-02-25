
using UnityEngine;

public class MaybeCondition : ActCondition
{
	[SerializeField][Core.Percent]
	float m_Percent = 0.5f;

	public override bool Evaluate(ActParams param)
	{
		return Random.value < m_Percent;
	}

	public override string ToString()
	{
		return Core.Str.Build("MaybeCondition(", m_Percent.ToString("P0"), ")");
	}
}
