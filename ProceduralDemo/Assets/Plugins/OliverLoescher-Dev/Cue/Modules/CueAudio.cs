using UnityEngine;

namespace ODev.Cue
{
	[System.Serializable]
	public class CueAudio : CueModule
	{
		[SerializeField]
		private Util.Audio.AudioPiece m_Sound;

		protected override void PlayInternal(in CueContext pContext, in SOCue pParent)
		{
			m_Sound.Play(pContext.Point);
		}
	}
}