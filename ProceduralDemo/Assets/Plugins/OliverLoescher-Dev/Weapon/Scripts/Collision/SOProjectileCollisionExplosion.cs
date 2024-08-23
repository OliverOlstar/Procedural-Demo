using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ODev.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Collision/Explosion")]
	public class SOProjectileCollisionExplosion : SOProjectileCollisionBase
	{
		[Space, SerializeField, Min(0.1f)]
		private float m_ExplosionRadius = 5.0f;
		[SerializeField, Min(0)]
		private int m_ExplosionDamage = 5;
		[SerializeField, Min(0.0f)]
		protected float m_ExplosionForce = 2000.0f;
		[SerializeField, Min(0.0f)]
		private float m_ExplosiveUpwardsModifier = 1;

		public override bool DoCollision(Projectile pProjectile, Collider pOther, ref bool rCanDamage, ref bool rActiveSelf)
		{
			pProjectile.MyRigidbody.isKinematic = true;
			rCanDamage = false;
			base.DoCollision(pProjectile, pOther, ref rCanDamage, ref rActiveSelf);
			Explode(pProjectile.transform.position, pProjectile);
			return true;
		}

		public void Explode(Vector3 pPoint, Projectile pProjectile)
		{
			Collider[] hits = new Collider[0];
			Physics.OverlapSphereNonAlloc(pPoint, m_ExplosionRadius, hits);
			foreach (Collider hit in hits)
			{
				if (hit.TryGetComponent(out Rigidbody rb))
				{
					rb.AddExplosionForce(m_ExplosionForce, pPoint, m_ExplosionRadius, m_ExplosiveUpwardsModifier);
				}
				IDamageable damageable = hit.GetComponent<IDamageable>();
				List<IDamageable> hitDamagables = ListPool<IDamageable>.Get();
				if (damageable != null && !hitDamagables.Contains(damageable.GetParentDamageable()))
				{
					hitDamagables.Add(damageable.GetParentDamageable());
					damageable.Damage(m_ExplosionDamage, pProjectile.Sender, pPoint, (hit.transform.position - pPoint).normalized);
				}
				ListPool<IDamageable>.Release(hitDamagables);
			}
		}

		public override void DrawGizmos(Projectile pProjectile)
		{
			Gizmos.DrawWireSphere(pProjectile.transform.position, m_ExplosionRadius);
		}
	}
}
