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
			set => m_IKSolver.IKPosition = value;
		}
		public Vector3 RelativeOriginalPosition => throw new System.NotImplementedException();

		public void Init(IKSolverCCD pIKSolver) => m_IKSolver = pIKSolver;

		public void DrawGizmos()
		{
			
		}
	}
}
