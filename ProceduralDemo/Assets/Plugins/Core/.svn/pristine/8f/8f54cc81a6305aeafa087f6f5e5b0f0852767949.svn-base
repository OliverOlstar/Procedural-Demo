using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Act2
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[ConditionGroup.Core]
	public class BoolCondition : ConditionGeneric<ITreeContext>
	{
		public enum Operator
		{
			Equal = 0,
			NotEqual
		}

		[SerializeField]
		private ActValue.Bool m_ValueA = new ActValue.Bool(true);

		[SerializeField]
		private Operator m_Operator = Operator.Equal;

		[SerializeField]
		private ActValue.Bool m_ValueB = new ActValue.Bool(true);

		public override bool IsEventRequired(out System.Type eventType)
		{
			if (m_ValueA.RequiresEvent(out eventType))
			{
				return true;
			}
			if (m_ValueB.RequiresEvent(out eventType))
			{
				return true;
			}
			return false;
		}

		protected override bool OnEvaluate(ITreeEvent treeEvent)
		{
			bool a = m_ValueA.GetValue(m_Context, treeEvent);
			bool b = m_ValueB.GetValue(m_Context, treeEvent);
			switch (m_Operator)
			{
				case Operator.NotEqual:
					return a != b;
				default:
					return a == b;
			}
		}

		public override string ToString()
		{
			string op = string.Empty;
			switch (m_Operator)
			{
				case Operator.Equal:
					op = "=";
					break;
				case Operator.NotEqual:
					op = "!=";
					break;
			}
			return Core.Str.Build("Bool(", m_ValueA.ToString(), " ", op, " ", m_ValueB.ToString(), ")");
		}
	}
}
