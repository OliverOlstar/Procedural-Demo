using ODev.Util;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ODev.Cue
{
	[System.Serializable]
    public class CueParticle : CueModule
	{
		[SerializeField]
		private ParticlePoolElement[] m_RandomParticles = new ParticlePoolElement[0];

		protected override void PlayInternal(in CueContext pContext, in SOCue pParent)
		{
			if (m_RandomParticles.IsNullOrEmpty())
			{
				pParent.LogWarning("IsEnabled is true but RandomParticles is empty");
				return;
			}
			int index = UnityEngine.Random.Range(0, m_RandomParticles.Length - 1);
			ObjectPoolDictionary.Play(m_RandomParticles[index], pContext.Point, Quaternion.LookRotation(Vector3.up));
		}
	}
}
