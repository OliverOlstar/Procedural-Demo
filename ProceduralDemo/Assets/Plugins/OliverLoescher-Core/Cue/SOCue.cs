using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher.Cue
{
	[CreateAssetMenu(menuName = "ScriptableObject/Cue")]
	public class SOCue : ScriptableObject
	{
		// TODO Add Duration & Motion

		[SerializeField]
		private CueAudio[] Audio = new CueAudio[0];
		[SerializeField]
		private CueParticle[] Particle = new CueParticle[0];
		[SerializeField]
		private CueCameraShake[] CameraShake = new CueCameraShake[0];

		public static void Play(SOCue pCue, in CueContext pContext)
		{
			if (pCue == null)
			{
				return;
			}
			foreach (CueModule module in pCue.Audio)
			{
				module.Play(pContext, pCue);
			}
			foreach (CueModule module in pCue.Particle)
			{
				module.Play(pContext, pCue);
			}
			foreach (CueModule module in pCue.CameraShake)
			{
				module.Play(pContext, pCue);
			}
		}
	}
}
