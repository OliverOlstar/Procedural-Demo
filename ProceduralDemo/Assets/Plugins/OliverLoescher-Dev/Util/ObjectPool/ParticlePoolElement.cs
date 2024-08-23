using UnityEngine;
using Sirenix.OdinInspector;

namespace ODev
{
	[RequireComponent(typeof(ParticleSystem))]
	public class ParticlePoolElement : PoolElement
	{
		[InfoBox("Ensure particle stopping action is Callback")]
		private ParticleSystem m_Particle = null;

		public override void Init(string pPoolKey, Transform pParent)
		{
			base.Init(pPoolKey, pParent);
			m_Particle = GetComponent<ParticleSystem>();
			SetStoppingAction(m_Particle);
		}

		private void Reset()
		{
			SetStoppingAction(GetComponent<ParticleSystem>());
		}

		private void SetStoppingAction(in ParticleSystem pParticle)
		{
			ParticleSystem.MainModule main = pParticle.main;
			main.stopAction = ParticleSystemStopAction.Callback;
		}

		public void OnParticleSystemStopped()
		{
			ReturnToPool();
		}

		public override void ReturnToPool()
		{
			base.ReturnToPool();
			m_Particle.Stop();
		}

		public override void OnExitPool()
		{
			base.OnExitPool();
		}
	}
}