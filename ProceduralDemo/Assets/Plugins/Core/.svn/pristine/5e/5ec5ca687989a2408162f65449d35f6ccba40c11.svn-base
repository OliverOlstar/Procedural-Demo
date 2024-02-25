
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public abstract class SOTrackController<TContext> : ScriptableObject where TContext : ITreeContext
	{
		public abstract Track.EndEventType GetEndEventType();

		public abstract float EditorDisplayDuration();

		protected TContext m_Context = default;

		public void StartController(TContext param)
		{
			m_Context = param;
			OnStart();
		}

		public abstract bool IsValid();

		public abstract void OnStart();

		public abstract void OnCancelled();

		public abstract void OnEnd();

		public abstract bool OnUpdate(float deltaTime);
	}
}
