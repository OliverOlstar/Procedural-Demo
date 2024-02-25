using System.Collections.Generic;
using UnityEngine;

namespace Act
{
	public interface ITrack
	{
		int GetNodeID();
		bool IsMaster();
		bool HasEndEvent();
		float GetStartTime();
		float _EditorDisplayEndTime();
		void _EditorAddSubTimes(List<float> times);
		bool HasNegativeEndTime();
		ActTrack.EndEventType GetEndEventType();
		bool IsActive();
		Color _EditorGetColor();
	}

	public class TrackGroupAttribute : System.Attribute
	{
		public string GroupName;

		public TrackGroupAttribute(string groupName)
		{
			GroupName = groupName;
		}
	}

	public static class CoreTrackGroup
	{
		public const string Debug = "Debug";
	}

}

public class ActTrack : ScriptableObject, Act.ITrack, System.IComparable
{
	public enum EndEventType
	{
		EndTime = 0,
		NoEndEvent,
		PositiveEndTime,
		NegativeEndTime,
	}

	public static int GetContextMask() { return 0x1; }

	[SerializeField][HideInInspector]
	float m_StartTime = 0.0f;
	public float GetStartTime() { return m_StartTime; }

	[SerializeField][HideInInspector]
	float m_EndTime = -Core.Util.SPF30;
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

	[SerializeField][HideInInspector]
	int m_NodeID = 0;
	public int GetNodeID() { return m_NodeID; }
#if UNITY_EDITOR
	public void EditorSetNodeID(int nodeID) { m_NodeID = nodeID; EditorUpdateName(); } // Update name from node ID
	// HACK: Unity has a bug where nested objects are sorted by name and then the first object in the sort order becomes the root
	// This makes it impossible to duplicate a tree unless we guarentee the tree's name always sorts to the top
	// Prefixing with "zz" should guarentee this is always the case
	public void EditorUpdateName() { name = $"zzNode{m_NodeID}_{GetType().Name}"; }
#endif

	[SerializeField]
	bool m_Active = true;
	public bool IsActive() { return m_Active; }

	public virtual void _EditorAddSubTimes(List<float> times) { }

	[SerializeField][Core.ShowIf("AllowMasterOrSlave")]
	bool m_Master = false;

	protected ActTreeRT m_Tree = null;
	protected ActNodeRT m_Node = null;

	float m_CurrentTimeScale = 1.0f;
	protected float CurrentTimeScale => m_CurrentTimeScale;
	float m_DeltaTime = 0.0f;
	protected float DeltaTime => m_DeltaTime;
	bool m_Started = false;
	bool m_Ended = false;

	public bool Initialize(ActTreeRT tree, ActNodeRT node, ActParams actParams)
	{
		m_Tree = tree;
		m_Node = node;
		return OnInitialize(actParams);
	}

	public void StateEnter(float timeScale, ActParams param)
	{
		//Debug.LogWarning((param is GOParams goParam ? goParam.GetGameObject().name + " " : "") + m_Node.GetName() + " " + GetType() + ".OnStateEnter() "  + Time.time);
		m_CurrentTimeScale = timeScale;
		m_DeltaTime = 0.0f;
		m_Started = false;
		m_Ended = false;

		if (Core.Util.Approximately(m_StartTime, 0.0f))
		{
			m_Started = true;
			if (TryStart(param))
			{
				OnStart();
			}
			else
			{
				m_Ended = true;
			}
		}
	}

	public void StateUpdate(float timeScale, float scaledDeltaTime, float time, ActParams param)
	{
		//Debug.Log($"{(param is GOParams goParam ? goParam.GetGameObject().name + " " : "")} {m_Node.GetName()} {GetType()}.StateUpdate() {Time.time}\n" +
		//	$"timeScale: {timeScale} time: {time}");
		m_CurrentTimeScale = timeScale;
		m_DeltaTime = scaledDeltaTime;
		if (!m_Started && m_StartTime > 0.0f && time > m_StartTime)
		{
			m_Started = true;
			if (TryStart(param))
			{
				OnStart();
			}
			else
			{
				m_Ended = true;
			}
		}

		EndEventType endEventType = GetEndEventType();
		if (endEventType == EndEventType.NoEndEvent || !m_Started || m_Ended)
		{
			return;
		}
		
		bool positiveEndTime = endEventType == EndEventType.PositiveEndTime 
			|| (endEventType == EndEventType.EndTime && m_EndTime >= 0.0f);
		if (positiveEndTime && time > m_EndTime)
		{
			m_Ended = true;
			OnEnd();
		}
		else if (!OnUpdate(time - m_StartTime))
		{
			m_Ended = true;
			OnEnd();
		}
	}

	public void StateExit(ActParams param, bool interrupted)
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
				m_Started = true;
				if (TryStart(param))
				{
					OnStart();
				}
				else
				{
					m_Ended = true;
				}
			}
		}
		// Note: In theory if start and end time are -1 we could start and end on exit
		if (HasEndEvent() && m_Started && !m_Ended)
		{
			if (interrupted || GetEndTime() > 0.0f)
			{
				// Track was interrupted before our end event
				OnInterrupted();
			}
			else
			{
				// Negative end time means we planned to end whenever the state ends
				m_Ended = true;
				OnEnd();
			}
		}

		OnStateExit(); // Always get this call no matter the state of the track
	}

	[System.Obsolete] // These functions now have more specific names
	protected float GetDuration() { return GetScaledDuration(); }
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

	public bool Alive()
	{
		if (!IsMaster())
		{
			return false;
		}

		return !m_Started || (!m_Ended && HasEndEvent());
	}

	protected virtual bool OnInitialize(ActParams actParams) { return true; }

	protected virtual bool TryStart(ActParams actParams) { return true; }

	protected virtual void OnStart() {}

	protected virtual void OnCancelled() {}

	protected virtual bool OnUpdate(float time) { return true; }

	protected virtual void OnEnd() {}

	protected virtual void OnInterrupted() {}

	protected virtual void OnStateExit() {}

	public virtual EndEventType GetEndEventType() { return EndEventType.EndTime; }

	/// <summary> This can be overriden to display the expected end time if the actual end time is set to -1. </summary>
	protected virtual float EditorDisplayDuration() { return -1.0f; }

	public virtual Color _EditorGetColor() { return new Color(0.5f, 0.25f, 0.0f); } // Default to shitty brown color so hopefully people pick nice colors

	public virtual bool ForceMaster() { return false; }

	public virtual bool ForceSlave() { return false; }
	
	public bool HasEndEvent() { return GetEndEventType() != EndEventType.NoEndEvent; }

	/// <summary> This can return the start time plus the expected duration if the actual end time is -1. </summary>
	public float _EditorDisplayEndTime()
	{
		float editorDuration = EditorDisplayDuration();
		if (HasNegativeEndTime() && editorDuration > 0.0f)
		{
			return GetStartTime() + editorDuration;
		}
		return GetEndTime();
	}

	public bool AllowMasterOrSlave() { return !ForceMaster() && !ForceSlave() && HasEndEvent(); }

	public bool IsMaster()
	{
		if (ForceMaster())
		{
			return true;
		}
		if (ForceSlave())
		{
			return false;
		}
		if (!HasEndEvent()) // Master tracks need to have some duration to keep a node alive
		{
			return false;
		}
		return m_Master;
	}

	public int CompareTo(object obj)
	{
		ActTrack track = obj as ActTrack;
		if (track == null)
		{
			return 0;
		}
		float myTime = GetStartTime();
		float theirTime = track.GetStartTime();
		if (Core.Util.Approximately(myTime, theirTime)) // If start times are the same, try end times
		{
			myTime = GetEndTime();
			if (myTime < 0.0f)
			{
				myTime = float.MaxValue;
			}
			theirTime = track.GetEndTime();
			if (theirTime < 0.0f)
			{
				theirTime = float.MaxValue;
			}
		}
		if (Core.Util.Approximately(myTime, theirTime)) // If we're still tied put the master first
		{
			myTime = IsMaster() ? 1.0f : 0.0f;
			theirTime = track.IsMaster() ? 1.0f : 0.0f;
		}
		if (Core.Util.Approximately(myTime, theirTime))
		{
			return GetInstanceID().CompareTo(track.GetInstanceID()); // We just need some kind of deterministic way to sort
		}
		return myTime > theirTime ? 1 : -1;
	}

	public override string ToString()
	{
		return GetType().ToString();
	}

	public string EditorDisplayName()
	{
		string trackName = GetType().Name;
		if (trackName.EndsWith("Track"))
		{
			trackName = trackName.Substring(0, trackName.Length - 5);
		}
		return trackName;
	}
}
