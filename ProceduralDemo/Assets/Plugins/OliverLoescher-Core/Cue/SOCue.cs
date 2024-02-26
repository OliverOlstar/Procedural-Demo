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
		private CueAudio Audio = new CueAudio();
		[SerializeField]
		private CueParticle Particle = new CueParticle();
		[SerializeField]
		private CueCameraShake CameraShake = new CueCameraShake();

		public void Play(CueContext pContext)
		{
			Audio.Play(pContext, this);
			Particle.Play(pContext, this);
			CameraShake.Play(pContext, this);
		}
    }
}
