using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using RootMotion.FinalIK;
using System;

namespace PA
{
	public class PARoot : MonoBehaviour
	{
		[SerializeField, DisableInPlayMode]
		private OliverLoescher.Util.Mono.Updateable m_Updateable = new(OliverLoescher.Util.Mono.Type.Late, OliverLoescher.Util.Mono.Priorities.ModelController);

		private bool m_IsInitalized = false;

		public IPACharacter Character { get; private set; }
		[SerializeField]
		private SOBody m_Body;
		private SOBody m_BodyInstances;
		[SerializeField]
		private Transform m_BodyTransform;
		[SerializeField]
		private SOLimb[] m_Limbs;
		private SOLimb[] m_LimbInstances;
		[SerializeField]
		private CCDIK[] m_LimbIKs;
		public IEnumerable<PAPoint> GetAllPoints()
		{
			foreach (SOLimb limb in Limbs)
			{
				yield return limb.Point;
			}
		}
		public int PointsCount => Limbs.Length;

		public SOBody Body => m_BodyInstances;
		public Transform BodyTransform => m_BodyTransform;
		public SOLimb[] Limbs => m_LimbInstances;
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
			if (m_Body != null)
			{
				m_BodyInstances = Instantiate(m_Body);
				m_BodyInstances.Init(this);
			}
			m_LimbInstances = new SOLimb[m_Limbs.Length];
			for (int i = 0; i < m_Limbs.Length; i++)
			{
				m_LimbInstances[i] = Instantiate(m_Limbs[i]);
				m_LimbInstances[i].Init(this, m_LimbIKs[i].solver);
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
			Destroy(m_BodyInstances);
			for (int i = 0; i < m_LimbInstances.Length; i++)
			{
				Destroy(m_LimbInstances[i]);
			}
			m_IsInitalized = false;
		}

		private void Tick(float pDeltaTime)
		{
			if (Body != null)
			{
				Body.Tick(pDeltaTime);
			}

			// Array.Sort(m_Limbs, (SOLimb a, SOLimb b) => b.GetTickPriority().CompareTo(a.GetTickPriority()));
			for (int i = 0; i < Limbs.Length; i++)
			{
				Limbs[i].Tick(pDeltaTime);
			}
		}

		private void OnDrawGizmos()
		{
			if (!m_IsInitalized)
			{
				return;
			}
			if (Body != null)
			{
				Body.DrawGizmos();
			}
			for (int i = 0; i < Limbs.Length; i++)
			{
				Limbs[i].DrawGizmos();
			}
			foreach (PAPoint point in GetAllPoints())
			{
				point.DrawGizmos();
			}
		}

		// public void AddLimb(IPALimb pLimb)
		// {
		// 	if (!m_IsInitalized)
		// 	{
		// 		OliverLoescher.Util.Debug.LogWarning("Not initalized yet, skipping add", "AddLimb", this);
		// 		return;
		// 	}
		// 	if (m_Limbs.Contains(pLimb))
		// 	{
		// 		OliverLoescher.Util.Debug.LogWarning("Limb already added", "AddLimb", this);
		// 		return;
		// 	}
		// 	m_Limbs.Add(pLimb);
		// 	pLimb.Init(this);
		// }
		// public void RemoveLimb(IPALimb pLimb)
		// {
		// 	if (!m_Limbs.Remove(pLimb))
		// 	{
		// 		OliverLoescher.Util.Debug.LogWarning("Failed to remove", "RemoveLimb", this);
		// 	}
		// }

		// public void AddPoint(IPAPoint pPoint)
		// {
		// 	if (!m_IsInitalized)
		// 	{
		// 		OliverLoescher.Util.Debug.LogWarning("Not initalized yet, skipping add", "AddPoint", this);
		// 		return;
		// 	}
		// 	if (Points.Contains(pPoint))
		// 	{
		// 		OliverLoescher.Util.Debug.LogWarning("Point already added", "AddPoint", this);
		// 		return;
		// 	}
		// 	Points.Add(pPoint);
		// 	pPoint.Init(Character);
		// }
		// public void RemovePoint(IPAPoint pAPoint)
		// {
		// 	if (!Points.Remove(pAPoint))
		// 	{
		// 		OliverLoescher.Util.Debug.LogWarning("Failed to remove", "RemovePoint", this);
		// 	}
		// }
	}
}