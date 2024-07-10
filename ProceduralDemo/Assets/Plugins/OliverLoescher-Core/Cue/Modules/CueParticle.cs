using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OCore.Cue
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
				Util.Debug.LogWarning("IsEnabled is true but RandomParticles is empty", pParent);
				return;
			}
			int index = Random.Range(0, m_RandomParticles.Length - 1);
			ObjectPoolDictionary.Play(m_RandomParticles[index], pContext.Point, Quaternion.LookRotation(Vector3.up));
		}
	}
}
