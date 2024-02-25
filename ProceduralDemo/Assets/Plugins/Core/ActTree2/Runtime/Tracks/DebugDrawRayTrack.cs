using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Act2
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[CoreTrackGroup.Debug]
	public class DebugDrawRayTrack : TrackEvent<ITreeEvent, ITreeContext>
	{
		[SerializeField, UberPicker.AssetNonNull]
		private BasePositionLambda m_Position = null;

		[SerializeField, UberPicker.AssetNonNull]
		private DirectionLambdaBase m_Direction = null;

		[SerializeField]
		private ActValue.Float m_Length = new ActValue.Float(2.0f);

		[SerializeField]
		private Color m_Color = Color.cyan;

		private enum UpdateType
		{
			OnStart,
			EveryFrame
		}
		[SerializeField]
		private UpdateType m_UpdateType = UpdateType.OnStart;
		[SerializeField]
		private float m_DrawDuration = 0.5f;

		private Vector3 m_Pos = Vector3.zero;
		private Vector3 m_Dir = Vector3.forward;
		private float m_Dist = 2.0f;

		public override EndEventType GetEndEventType() => m_UpdateType == UpdateType.OnStart ? EndEventType.NoEndEvent : EndEventType.EndTime;
		public override TrackType GetDefaultTrackType() => TrackType.Minor;

		public override bool IsEventRequired(out System.Type eventType)
		{
			if (m_Position != null && m_Position.RequiresEvent(out eventType))
			{
				return true;
			}
			if (m_Direction != null && m_Direction.RequiresEvent(out eventType))
			{
				return true;
			}
			if (m_Length.RequiresEvent(out eventType))
			{
				return true;
			}
			eventType = null;
			return false;
		}

		private bool Update()
		{
			if (!m_Position.TryEvaluate(m_Context, m_Event, out m_Pos))
			{
				return false;
			}
			if (!m_Direction.TryEvaluate(m_Context, m_Event, out m_Dir))
			{
				return false;
			}
			m_Dist = m_Length.GetValue(m_Context, m_Event);
			return true;
		}

		protected override void OnStart()
		{
			if (m_UpdateType != UpdateType.OnStart)
			{
				return;
			}
			if (!Update())
			{
				return;
			}
			Draw();
		}

		protected override bool OnUpdate(float time)
		{
			if (!Update())
			{
				return false;
			}
			Draw();
			return true;
		}

		private void Draw()
		{
			Debug.DrawRay(m_Pos, m_Dist * m_Dir, m_Color, m_DrawDuration);
			Core.DebugUtil.DrawBox(m_Pos, m_Color, 0.2f, m_DrawDuration);
			Core.DebugUtil.DrawBox(m_Pos + m_Dist * m_Dir, m_Color, 0.05f, m_DrawDuration);
		}
	}
}
