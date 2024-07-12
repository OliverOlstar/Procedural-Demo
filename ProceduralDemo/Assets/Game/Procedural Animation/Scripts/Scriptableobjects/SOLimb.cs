using UnityEngine;
using ODev.Picker;

namespace PA
{
	[CreateAssetMenu(fileName = "New PA Limb", menuName = "Procedural Animation/Limb/Limb")]
	public class SOLimb : ScriptableObject
	{
		[Asset]
		public SOLimbTrigger m_StepTrigger = null;
		[Asset]
		public SOLimbMovement m_StepMovement = null;
	}
}
