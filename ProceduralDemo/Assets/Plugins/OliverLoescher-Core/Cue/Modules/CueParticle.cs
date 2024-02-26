using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher.Cue
{
	[System.Serializable]
    public class CueParticle : CueModule
	{
		[SerializeField]
		private ParticlePoolElement[] RandomParticles = new ParticlePoolElement[0];

		protected override void PlayInternal(in CueContext pContext, in SOCue pParent)
		{
			if (RandomParticles.IsNullOrEmpty())
			{
				Util.Debug.LogWarning("IsEnabled is true but RandomParticles is empty", "PlayInternal", pParent);
				return;
			}
			int index = Random.Range(0, RandomParticles.Length - 1);
			ObjectPoolDictionary.Play(RandomParticles[index], pContext.Point, Quaternion.LookRotation(Vector3.up));
		}
	}
}
