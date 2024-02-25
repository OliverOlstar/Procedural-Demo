
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Act2
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[CoreTrackGroup.ActVar]
	public class ActVarMathTrack : TrackEvent<ITreeEvent, IGOContext>
	{
		public enum Source
		{
			Local = 0,
			Global
		}

		[System.Serializable]
		public class Operation
		{
			[SerializeField, UberPicker.AssetNonNull]
			public SOActVar Var = null;
			[SerializeField]
			public SOActVar.Operator Operator = SOActVar.Operator.Add;
			[SerializeField]
			public ActValue.Int Value = new ActValue.Int(1);
		}

		[SerializeField]
		private Source m_Source = Source.Local;

		[SerializeField, Core.Flatten]
		private Operation m_OnStart = new Operation();

		[SerializeField]
		private bool m_HasOnEnd = false;

		[SerializeField, Core.Conditional(nameof(m_HasOnEnd), true, flattenClass: true)]
		private Operation m_OnEnd = new Operation { Operator = SOActVar.Operator.Subtract };

		private ActVarBehaviour m_VarBehaviour = null;

		public override TrackType GetDefaultTrackType() => TrackType.Minor;
		public override EndEventType GetEndEventType() => m_HasOnEnd ? EndEventType.EndTime : EndEventType.NoEndEvent;

		public override bool IsEventRequired(out System.Type eventType)
		{
			if (m_OnStart.Value.RequiresEvent(out eventType))
			{
				return true;
			}
			if (m_HasOnEnd && m_OnEnd.Value.RequiresEvent(out eventType))
			{
				return true;
			}
			eventType = null;
			return false;
		}

		protected override bool OnInitialize()
		{
			switch (m_Source)
			{
				case Source.Global:
					m_VarBehaviour = GlobalActVarBehaviour.GetOrCreate();
					break;
				default:
					m_VarBehaviour = ActVarBehaviour.Get(m_Context.GameObject);
					break;
			}
			return true;
		}

		protected override void OnStart()
		{
			int value = m_OnStart.Value.GetValue(m_Context, m_Event);
			m_VarBehaviour.VarMath(m_OnStart.Var, m_OnStart.Operator, value);
		}

		protected override void OnEnd()
		{
			int value = m_OnEnd.Value.GetValue(m_Context, m_Event);
			m_VarBehaviour.VarMath(m_OnEnd.Var, m_OnEnd.Operator, value);
		}
	}
}
