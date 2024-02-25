
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Act2
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[ConditionGroup.Logic]
	public class MaybeCondition : ConditionPoling<ITreeContext>
	{
		[SerializeField]
		[Core.Percent]
		float m_Percent = 0.5f;

		protected override bool EvaluatePoling()
		{
			return Random.value < m_Percent;
		}

		public override string ToString()
		{
			return Core.Str.Build("Maybe(", m_Percent.ToString("P0"), ")");
		}
	}
}
