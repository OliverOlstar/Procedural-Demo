using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using RootMotion.FinalIK;
using System;

namespace PA
{
	public class PARoot2 : MonoBehaviour
	{
		[SerializeField, DisableInPlayMode]
		private OliverLoescher.Util.Mono.Updateable m_Updateable = new(OliverLoescher.Util.Mono.Type.Late, OliverLoescher.Util.Mono.Priorities.ModelController);

		private bool m_IsInitalized = false;

		public IPACharacter Character { get; private set; }
		[SerializeField]
		private SOBody m_Body;
		[SerializeField]
		private Transform m_BodyTransform;
		[SerializeField]
		private SOLimb[] m_Limbs;
		[SerializeField]
		private CCDIK[] m_LimbIKs;
		public List<PAPoint> Points { get; private set; } = new();

		public SOBody Body => m_Body;
		public Transform BodyTransform => m_BodyTransform;
		public SOLimb[] Limbs => m_Limbs;
		public CCDIK[] LimbIKs => m_LimbIKs;

		public void Initalize()
		{
			if (m_IsInitalized)
			{
				return;
			}
			m_IsInitalized = true;

			if (m_Body != null)
			{
				m_Body.Init(this);
			}
			for (int i = 0; i < m_Limbs.Length; i++)
			{
				m_Limbs[i].Init(this);
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
		}

		private void Tick(float pDeltaTime)
		{
			if (m_Body != null)
			{
				m_Body.Tick(pDeltaTime);
			}

			// Array.Sort(m_Limbs, (SOLimb a, SOLimb b) => b.GetTickPriority().CompareTo(a.GetTickPriority()));
			for (int i = 0; i < m_Limbs.Length; i++)
			{
				m_Limbs[i].Tick(pDeltaTime);
			}
		}

		private void OnDrawGizmos()
		{
			Initalize();

			if (m_Body != null)
			{
				m_Body.DrawGizmos();
			}
			for (int i = 0; i < m_Limbs.Length; i++)
			{
				m_Limbs[i].DrawGizmos();
			}
			for (int i = 0; i < Points.Count; i++)
			{
				Points[i].DrawGizmos();
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