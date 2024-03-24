using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Collision/Penetrate")]
	public class SOProjectileCollisionPenetrate : SOProjectileCollisionBase
	{
		public override bool DoCollision(Projectile pProjectile, Collider pOther, ref bool canDamage, ref bool activeSelf)
		{
			if (pOther.gameObject.isStatic)
			{
				pProjectile.myRigidbody.isKinematic = true;
				canDamage = false;
				base.DoCollision(pProjectile, pOther, ref canDamage, ref activeSelf);
				return true;
			}
			return false;
		}
	}
}
