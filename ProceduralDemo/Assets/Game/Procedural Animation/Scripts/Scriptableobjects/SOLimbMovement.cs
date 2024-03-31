using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverLoescher.Util;
using OliverLoescher.Cue;

namespace PA
{
	[CreateAssetMenu(fileName = "New PA Limb Movement", menuName = "Procedural Animation/Limb/Movement")]
	public class SOLimbMovement : ScriptableObject
	{
		[SerializeField]
		private float StepDistance = 1.0f;

		[Header("Animation")]
		[SerializeField]
		private Easing.EaseParams EaseStep;
		[SerializeField]
		private Easing.EaseParams EaseHeight;
		[SerializeField, Min(Math.NEARZERO)]
		private Vector2 StepSeconds = Vector2.one;
		[SerializeField]
		private float UpHeight = 1.0f;

		[Header("Linecast")]
		[SerializeField]
		private Vector2 LinecastUpDown = new(1, -1);
		[SerializeField]
		private LayerMask StepLayer = new();

		[Header("Cues")]
		[SerializeField]
		private SOCue OnStepCue;

		public Vector3 Position
		{
			get => m_Limb.Position;
			set => m_Limb.Position = value;
		}

		private Vector3 StepOffset;

		public Vector3 TargetPosition() => m_Limb.OriginalPositionWorld();

		private PARoot m_Root;
		private SOLimb m_Limb;
		Anim.IAnimation m_Animation;

		public void Init(PARoot pRoot, SOLimb pLimb)
		{
			m_Root = pRoot;
			m_Limb = pLimb;
			Position = CalculateStepPoint();
		}

		public void StartMove()
		{
			StepOffset = Position;
			m_Animation = Anim.Play2D(EaseStep, EaseHeight, Random2.Range(StepSeconds), StepTick, StepComplete);
		}
		public void StopMove()
		{
			if (m_Animation != null)
			{
				m_Animation.Cancel();
				m_Animation = null;
			}
		}

		private void StepTick(Vector2 pProgress)
		{
			Vector3 endStepPoint = CalculateStepPoint();
			Vector3 up = m_Root.Up * Math.LerpUnclamped(0, UpHeight, 0, pProgress.y);
			Vector3 horizontal = Vector3.LerpUnclamped(StepOffset, endStepPoint, pProgress.x);
			Position = horizontal + up;
		}
		private void StepComplete(Vector2 _)
		{
			Position = CalculateStepPoint();
			SOCue.Play(OnStepCue, new CueContext(Position));
			m_Limb.SwitchState(SOLimb.State.None);
			m_Animation = null;
		}

		private Vector3 CalculateStepPoint()
		{
			Vector3 targetPosition = TargetPosition();
			Vector3 stepMotion = m_Root.Velocity.normalized * -StepDistance;
			Vector3 stepEndPoint = stepMotion + targetPosition;
			Vector3 upPoint = (LinecastUpDown.x * m_Root.Up) + stepEndPoint;
			Vector3 downPoint = (LinecastUpDown.y * m_Root.Up) + stepEndPoint;
			if (Physics.Linecast(upPoint, downPoint, out RaycastHit hit, StepLayer))
			{
				stepMotion = hit.point - targetPosition;
			}
			return targetPosition + stepMotion;
		}

		public void DrawGizmos()
		{
			Vector3 targetPosition = TargetPosition();

			// Points
			// Gizmos.color = Color.blue;
			// Gizmos.DrawSphere(Position, 0.2f);
			// Gizmos.color = Color.yellow;
			// Gizmos.DrawSphere(targetPosition, 0.2f);
			Gizmos.DrawWireSphere(targetPosition, StepDistance);

			// Linecast
			Vector3 stepMotion = m_Root.Velocity.normalized * -StepDistance;
			Vector3 stepEndPoint = stepMotion + targetPosition;
			Vector3 upPoint = (LinecastUpDown.x * m_Root.Up) + stepEndPoint;
			Vector3 downPoint = (LinecastUpDown.y * m_Root.Up) + stepEndPoint;
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(upPoint, downPoint);
		}
	}
}
