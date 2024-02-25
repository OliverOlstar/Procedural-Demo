using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public class ActTrackFab<TTrackFab, TContext> : TrackEvent<Act2.ITreeEvent, TContext>, ITreeOwner
		where TTrackFab : SOActTrackFab<TContext>
		where TContext : ITreeContext
	{
		[SerializeField, UberPicker.AssetNonNull]
		private TTrackFab m_TrackFab = null;

		private TreeRT m_FabTree = null;

		private TrackPlayer m_TrackPlayer = new();

		string ITreeOwner.name => m_Node.Name;
		ITreeContext ITreeOwner.GetContext() => m_Context;

		public override TrackType GetDefaultTrackType() => TrackType.Minor;
		public override EndEventType GetEndEventType() => EndEventType.EndTime;

		public override bool IsEventRequired(out System.Type eventType)
		{
			if (m_TrackFab == null)
			{
				eventType = null;
				return false;
			}
			return m_TrackFab.RootNode.IsEventRequired(out eventType);
		}

		protected override string OnValidate()
		{
			return m_TrackFab == null ? "Fab cannot be null" : null;
		}

		protected override bool OnInitialize()
		{
			m_FabTree = TreeRT.CreateAndInitialize(m_TrackFab, this);
			return true;
		}

		protected override void OnStart()
		{
#if UNITY_EDITOR
			m_FabTree._EditorRebuildIfDirty();
#endif
			m_TrackPlayer.StateEnter(m_FabTree.RootNode.Tracks, CurrentTimeScale, m_Event);
		}

		protected override bool OnUpdate(float time)
		{
#if UNITY_EDITOR
			m_FabTree._EditorRebuildIfDirty();
#endif
			m_TrackPlayer.UpdateAll(time, DeltaTime, CurrentTimeScale);
			bool keepAlive = m_TrackPlayer.KeepAlive();
			return keepAlive;
		}

		protected override void OnEnd()
		{
#if UNITY_EDITOR
			m_FabTree._EditorRebuildIfDirty();
#endif
			m_TrackPlayer.StateExit(Interrupted);
		}

		public override string ToString() => m_TrackFab != null ? m_TrackFab.name : $"{GetType().Name}(NULL)";
	}
}
