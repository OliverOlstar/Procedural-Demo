using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	[System.Serializable]
	public class NodeTransition : ITimedItem
	{
		public static readonly Color EDITOR_COLOR = new Color(0.3f, 0.3f, 0.3f, 1.0f);

		[SerializeField, HideInInspector]
		private int m_ToID = 0;
		public int GetToID() { return m_ToID; }

		[SerializeField, HideInInspector]
		private float m_StartTime = 0.0f;
		public float GetStartTime() { return m_StartTime; }
		public bool IsAvailableOnEnd() { return m_StartTime < 0.0f; }

		[SerializeField, HideInInspector]
		private float m_EndTime = -1.0f;
		public float GetEndTime() { return m_EndTime; }
		public bool HasNegativeEndTime() { return m_EndTime < 0.0f; }

		public virtual void _EditorAddSubTimes(List<float> times) { }

		public Track.EndEventType GetEndEventType() { return IsAvailableOnEnd() ? Track.EndEventType.NoEndEvent : Track.EndEventType.EndTime; }

		public bool IsAvailable(float time)
		{
			if (m_StartTime < 0.0f) // Only available on end
			{
				return false;
			}
			if (time < m_StartTime)
			{
				return false;
			}
			if (m_EndTime < 0.0f)
			{
				return true;
			}
			return time < m_EndTime;
		}

		public override string ToString()
		{
			return "NodeTransition(" + m_ToID + ")";
		}

		public int GetInstanceID()
		{
			return m_ToID;
		}

		public bool IsMajor()
		{
			return false;
		}

		public bool HasEndEvent()
		{
			return !IsAvailableOnEnd();
		}

		public float _EditorDisplayEndTime()
		{
			return m_EndTime;
		}

		public bool IsActive()
		{
			return true;
		}

		public Color _EditorGetColor() => EDITOR_COLOR;
	}
}
