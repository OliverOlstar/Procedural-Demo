using UnityEngine;

namespace ODev.Cue
{
	[System.Serializable]
	public class CueModule
	{
		[SerializeField]
		private bool m_IsEnabled = true;

		public void Play(in CueContext pContext, in SOCue pParent)
		{
			if (m_IsEnabled)
			{
				PlayInternal(pContext, pParent);
			}
		}

		protected virtual void PlayInternal(in CueContext pContext, in SOCue pParent) { }
	}
}