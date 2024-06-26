using UnityEngine;

namespace OCore.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Collision/Penetrate")]
	public class SOProjectileCollisionPenetrate : SOProjectileCollisionBase
	{
		public override bool DoCollision(Projectile pProjectile, Collider pOther, ref bool rCanDamage, ref bool rActiveSelf)
		{
			if (pOther.gameObject.isStatic)
			{
				pProjectile.MyRigidbody.isKinematic = true;
				rCanDamage = false;
				base.DoCollision(pProjectile, pOther, ref rCanDamage, ref rActiveSelf);
				return true;
			}
			return false;
		}
	}
}
