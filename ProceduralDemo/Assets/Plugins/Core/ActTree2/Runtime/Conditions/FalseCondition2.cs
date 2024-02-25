
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Act2
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[ConditionGroup.Logic]
	public class FalseCondition : ConditionPoling<ITreeContext>
	{
		protected override bool EvaluatePoling() => false;

		public override string ToString() => "False";
	}
}
