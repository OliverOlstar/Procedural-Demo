
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Act2
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[CoreTrackGroup.Default]
	public class WaitTrack : TrackGeneric<ITreeContext>
	{
		public override TrackType GetDefaultTrackType() => TrackType.Major;
		public override EndEventType GetEndEventType() => EndEventType.EndTime;
	}
}
