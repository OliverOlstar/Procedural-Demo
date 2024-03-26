using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;

namespace PA
{
	public class PAPoint
	{
		private IKSolverCCD m_IKSolver;

		public Vector3 Position
		{
			get => m_IKSolver.IKPosition;
			set
			{
				m_IKSolver.IKPosition = value;
				
			}
		}
		public Vector3 OriginalPositionWorld() => m_Root.TransformPoint(m_OriginalPositionLocal);
		public Vector3 OriginalPositionLocal => m_OriginalPositionLocal;

		private PARoot2 m_Root;
		private Vector3 m_OriginalPositionLocal;

		public void Init(PARoot2 pRoot, IKSolverCCD pIKSolver)
		{
			m_Root = pRoot;
			m_IKSolver = pIKSolver;

			m_OriginalPositionLocal = pRoot.InverseTransformPoint(pIKSolver.IKPosition);
		}

		public void DrawGizmos()
		{
			if (m_Root == null || m_IKSolver == null)
			{
				return;
			}
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(Position, 0.25f);
			Gizmos.DrawWireSphere(OriginalPositionWorld(), 0.25f);
		}
	}
}
