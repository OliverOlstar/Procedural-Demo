using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher.Cue
{
	[System.Serializable]
    public class CueParticle : CueModule
	{
		public override void Play(CueContext pContext)
		{
			Util.Debug.LogError("Not Implemented", "Play", typeof(CueParticle));
		}
	}
}
