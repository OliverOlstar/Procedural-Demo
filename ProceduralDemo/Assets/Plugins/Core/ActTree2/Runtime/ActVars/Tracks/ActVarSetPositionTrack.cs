
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Act2
{
	[CoreTrackGroup.ActVar]
	public class ActVarSetPositionTrack : TrackEvent<ITreeEvent, IGOContext>
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

		[SerializeField, UberPicker.AssetNonNull]
		private BasePositionLambda m_Position = null;

		private ActVarBehaviour m_VarBehaviour = null;

		public override TrackType GetDefaultTrackType() => TrackType.Minor;
		public override EndEventType GetEndEventType() => EndEventType.NoEndEvent;

		public override bool IsEventRequired(out System.Type eventType)
		{
			if (m_Position == null)
			{
				eventType = null;
				return false;
			}
			return m_Position.RequiresEvent(out eventType);
		}

		protected override string OnValidate()
		{
			if (m_Var == null)
			{
				return "Var cannot be null";
			}
			if (m_Position == null)
			{
				return "Position cannot be null";
			}
			return null;
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

		protected override bool TryStart(ITreeEvent treeEvent)
		{
			if (!base.TryStart(treeEvent))
			{
				return false;
			}
			if (!m_Position.TryEvaluate(m_Context, treeEvent, out Vector3 direction))
			{
				return false;
			}
			m_VarBehaviour.SetVector(m_Var, direction);
			return true;
		}
	}
}
