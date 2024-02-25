
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Act2
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[CoreTrackGroup.ActVar]
	public class ActVarSetTrack : TrackEvent<ITreeEvent, IGOContext>
	{
		public enum Source
		{
			Local = 0,
			Global
		}

		[SerializeField]
		private Source m_Source = Source.Local;

		[SerializeField, UberPicker.AssetNonNull]
		private SOActVar m_Var = null;

		[SerializeField]
		private ActValue.Int m_Value = new ActValue.Int(0);

		private ActVarBehaviour m_VarBehaviour = null;

		public override TrackType GetDefaultTrackType() => TrackType.Minor;
		public override EndEventType GetEndEventType() => EndEventType.NoEndEvent;

		public override bool IsEventRequired(out System.Type eventType) => m_Value.RequiresEvent(out eventType);

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
			m_VarBehaviour.SetVarValue(m_Var, m_Value.GetValue(m_Context, m_Event));
		}
	}
}
