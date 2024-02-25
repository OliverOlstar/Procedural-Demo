
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Act2
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[ConditionGroup.Logic]
	public class NotCondition : ConditionPoling<ITreeContext>
	{
		// This is a special condition that never actually get evaluated
		protected override bool EvaluatePoling() => throw new System.NotImplementedException("NotCondition.EvaluatePoling() Should never be called");

		public override string ToString() => "Not";
	}
}
