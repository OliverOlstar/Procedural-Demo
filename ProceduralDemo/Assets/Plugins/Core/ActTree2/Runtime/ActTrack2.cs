using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Act2
{
	public enum NodeItemType
	{
		None = 0,
		Condition,
		Transition,
		Track
	}

	public interface ITimedItem
	{
		bool IsMajor();
		bool HasEndEvent();
		float GetStartTime();
		float _EditorDisplayEndTime();
		void _EditorAddSubTimes(List<float> times);
		bool HasNegativeEndTime();
		Track.EndEventType GetEndEventType();
		bool IsActive();
		Color _EditorGetColor();
	}

	[System.Serializable]
	public abstract class Track : ITimedItem, INodeItem, System.IComparable
	{
		public enum TrackType
		{
			Major = 0,
			Minor
		}

		public enum EndEventType
		{
			EndTime = 0,
			NoEndEvent,
			PositiveEndTime,
			NegativeEndTime,
		}

		// Called by Editor scripts through reflection
		public static System.Type _EditorGetContext() => typeof(IGOContext);
		public static System.Type _EditorGetEvent() => null;

		[SerializeField]
		[HideInInspector]
		private float m_StartTime = 0.0f;
		public float GetStartTime() { return m_StartTime; }

		[SerializeField]
		[HideInInspector]
		private float m_EndTime = -Core.Util.SPF30;
		public float GetEndTime()
		{
			switch (GetEndEventType())
			{
				case EndEventType.NoEndEvent:
					return m_StartTime;
				case EndEventType.NegativeEndTime:
					return -1.0f;
				default:
					return m_EndTime;
			}
		}
		public bool HasNegativeEndTime() { return GetEndTime() < 0.0f; }

		[SerializeField, HideInInspector]
		private bool m_Active = true;
		public bool IsActive() { return m_Active; }

		//public enum Breakpoint
		//{
		//	None = 0,
		//	Once,
		//	Always
		//}
		//[SerializeField]
		//private Breakpoint m_Breakpoint = Breakpoint.None;

		[SerializeField, HideInInspector]
		[FormerlySerializedAs("m_Master")]
		private bool m_Major = false;

		private float m_CurrentTimeScale = 1.0f;
		protected float CurrentTimeScale => m_CurrentTimeScale;
		private float m_DeltaTime = 0.0f;
		protected float DeltaTime => m_DeltaTime;
		private bool m_Started = false;
		public bool Started => m_Started;
		private bool m_FailedToStart = false;
		public bool FailedToStart => m_FailedToStart;
		private bool m_Ended = false;
		public bool Ended => m_Ended;
		private bool m_Interrupted = false;
		public bool Interrupted => m_Interrupted;

		internal abstract bool Initialize(IActObject tree, IActNodeRuntime node, ITreeContext context);

		public abstract bool _EditorIsValid(IActObject tree, IActNodeRuntime node, out string error);

		public abstract bool IsEventRequired(out System.Type trackEventType);

		public Track()
		{
			m_Major = GetDefaultTrackType() == TrackType.Major ? true : false;
		}

		public void StateEnter(ITreeEvent treeEvent, float timeScale)
		{
			//Debug.LogWarning((param is GOParams goParam ? goParam.GetGameObject().name + " " : "") + m_Node.GetName() + " " + GetType() + ".OnStateEnter() "  + Time.time);
			m_CurrentTimeScale = timeScale;
			m_DeltaTime = 0.0f;
			m_Started = false;
			m_FailedToStart = false;
			m_Ended = false;
			m_Interrupted = false;

			if (Core.Util.Approximately(m_StartTime, 0.0f))
			{
				Start(treeEvent);
			}
		}

		public void StateUpdate(ITreeEvent treeEvent, float time, float scaledDeltaTime, float timeScale)
		{
			//Debug.Log($"{(param is GOParams goParam ? goParam.GetGameObject().name + " " : "")} {m_Node.GetName()} {GetType()}.StateUpdate() {Time.time}\n" +
			//	$"timeScale: {timeScale} time: {time}");
			m_CurrentTimeScale = timeScale;
			m_DeltaTime = scaledDeltaTime;
			if (!m_Started && m_StartTime > 0.0f && time > m_StartTime)
			{
				Start(treeEvent);
			}

			if (!m_Started || m_Ended)
			{
				return;
			}

			EndEventType endEventType = GetEndEventType();
			bool positiveEndTime = endEventType == EndEventType.PositiveEndTime ||
				(endEventType == EndEventType.EndTime && m_EndTime >= 0.0f);
			if (positiveEndTime && time > m_EndTime)
			{
				m_Ended = true;
				OnEnd();
				return;
			}
			bool keepUpdating = OnUpdate(time - m_StartTime);
			if (keepUpdating)
			{
				return;
			}
			if (!m_Ended) // It's possible we were ended during our OnUpdate, ie. we could have disabled the object that is playing the sequencer that's running this track
			{
				m_Ended = true;
				OnEnd();
			}
		}

		public void StateExit(ITreeEvent treeEvent, bool interrupted)
		{
			//Debug.LogError((param is GOParams goParam ? goParam.GetGameObject().name + " " : "") + m_Node.GetName() + " " + GetType() + ".StateExit() " + Time.time + "\n" +
			//	" hasEnd:" + HasEndEvent() + " started:" + m_Started + " ended:" + m_Ended + " interrupt:" + interrupted + " startTime:" + m_StartTime + " endTime:" + m_EndTime);
			m_CurrentTimeScale = 1.0f;
			if (!m_Started)
			{
				if (m_StartTime > 0.0f)
				{
					// Track finished before it got a chance to start
					OnCancelled();
				}
				else
				{
					// Negative start time means we want to fire when animation ends
					Start(treeEvent);
				}
			}
			// Note: In theory if start and end time are -1 we could start and end on exit
			if (m_Started && !m_Ended)
			{
				m_Ended = true;
				if (interrupted || GetEndTime() > 0.0f)
				{
					// Track was interrupted before our end event
					m_Interrupted = true;
					OnEnd();
				}
				else
				{
					// Negative end time means we planned to end whenever the state ends
					OnEnd();
				}
			}
			OnStateExit(); // Always get this call no matter the state of the track
		}

		// Unscale duration
		protected float GetFixedDuration()
		{
			if (!HasEndEvent())
			{
				return 0.0f;
			}
			if (m_EndTime < Core.Util.EPSILON)
			{
				return -1.0f;
			}
			return m_EndTime - m_StartTime;
		}
		// Scale duration by current time scale
		protected float GetScaledDuration()
		{
			if (!HasEndEvent())
			{
				return 0.0f;
			}
			if (m_EndTime < Core.Util.EPSILON)
			{
				return -1.0f;
			}
			return 1.0f / m_CurrentTimeScale * (m_EndTime - m_StartTime);
		}

		public bool KeepSequencerAlive()
		{
			if (!IsMajor())
			{
				return false;
			}
			return !m_Ended;
		}

		public bool IsPlaying() => m_Started && !m_Ended;

		private void Start(ITreeEvent treeEvent)
		{
//#if UNITY_EDITOR
//			if (m_Breakpoint != Breakpoint.None)
//			{
//				if (m_Breakpoint == Breakpoint.Once) m_Breakpoint = Breakpoint.None;
//				Debug.Break();
//			}
//#endif
			m_Started = true;
			if (!TryStart(treeEvent))
			{
				m_Ended = true;
				m_FailedToStart = true;
				return;
			}
			OnStart();
			if (!HasEndEvent())
			{
				m_Ended = true;
			}
		}

		protected virtual bool TryStart(ITreeEvent treeEvent) { return true; }

		protected virtual void OnStart() { }

		protected virtual void OnCancelled() { }

		protected virtual bool OnUpdate(float time) { return true; }

		protected virtual void OnEnd() { }

		protected virtual void OnStateExit() { }

		public virtual bool TryHandleEvent(Params newParams) => false;

		public abstract TrackType GetDefaultTrackType();
		public abstract EndEventType GetEndEventType();

		/// <summary> This can be overriden to display the expected end time if the actual end time is set to -1. </summary>
		protected virtual float _EditorDisplayDuration() { return -1.0f; }
		public virtual void _EditorAddSubTimes(List<float> times) { }

		public bool HasEndEvent() { return GetEndEventType() != EndEventType.NoEndEvent; }

		/// <summary> This can return the start time plus the expected duration if the actual end time is -1. </summary>
		public float _EditorDisplayEndTime()
		{
			float editorDuration = _EditorDisplayDuration();
			if (HasNegativeEndTime() && editorDuration > 0.0f)
			{
				return GetStartTime() + editorDuration;
			}
			return GetEndTime();
		}

		public bool IsMajor() => m_Major;

		public int CompareTo(object obj)
		{
			ITimedItem track = obj as ITimedItem;
			if (track == null)
			{
				return 0;
			}
			return CompareTracks(this, track);
		}

		public static int CompareTracks(ITimedItem mine, ITimedItem theirs)
		{
			float myTime = mine.GetStartTime();
			float theirTime = theirs.GetStartTime();
			if (Core.Util.Approximately(myTime, theirTime)) // If start times are the same, try end times
			{
				myTime = mine._EditorDisplayEndTime();
				if (myTime < 0.0f)
				{
					myTime = float.MaxValue;
				}
				theirTime = theirs._EditorDisplayEndTime();
				if (theirTime < 0.0f)
				{
					theirTime = float.MaxValue;
				}
			}
			if (Core.Util.Approximately(myTime, theirTime)) // If we're still tied put the master first
			{
				myTime = mine.IsMajor() ? 1.0f : 0.0f;
				theirTime = theirs.IsMajor() ? 1.0f : 0.0f;
			}
			if (Core.Util.Approximately(myTime, theirTime))
			{
				return mine.GetType().Name.CompareTo(theirs.GetType().Name); // We just need some kind of deterministic way to sort
			}
			return myTime > theirTime ? 1 : -1;
		}

		public override string ToString()
		{
			string trackName = GetType().Name;
			if (trackName.EndsWith("Track"))
			{
				trackName = trackName.Substring(0, trackName.Length - 5);
			}
			return trackName;
		}

		public virtual Color _EditorGetColor()
		{
			return System.Attribute.GetCustomAttribute(GetType(), typeof(NodeItemGroupAttribute), true) is NodeItemGroupAttribute at ? 
				at.Color : 
				CoreTrackGroup.DEFAULT_COLOR;
		}
	}
}
