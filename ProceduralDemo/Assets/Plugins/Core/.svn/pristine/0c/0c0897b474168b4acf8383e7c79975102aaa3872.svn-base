using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Act2
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[CoreTrackGroup.Debug]
	public class DebugPauseTrack : TrackGeneric<IGOContext>
	{
		public override TrackType GetDefaultTrackType() => TrackType.Minor;
		public override EndEventType GetEndEventType() => EndEventType.NoEndEvent;

#if UNITY_EDITOR
		protected override void OnStart()
		{
			Debug.LogWarning($"DebugPauseTrack.OnStart() Paused Editor from node '{m_Node}' in tree '{m_Tree.name}'");
			UnityEditor.EditorApplication.isPaused = true;
		}
#endif
	}
}
