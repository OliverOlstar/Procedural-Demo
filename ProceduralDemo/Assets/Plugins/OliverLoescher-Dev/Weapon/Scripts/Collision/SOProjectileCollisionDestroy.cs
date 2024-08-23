using UnityEngine;

namespace ODev.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Collision/Destroy")]
	public class SOProjectileCollisionDestroy : SOProjectileCollisionBase
	{
		public override bool DoCollision(Projectile projectile, Collider other, ref bool rCanDamage, ref bool rActiveSelf)
		{
			projectile.MyRigidbody.isKinematic = true;
			rCanDamage = false;

			if (m_ParticlePrefab != null)
			{
				ObjectPoolDictionary.Play(m_ParticlePrefab, projectile.transform.position, projectile.transform.rotation);
			}
			m_Audio.Play(projectile.transform.position);

			if (other != null)
			{
				if (other.TryGetComponent(out Rigidbody rigidbody))
				{
					rigidbody.AddForce(projectile.transform.forward * m_KnockbackForce, ForceMode.Impulse);
				}
			}
			return true;
		}
	}
}
