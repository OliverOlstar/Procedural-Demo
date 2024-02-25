using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace OliverLoescher.Util
{
	// Static Util class for general playing audio functions
	public static class Audio
	{
		[System.Serializable]
		public class AudioPiece
		{
			public AudioClip[] Clips = new AudioClip[0];
			[Range(0, 1)] 
			public float Volume = 1.0f;
			[MinMaxSlider(0, 3, true)]
			public Vector2 Pitch = new Vector2(0.9f, 1.2f);

			public void Play() => AudioPool.PlayOneShot(Clips, Pitch, Volume);
			public void Play(in Vector3 pPoint) => AudioPool.PlayOneShot(Clips, pPoint, Pitch, Volume);
		}
	}
}