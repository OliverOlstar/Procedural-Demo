using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverLoescher.Util;

namespace PA
{
	[CreateAssetMenu(fileName = "New PA Limb Trigger", menuName = "Procedural Animation/Limb/Trigger")]
	public class SOLimbTrigger : ScriptableObject
	{
		[Header("Values"), SerializeField]
		private float MaxDistance = 5.0f;

		[SerializeField]
		private SOLimb[] OppositeTargets;

		private SOLimb m_Limb;

		public void Init(SOLimb pLimb) { m_Limb = pLimb; }

		public bool Tick(float pDeltaTime)
		{
			foreach (SOLimb leg in OppositeTargets)
			{
				if (leg != null && leg.IsMoving)
				{
					return false;
				}
			}
			return Math.DistanceXZGreaterThan(m_Limb.Position, m_Limb.OriginalPositionWorld(), MaxDistance);
		}

		public float GetTickPriority() => (m_Limb.OriginalPositionWorld() - m_Limb.Position).sqrMagnitude;

		public void DrawGizmos()
		{
			
		}
	}
}
