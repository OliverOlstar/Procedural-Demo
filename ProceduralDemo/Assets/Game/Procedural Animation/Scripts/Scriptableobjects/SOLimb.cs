using System;
using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;

namespace PA
{
	[CreateAssetMenu(fileName = "New PA Limb", menuName = "Procedural Animation/Limb/Limb")]
	public class SOLimb : ScriptableObject
	{
		public enum State
		{
			None,
			Moving,
			Falling
		}

		[SerializeField]
		private SOLimbTrigger m_StepTrigger = null;
		[SerializeField]
		private SOLimbMovement m_StepMovement = null;

		private readonly PAPoint m_Point = new();
		public Vector3 Position { get => m_Point.Position; set { m_Point.Position = value; } }
		public Vector3 OriginalPositionWorld() => m_Point.OriginalPositionWorld();
		public PAPoint Point => m_Point;

		private State m_State = State.None;
		public bool IsIdle => m_State == State.None;
		public bool IsMoving => m_State == State.Moving;

		public void Init(PARoot pRoot, IKSolverCCD pIK)
		{
			m_StepTrigger = Instantiate(m_StepTrigger);
			m_StepMovement = Instantiate(m_StepMovement);

			m_Point.Init(pRoot, pIK);
			m_StepTrigger.Init(pRoot, this);
			m_StepMovement.Init(pRoot, this);
		}

		/// <summary>
		/// Returns true if a trigger is hit
		/// </summary>
		public bool TickTriggers(float pDeltaTime)
		{
			if (!IsIdle)
			{
				return false;
			}
			if (m_StepTrigger.Tick(pDeltaTime))
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
					m_StepMovement.StopMove();
					break;

				default:
					throw new NotImplementedException();
			}

			m_State = pState;

			switch (m_State)
			{
				case State.None:
					break;

				case State.Moving:
					m_StepMovement.StartMove();
					break;

				default:
					throw new NotImplementedException();
			}
		}

		public void DrawGizmos()
		{
			m_Point.DrawGizmos();
			m_StepTrigger.DrawGizmos();
			m_StepMovement.DrawGizmos();
		}
	}
}
