using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverLoescher.Util;

namespace PA
{
	[CreateAssetMenu(fileName = "New PA Limb Trigger", menuName = "Procedural Animation/Limb/Trigger")]
	public class SOLimbTrigger : ScriptableObject
	{
		[SerializeField]
		private float m_MaxDistance = 5.0f;
		[SerializeField]
		private float m_Cooldown = 1.0f;

		private PARoot m_Root;
		private SOLimb m_Limb;
		private Coroutine m_CooldownRoutine;

		public void Init(PARoot pRoot, SOLimb pLimb) { m_Root = pRoot; m_Limb = pLimb; }

		public bool Tick(float pDeltaTime)
		{
			if (m_CooldownRoutine != null)
			{
				return false;
			}
			foreach (SOLimb leg in m_Root.Limbs)
			{
				if (leg != null && leg.IsMoving)
				{
					return false;
				}
			}
			if (Math.DistanceXZGreaterThan(m_Limb.Position, m_Limb.OriginalPositionWorld(), m_MaxDistance))
			{
				m_CooldownRoutine = OliverLoescher.Util.Mono.Start(DoCoolDown());
				return true;
			}
			return false;
		}

		private IEnumerator DoCoolDown()
		{
			yield return new WaitForSeconds(m_Cooldown);
			m_CooldownRoutine = null;
		}

		public float GetTickPriority() => (m_Limb.OriginalPositionWorld() - m_Limb.Position).sqrMagnitude;

		public void DrawGizmos()
		{
			
		}
	}
}
