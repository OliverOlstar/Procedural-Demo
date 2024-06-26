using UnityEngine;
using RootMotion.FinalIK;

namespace PA
{
	public class PALimb
	{
		public enum State
		{
			None,
			Moving,
			Falling
		}

		private readonly SOLimb m_Data;
		private readonly PALimbTrigger m_Trigger;
		private readonly PALimbMovement m_Movement;
		private readonly PAPoint m_Point;
		private State m_State = State.None;
		
		public Vector3 Position { get => m_Point.Position; set { m_Point.Position = value; } }
		public Vector3 OriginalPositionWorld() => m_Point.OriginalPositionWorld();
		public PAPoint Point => m_Point;
		public bool IsIdle => m_State == State.None;
		public bool IsMoving => m_State == State.Moving;

		public PALimb(SOLimb pData, IKSolverCCD pIK, PARoot pRoot)
		{
			m_Data = pData;
			m_Point = new PAPoint(pRoot, pIK);
			m_Trigger = new PALimbTrigger(pData.m_StepTrigger, pRoot, this);
			m_Movement = new PALimbMovement(pData.m_StepMovement, pRoot, this);
		}

		/// <summary> Returns true if a trigger is hit </summary>
		public bool TickTriggers(float pDeltaTime)
		{
			if (!IsIdle)
			{
				return false;
			}
			if (m_Trigger.Tick(pDeltaTime))
			{
				SwitchState(State.Moving);
				return true;
			}
			return false;
		}

		public void SwitchState(State pState)
		{
			if (m_State == pState)
			{
				return;
			}

			switch (m_State)
			{
				case State.None:
					break;

				case State.Moving:
					m_Movement.StopMove();
					break;

				default:
					throw new System.NotImplementedException();
			}

			m_State = pState;

			switch (m_State)
			{
				case State.None:
					break;

				case State.Moving:
					m_Movement.StartMove();
					break;

				default:
					throw new System.NotImplementedException();
			}
		}

		public void DrawGizmos()
		{
			m_Point.DrawGizmos();
			m_Trigger.DrawGizmos();
			m_Movement.DrawGizmos();
		}
	}
}
