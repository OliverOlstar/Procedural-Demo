using UnityEngine;

namespace PA
{
	[CreateAssetMenu(fileName = "New PA Limb", menuName = "Procedural Animation/Limb/Limb")]
	public class SOLimb : ScriptableObject
	{
		public SOLimbTrigger m_StepTrigger = null;
		public SOLimbMovement m_StepMovement = null;
	}
}
