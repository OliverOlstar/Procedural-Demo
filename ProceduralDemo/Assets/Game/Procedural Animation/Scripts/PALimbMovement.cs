using UnityEngine;
using ODev.Cue;
using ODev.Util;

namespace PA
{
	public class PALimbMovement
    {
		private Vector3 m_StepOffset;

		private readonly PARoot m_Root;
		private readonly PALimb m_Limb;
		private readonly SOLimbMovement m_Data;
		private Anim.IAnimation m_Animation;

		public Vector3 Position
		{
			get => m_Limb.Position;
			set => m_Limb.Position = value;
		}
		public Vector3 TargetPosition() => m_Limb.OriginalPositionWorld();

		public PALimbMovement(SOLimbMovement pData, PARoot pRoot, PALimb pLimb)
		{
			m_Root = pRoot;
			m_Limb = pLimb;
			m_Data = pData;
			Position = CalculateStepPoint();
		}

		public void StartMove()
		{
			m_StepOffset = Position;
			m_Animation = Anim.Play2D(m_Data.EaseStep, m_Data.EaseHeight, ODev.Util.Random.Range(m_Data.StepSeconds), Anim.Type.Visual, StepTick, StepComplete);
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
			Vector3 up = m_Root.Up * Math.LerpUnclamped(0, m_Data.UpHeight, 0, pProgress.y);
			Vector3 horizontal = Vector3.LerpUnclamped(m_StepOffset, endStepPoint, pProgress.x);
			Position = horizontal + up;
		}
		private void StepComplete(Vector2 _)
		{
			Position = CalculateStepPoint();
			SOCue.Play(m_Data.OnStepCue, new CueContext(Position));
			m_Limb.SwitchState(PALimb.State.None);
			m_Animation = null;
		}

		private Vector3 CalculateStepPoint()
		{
			Vector3 targetPosition = TargetPosition();
			Vector3 stepMotion = m_Root.Velocity.normalized * -m_Data.StepDistance;
			Vector3 stepEndPoint = stepMotion + targetPosition;
			Vector3 upPoint = (m_Data.LinecastUpDown.x * m_Root.Up) + stepEndPoint;
			Vector3 downPoint = (m_Data.LinecastUpDown.y * m_Root.Up) + stepEndPoint;
			if (Physics.Linecast(upPoint, downPoint, out RaycastHit hit, m_Data.StepLayer))
			{
				stepMotion = hit.point - targetPosition;
			}
			return targetPosition + stepMotion;
		}

		public void DrawGizmos()
		{
			Vector3 targetPosition = TargetPosition();
			Gizmos.DrawWireSphere(targetPosition, m_Data.StepDistance);

			// Linecast
			Vector3 stepMotion = m_Root.Velocity.normalized * -m_Data.StepDistance;
			Vector3 stepEndPoint = stepMotion + targetPosition;
			Vector3 upPoint = (m_Data.LinecastUpDown.x * m_Root.Up) + stepEndPoint;
			Vector3 downPoint = (m_Data.LinecastUpDown.y * m_Root.Up) + stepEndPoint;
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(upPoint, downPoint);
		}
    }
}
