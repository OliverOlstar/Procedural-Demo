using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Collision/Physics")]
	public class SOProjectileCollisionPhysics : SOProjectileCollisionBase
	{
		public override bool DoCollision(Projectile pProjectile, Collider pOther, ref bool canDamage, ref bool activeSelf)
		{
			pProjectile.myRigidbody.useGravity = true;
			pProjectile.hitboxCollider.enabled = false;
			pProjectile.physicsCollider.enabled = true;
			pProjectile.transform.position += pProjectile.myRigidbody.velocity.normalized * -0.25f;
			activeSelf = false;
			base.DoCollision(pProjectile, pOther, ref canDamage, ref activeSelf);
			return false;
		}
	}
}