using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Collision/Destroy")]
	public class SOProjectileCollisionDestroy : SOProjectileCollisionBase
	{
		public override bool DoCollision(Projectile projectile, Collider other, ref bool canDamage, ref bool activeSelf)
		{
			projectile.myRigidbody.isKinematic = true;
			canDamage = false;

			if (particlePrefab != null)
			{
				ObjectPoolDictionary.Play(particlePrefab, projectile.transform.position, projectile.transform.rotation);
			}
			audio.Play(projectile.transform.position);

			if (other != null)
			{
				Rigidbody rb = other.GetComponent<Rigidbody>();
				if (rb != null)
				{
					rb.AddForce(projectile.transform.forward * knockbackForce, ForceMode.Impulse);
				}
			}
			return true;
		}
	}
}
