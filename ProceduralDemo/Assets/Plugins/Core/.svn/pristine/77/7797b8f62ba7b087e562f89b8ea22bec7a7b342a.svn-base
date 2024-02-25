using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Act2
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[CoreTrackGroup.Default]
	public class WaitRandomTrack : TrackEvent<ITreeEvent, ITreeContext>
	{
		[SerializeField]
		private ActValue.Int m_MinDurationFrames = new ActValue.Int(30);
		[SerializeField]
		private ActValue.Int m_MaxDurationFrames = new ActValue.Int(30);

		private float m_Duration = 1.0f;

		public override TrackType GetDefaultTrackType() => TrackType.Major;
		public override EndEventType GetEndEventType() => EndEventType.NegativeEndTime;

		public override bool IsEventRequired(out System.Type eventType)
		{
			if (m_MinDurationFrames.RequiresEvent(out eventType))
			{
				return true;
			}
			if (m_MaxDurationFrames.RequiresEvent(out eventType))
			{
				return true;
			}
			return false;
		}

		protected override void OnStart()
		{
			int minFrames = m_MinDurationFrames.GetValue(m_Context, m_Event);
			int maxFrames = m_MaxDurationFrames.GetValue(m_Context, m_Event);
			float minTime = Core.Util.FramesToSeconds(minFrames);
			float maxTime = Core.Util.FramesToSeconds(maxFrames);
			m_Duration = Random.Range(minTime, maxTime);
		}

		protected override bool OnUpdate(float time)
		{
			return time < m_Duration;
		}
	}
}
