using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RootMotion.FinalIK;
using UnityEngine;

namespace PA
{
	public class PAPoint
	{
		private static Transform s_Container;

		public Vector3 Position
		{
			get => m_Target.position;
			set => m_Target.position = value;
		}
		public Vector3 OriginalPositionWorld() => m_Root.TransformPoint(m_OriginalPositionLocal);
		public Vector3 OriginalPositionLocal => m_OriginalPositionLocal;

		private PARoot2 m_Root;
		private Transform m_Target;
		private Vector3 m_OriginalPositionLocal;

		public void Init(PARoot2 pRoot, IKSolverCCD pIKSolver)
		{
			m_Root = pRoot;
			m_Target = pIKSolver.target;
			if (m_Target == null)
			{
				m_Target = new GameObject($"{pRoot.name}.{pIKSolver.GetRoot().name} IKTarget").transform;
				if (s_Container == null)
					s_Container = new GameObject("IKTargets Container").transform;
				m_Target.SetParent(s_Container);
				m_Target.position = pIKSolver.bones.Last().transform.position;
				pIKSolver.target = m_Target;
			}
			m_OriginalPositionLocal = pRoot.InverseTransformPoint(m_Target.position);
		}

		public void DrawGizmos()
		{
			if (m_Root == null)
			{
				return;
			}
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(Position, 0.25f);
			Gizmos.DrawWireSphere(OriginalPositionWorld(), 0.25f);
		}
	}
}
