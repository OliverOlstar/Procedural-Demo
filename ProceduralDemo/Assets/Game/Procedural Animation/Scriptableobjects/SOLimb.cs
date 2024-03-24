using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PA
{
	[CreateAssetMenu(fileName = "New PA Limb", menuName = "Procedural Animation/Limb/Limb")]
    public class SOLimb : ScriptableObject
    {
		[SerializeField]
		private SOLimbTrigger m_Trigger = null;
		[SerializeField]
		private SOLimbMovement m_Movement = null;

		public void Init(PARoot2 pRoot)
		{
			m_Trigger.Init();
			m_Movement.Init();
		}

		public void Tick(float pDeltaTime)
		{
			m_Trigger.Tick(pDeltaTime);
			// m_Movement.Tick(pDeltaTime);
		}

		public void DrawGizmos()
		{
			m_Trigger.DrawGizmos();
			m_Movement.DrawGizmos();
		}
    }
}
