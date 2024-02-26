using UnityEngine;

namespace OliverLoescher.Cue
{
	[System.Serializable]
	public class CueAudio : CueModule
	{
		[SerializeField]
		private Util.Audio.AudioPiece Sound;

		protected override void PlayInternal(in CueContext pContext, in SOCue pParent)
		{
			Sound.Play(pContext.Point);
		}
	}
}