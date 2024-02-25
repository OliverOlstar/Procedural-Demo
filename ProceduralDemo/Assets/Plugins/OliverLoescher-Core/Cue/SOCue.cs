using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher.Cue
{
    public class SOCue : ScriptableObject
    {
		// Duration & Motion

		// Audio
		[SerializeField]
		private Util.Audio.AudioPiece Sound;

		// Particle

		// Camera Shake

		public void Play(Transform pTarget, Vector3 pPoint)
		{
			//Sound.Play();
		}
    }
}
