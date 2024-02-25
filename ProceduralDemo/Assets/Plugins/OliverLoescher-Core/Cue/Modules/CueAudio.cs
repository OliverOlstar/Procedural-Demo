using System.Collections;
using System.Collections.Generic;
using OliverLoescher.Cue;
using UnityEngine;

namespace OliverLoescher.Cue
{
	[System.Serializable]
	public class CueAudio : CueModule
	{
		[SerializeField]
		private Util.Audio.AudioPiece Sound;

		public override void Play(CueContext pContext)
		{
			Sound.Play(pContext.Point);
		}
	}
}