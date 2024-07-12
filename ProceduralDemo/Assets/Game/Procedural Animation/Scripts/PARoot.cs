using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using RootMotion.FinalIK;
using ODev.Util;

namespace PA
{
	public class PARoot : MonoBehaviour
	{
		[SerializeField, DisableInPlayMode]
		private ODev.Util.Mono.Updateable m_Updateable = new(ODev.Util.Mono.Type.Late, ODev.Util.Mono.Priorities.ModelController);

		private bool m_IsInitalized = false;

		public IPACharacter Character { get; private set; }
		[SerializeField]
		private SOBody m_BodyData;
		private PABody m_Body;
		[SerializeField]
		private Transform m_BodyTransform;
		[SerializeField]
		private SOLimb[] m_LimbDatas;
		private PALimb[] m_Limbs;
		private int m_LastLimbIndex = 0;
		[SerializeField]
		private CCDIK[] m_LimbIKs;
		public IEnumerable<PAPoint> GetAllPoints()
		{
			foreach (PALimb limb in Limbs)
			{
				yield return limb.Point;
			}
		}
		public int PointsCount => Limbs.Length;

		public PABody Body => m_Body;
		public PALimb[] Limbs => m_Limbs;
		public CCDIK[] LimbIKs => m_LimbIKs;

		public Vector3 Center => Character.Position;
		public Vector3 Forward => Character.Forward;
		public Vector3 Up => Character.Up;
		public Quaternion Rotation => Character.Rotation;
		public Vector3 Velocity => Character.Veclocity;
		public Vector3 TransformPoint(Vector3 pPoint) => Character.TransformPoint(pPoint); // Local to World
		public Vector3 InverseTransformPoint(Vector3 pPoint) => Character.InverseTransformPoint(pPoint); // World to Local

		public void Initalize()
		{
			if (m_IsInitalized)
			{
				return;
			}
			m_IsInitalized = true;
			
			Character = GetComponentInChildren<IPACharacter>();
			m_Body = new PABody(m_BodyData, m_BodyTransform, this);
			m_Limbs = new PALimb[m_LimbDatas.Length];
			for (int i = 0; i < m_Limbs.Length; i++)
			{
				m_Limbs[i] = new PALimb(m_LimbDatas[i], m_LimbIKs[i].solver, this);
			}
		}

		private void Awake() => Initalize();
		private void Start()
		{
			m_Updateable.Register(Tick);
		}
		private void OnDestroy()
		{
			m_Updateable.Deregister();
			m_IsInitalized = false;
		}

		private void Tick(float pDeltaTime)
		{
			Body?.Tick(pDeltaTime);
			
			Func.Foreach(Limbs, m_LastLimbIndex + 1, (PALimb pLimb, int pIndex) =>
			{
				if (pLimb.TickTriggers(pDeltaTime))
				{
					m_LastLimbIndex = pIndex;
				}
				return false;
			});
		}

		private void OnDrawGizmos()
		{
			if (!m_IsInitalized)
			{
				return;
			}
			Body?.DrawGizmos();
			for (int i = 0; i < Limbs.Length; i++)
			{
				Limbs[i].DrawGizmos();
			}
		}

		// public void AddLimb(IPALimb pLimb)
		// {
		// 	if (!m_IsInitalized)
		// 	{
		// 		ODev.Util.Debug.LogWarning("Not initalized yet, skipping add", "AddLimb", this);
		// 		return;
		// 	}
		// 	if (m_Limbs.Contains(pLimb))
		// 	{
		// 		ODev.Util.Debug.LogWarning("Limb already added", "AddLimb", this);
		// 		return;
		// 	}
		// 	m_Limbs.Add(pLimb);
		// 	pLimb.Init(this);
		// }
		// public void RemoveLimb(IPALimb pLimb)
		// {
		// 	if (!m_Limbs.Remove(pLimb))
		// 	{
		// 		ODev.Util.Debug.LogWarning("Failed to remove", "RemoveLimb", this);
		// 	}
		// }

		// public void AddPoint(IPAPoint pPoint)
		// {
		// 	if (!m_IsInitalized)
		// 	{
		// 		ODev.Util.Debug.LogWarning("Not initalized yet, skipping add", "AddPoint", this);
		// 		return;
		// 	}
		// 	if (Points.Contains(pPoint))
		// 	{
		// 		ODev.Util.Debug.LogWarning("Point already added", "AddPoint", this);
		// 		return;
		// 	}
		// 	Points.Add(pPoint);
		// 	pPoint.Init(Character);
		// }
		// public void RemovePoint(IPAPoint pAPoint)
		// {
		// 	if (!Points.Remove(pAPoint))
		// 	{
		// 		ODev.Util.Debug.LogWarning("Failed to remove", "RemovePoint", this);
		// 	}
		// }
	}
}