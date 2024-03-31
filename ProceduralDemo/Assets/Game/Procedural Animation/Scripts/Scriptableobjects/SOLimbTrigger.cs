using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverLoescher.Util;

namespace PA
{
	[CreateAssetMenu(fileName = "New PA Limb Trigger", menuName = "Procedural Animation/Limb/Trigger")]
	public class SOLimbTrigger : ScriptableObject
	{
		public float m_MaxDistance = 5.0f;
	}
}
