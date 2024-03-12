using UnityEngine;

namespace OliverLoescher.Cue
{
	[System.Serializable]
	public class CueModule
	{
		[SerializeField]
		private bool IsEnabled = true;

		public void Play(in CueContext pContext, in SOCue pParent)
		{
			if (IsEnabled)
			{
				PlayInternal(pContext, pParent);
			}
		}

		protected virtual void PlayInternal(in CueContext pContext, in SOCue pParent) { }
	}
}