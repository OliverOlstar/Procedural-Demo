
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Act2
{
	[CoreTrackGroup.Default]
	public class EnableChildObjectTrack : TrackGeneric<IGOContext>
	{
		[SerializeField]
		private string m_ObjectName = string.Empty;

		private enum OnStartState
		{
			Disabled = 0,
			Enabled,
		}
		[SerializeField]
		private OnStartState m_OnStart = OnStartState.Enabled;

		private enum OnEndState
		{
			DoNothing = 0,
			Disabled,
			Enabled,
		}
		[SerializeField]
		private OnEndState m_OnEnd = OnEndState.Disabled;

		private Transform m_Child = null;

		public override TrackType GetDefaultTrackType() => TrackType.Minor;
		public override EndEventType GetEndEventType() => m_OnEnd == OnEndState.DoNothing ? 
			EndEventType.NoEndEvent :
			EndEventType.EndTime;

		protected override bool OnInitialize()
		{
			m_Child = Core.Util.FindInTransformChildren(m_Context.Transform, m_ObjectName);
			return m_Child != null;
		}

		protected override void OnStart()
		{
			switch (m_OnStart)
			{
				case OnStartState.Disabled:
					m_Child.gameObject.SetActive(false);
					break;
				case OnStartState.Enabled:
					m_Child.gameObject.SetActive(true);
					break;
			}
		}

		protected override void OnEnd()
		{
			switch (m_OnEnd)
			{
				case OnEndState.Disabled:
					m_Child.gameObject.SetActive(false);
					break;
				case OnEndState.Enabled:
					m_Child.gameObject.SetActive(true);
					break;
			}
		}
	}
}
