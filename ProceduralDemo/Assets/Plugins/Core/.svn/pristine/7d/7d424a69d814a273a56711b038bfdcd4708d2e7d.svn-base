
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Act2
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[ConditionGroup.Core]
	public class AngleCondition : ConditionGeneric<ITreeContext>
	{
		public enum Operator
		{
			GreaterThan = 0,
			LessThan,
			Approximately,
		}

		[SerializeField, UberPicker.AssetNonNull]
		private DirectionLambdaBase m_From = null;
		[SerializeField, UberPicker.AssetNonNull]
		private DirectionLambdaBase m_To = null;

		[SerializeField]
		private Operator m_Operator = Operator.GreaterThan;

		private enum AngleType
		{
			Arc = 0,
			SignedAngle,
			AbsoluteAngle,
		}

		[SerializeField]
		private AngleType m_AngleType = AngleType.Arc;

		[SerializeField]
		private ActValue.Float m_Degrees = new ActValue.Float(90.0f);

		[Space]
		[SerializeField]
		private bool m_DebugLog;

		public override bool IsEventRequired(out System.Type eventType)
		{
			if (m_From != null && m_From.RequiresEvent(out eventType))
			{
				return true;
			}
			if (m_To != null && m_To.RequiresEvent(out eventType))
			{
				return true;
			}
			if (m_Degrees.RequiresEvent(out eventType))
			{
				return true;
			}
			return false;
		}

		protected override void OnInitialize(ref bool? failedToInitializeReturnValue)
		{
			if (m_From == null || m_To == null)
			{
				failedToInitializeReturnValue = true;
			}
		}

		protected override bool OnEvaluate(ITreeEvent treeEvent)
		{
			if (!m_From.TryEvaluate(m_Context, treeEvent, out Vector3 from))
			{
				return true; // I guess return true because that's what we do when failing to initialize?
			}
			if (!m_To.TryEvaluate(m_Context, treeEvent, out Vector3 to))
			{
				return true; // I guess return true because that's what we do when failing to initialize?
			}
			float degrees = m_Degrees.GetValue(m_Context, treeEvent);

			switch (m_AngleType)
			{
				case AngleType.SignedAngle:
					float angle = Vector3.SignedAngle(from, to, Vector3.up);
#if !RELEASE
					if (m_DebugLog)
					{
						Debug.Log(m_Node + " " + this + " " + Time.time + "\n" + 
							"angle: " + angle + " " + OperatorToString(m_Operator) + " degrees: " + degrees + "\n" +
							"v1: " + Core.DebugUtil.VectorToString(from) + " v2: " + Core.DebugUtil.VectorToString(to));
					}
#endif
					switch (m_Operator)
					{
						case Operator.GreaterThan:
							return angle > degrees;
						case Operator.LessThan:
							return angle < degrees;
						default:
							return Core.Util.Approximately(angle, degrees);
					}
				default:
					degrees = Mathf.Abs(degrees);
					if (m_AngleType == AngleType.Arc)
					{
						degrees *= 0.5f;
					}
					float cos = Core.Util.Cos(degrees);
					float dot = Vector3.Dot(from, to);
#if !RELEASE
					if (m_DebugLog)
					{
						Debug.Log(m_Node + " " + this + " " + Time.time + "\n" + 
							"angle: " + Core.Util.ACos(dot) + " " + OperatorToString(m_Operator) + " degrees: " + degrees + "\n" +
							"v1: " + Core.DebugUtil.VectorToString(from) + " v2: " + Core.DebugUtil.VectorToString(to));
					}
#endif
					// Note: Because we are comparing cos not angle operator is reversed
					switch (m_Operator)
					{
						case Operator.GreaterThan:
							return dot < cos;
						case Operator.LessThan:
							return dot > cos;
						default:
							return Core.Util.Approximately(dot, cos);
					}
			}
		}

		private static string OperatorToString(Operator op)
		{
			switch (op)
			{
				case Operator.GreaterThan:
					return ">";
				case Operator.LessThan:
					return "<";
				default:
					return "=";
			}
		}

		private static string AngleTypeToString(AngleType angleType)
		{
			switch (angleType)
			{
				case AngleType.Arc:
					return " Arc ";
				case AngleType.AbsoluteAngle:
					return " (+/-) ";
				default:
					return " ";
			}
		}

		public override string ToString()
		{
			return Core.Str.Build(
				"Angle(",
				m_From == null ? "null" : m_From.name, " to ",
				m_To == null ? "null" : m_To.name, " ",
				OperatorToString(m_Operator),
				AngleTypeToString(m_AngleType),
				m_Degrees.ToString());
		}
	}
}
