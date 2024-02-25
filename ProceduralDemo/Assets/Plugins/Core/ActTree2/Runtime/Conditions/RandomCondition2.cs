
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Act2
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[ConditionGroup.Logic]
	public class RandomCondition : ConditionPoling<ITreeContext>
	{
		[SerializeField]
		float m_Weight = 10.0f;
		public float GetWeight() => m_Weight;

		// This is a special condition that never actually get evaluated
		protected override bool EvaluatePoling() => throw new System.NotImplementedException("RandomCondition.EvaluatePoling() Should never be called");

		public override string ToString() => $"Random({m_Weight})";
	}
}
