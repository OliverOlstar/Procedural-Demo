using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Act2
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[CoreTrackGroup.Default]
	public class WaitVariableTrack : TrackEvent<ITreeEvent, ITreeContext>
	{
		[SerializeField]
		private ActValue.Int m_DurationFrames = new ActValue.Int(30);

		private float m_Duration = 1.0f;

		public override TrackType GetDefaultTrackType() => TrackType.Major;
		public override EndEventType GetEndEventType() => EndEventType.NegativeEndTime;

		public override bool IsEventRequired(out System.Type eventType)
		{
			return m_DurationFrames.RequiresEvent(out eventType);
		}

		protected override void OnStart()
		{
			int frames = m_DurationFrames.GetValue(m_Context, m_Event);
			m_Duration = Core.Util.FramesToSeconds(frames);
		}

		protected override bool OnUpdate(float time)
		{
			return time < m_Duration;
		}
	}
}

