using UnityEngine;

namespace ODev.Weapon
{
	public abstract class SOProjectileCollisionBase : ScriptableObject
	{
		[SerializeField]
		protected PoolElement m_ParticlePrefab = null;
		[SerializeField]
		protected Util.Audio.AudioPiece m_Audio = null;
		[SerializeField, Min(0.0f)]
		protected float m_KnockbackForce = 300.0f;

		public virtual bool DoCollision(Projectile pProjectile, Collider pOther, ref bool rCanDamage, ref bool rActiveSelf)
		{
			if (pOther != null && pOther.TryGetComponent(out Rigidbody rb))
			{
				rb.AddForce(pProjectile.transform.forward * m_KnockbackForce, ForceMode.Impulse);
			}
			ObjectPoolDictionary.Play(m_ParticlePrefab, pProjectile.transform.position, pProjectile.transform.rotation);
			m_Audio.Play(pProjectile.transform.position);
			return true;
		}

		public virtual void DrawGizmos(Projectile pProjectile) { }
	}
}