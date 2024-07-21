using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ODev.Cue
{
	[CreateAssetMenu(menuName = "Scriptable Object/Cue")]
	public class SOCue : ScriptableObject
	{
		// TODO Add Duration & Motion

		[SerializeField]
		private CueAudio[] m_Audio = new CueAudio[0];
		[SerializeField]
		private CueParticle[] m_Particle = new CueParticle[0];
		[SerializeField]
		private CueCameraShake[] m_CameraShake = new CueCameraShake[0];

		public static void Play(SOCue pCue, in CueContext pContext)
		{
			if (pCue == null)
			{
				return;
			}
			foreach (CueModule module in pCue.m_Audio)
			{
				module.Play(pContext, pCue);
			}
			foreach (CueModule module in pCue.m_Particle)
			{
				module.Play(pContext, pCue);
			}
			foreach (CueModule module in pCue.m_CameraShake)
			{
				module.Play(pContext, pCue);
			}
		}
		public void Play(in CueContext pContext) => Play(this, pContext);
	}
}
