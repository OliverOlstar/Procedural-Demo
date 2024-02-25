
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Act2
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[CoreTrackGroup.ActVar]
	public class ActVarLerpTrack : TrackEvent<ITreeEvent, IGOContext>
	{
		public enum Source
		{
			Local = 0,
			Global
		}

		[SerializeField]
		private Source m_Source = Source.Local;

		[SerializeField]
		[UberPicker.AssetNonNull]
		private SOActVar m_Var = null;

		[SerializeField]
		private ActValue.Float m_From = new ActValue.Float(0.0f);

		[SerializeField]
		private ActValue.Float m_To = new ActValue.Float(1.0f);

		private ActVarBehaviour m_VarBehaviour = null;

		public override TrackType GetDefaultTrackType() => TrackType.Minor;
		public override EndEventType GetEndEventType() => EndEventType.PositiveEndTime;

		public override bool IsEventRequired(out System.Type eventType)
		{
			if (m_From.RequiresEvent(out eventType))
			{
				return true;
			}
			if (m_To.RequiresEvent(out eventType))
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
			float from = m_From.GetValue(m_Context, m_Event);
			m_VarBehaviour.SetVarValue(m_Var, from);
		}

		protected override bool OnUpdate(float time)
		{
			float from = m_From.GetValue(m_Context, m_Event);
			float to = m_To.GetValue(m_Context, m_Event);
			float value = Mathf.Lerp(from, to, time / GetScaledDuration());
			m_VarBehaviour.SetVarValue(m_Var, value);
			return true;
		}

		protected override void OnEnd()
		{
			if (!Interrupted)
			{
				float to = m_To.GetValue(m_Context, m_Event);
				m_VarBehaviour.SetVarValue(m_Var, to);
			}
		}
	}
}
