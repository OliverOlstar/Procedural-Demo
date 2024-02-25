
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public abstract class TrackEvent<TEvent, TContext> : TrackGeneric<TContext> 
		where TEvent : ITreeEvent
		where TContext : ITreeContext
	{
		public new static System.Type _EditorGetEvent() => typeof(TEvent);

		protected TEvent m_Event = default;

		public override bool IsEventRequired(out System.Type eventType)
		{
			eventType = typeof(TEvent);
			return true;
		}

		protected override bool TryStart(ITreeEvent treeEvent)
		{
			if (!IsEventRequired(out System.Type trackEventType))
			{
				m_Event = default;
				return true;
			}
			if (treeEvent == null)
			{
				m_Event = default;
				return false;
			}
			if (!trackEventType.IsAssignableFrom(treeEvent.GetType()))
			{
				m_Event = default;
				return false;
			}
			m_Event = (TEvent)treeEvent;
			return true;
		}
	}
}
