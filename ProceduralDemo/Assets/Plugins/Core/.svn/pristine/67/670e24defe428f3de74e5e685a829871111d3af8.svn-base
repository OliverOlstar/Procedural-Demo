
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public abstract class SOTrack<TController, TContext> : TrackGeneric<TContext>
		where TController : SOTrackController<TContext>
		where TContext : ITreeContext
	{
		[SerializeField]
		private TController m_Controller = null;

		private TController m_Instance = null;

		public override EndEventType GetEndEventType()
		{
			return m_Controller == null ? EndEventType.NoEndEvent : m_Controller.GetEndEventType();
		}

		protected override float _EditorDisplayDuration()
		{
			return m_Controller == null ? -1.0f : m_Controller.EditorDisplayDuration();
		}

		protected override bool OnInitialize()
		{
			if (m_Controller == null || !m_Controller.IsValid())
			{
				return false;
			}
			m_Instance = UnityEngine.Object.Instantiate(m_Controller);
			return true;
		}

		protected override void OnStart()
		{
			m_Instance.StartController(m_Context);
		}

		protected override void OnCancelled()
		{
			m_Instance.OnCancelled();
		}

		protected override void OnEnd()
		{
			m_Instance.OnEnd();
		}

		protected override bool OnUpdate(float time)
		{
			return m_Instance.OnUpdate(DeltaTime);
		}

		private void OnDestroy()
		{
			UnityEngine.Object.Destroy(m_Instance);
		}
	}
}
