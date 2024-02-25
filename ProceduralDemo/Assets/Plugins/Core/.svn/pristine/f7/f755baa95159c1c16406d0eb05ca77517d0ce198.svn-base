using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Act2
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[ConditionGroup.Core]
	public class FloatCondition : ConditionGeneric<ITreeContext>
	{
		public enum Operator
		{
			GreaterThan = 0,
			GreaterThanEquals,
			LessThan,
			LessThanEquals,
			Approximately,
		}

		[SerializeField]
		private ActValue.Float m_ValueA = new ActValue.Float(0.0f);

		[SerializeField]
		private Operator m_Operator = Operator.GreaterThan;

		[SerializeField]
		private ActValue.Float m_ValueB = new ActValue.Float(0.0f);

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
			float a = m_ValueA.GetValue(m_Context, treeEvent);
			float b = m_ValueB.GetValue(m_Context, treeEvent);
			switch (m_Operator)
			{
				case Operator.GreaterThan:
					return a > b;
				case Operator.GreaterThanEquals:
					return a > b - Core.Util.EPSILON;
				case Operator.LessThan:
					return a < b;
				case Operator.LessThanEquals:
					return a < b + Core.Util.EPSILON;
				default:
					return Core.Util.Approximately(a, b);
			}
		}

		public override string ToString()
		{
			string op = string.Empty;
			switch (m_Operator)
			{
				case Operator.Approximately:
					op = "=";
					break;
				case Operator.GreaterThan:
					op = ">";
					break;
				case Operator.GreaterThanEquals:
					op = ">=";
					break;
				case Operator.LessThan:
					op = "<";
					break;
				case Operator.LessThanEquals:
					op = "<=";
					break;
			}
			return Core.Str.Build("Float(", m_ValueA.ToString(), " ", op, " ", m_ValueB.ToString(), ")");
		}
	}
}
