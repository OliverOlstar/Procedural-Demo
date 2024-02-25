
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	[CoreTrackGroup.Default]
	public class TimeScaleTrack : TrackGeneric<ITreeContext>
	{
		[SerializeField, Range(0.0f, 1.0f)]
		private float m_Amount = 0.5f;

		private int m_Handle = Core.TimeScaleManager.INVALID_HANDLE;

		public override TrackType GetDefaultTrackType() => TrackType.Minor;
		public override EndEventType GetEndEventType() => EndEventType.EndTime;

		protected override void OnStart()
		{
			m_Handle = Core.TimeScaleManager.StartTimeEvent(m_Amount);
		}

		protected override void OnEnd()
		{
			Core.TimeScaleManager.EndTimeEvent(m_Handle);
		}
	}
}

