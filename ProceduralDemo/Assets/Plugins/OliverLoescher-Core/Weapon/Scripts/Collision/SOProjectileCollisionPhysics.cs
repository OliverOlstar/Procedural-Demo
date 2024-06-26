using UnityEngine;

namespace OCore.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Collision/Physics")]
	public class SOProjectileCollisionPhysics : SOProjectileCollisionBase
	{
		public override bool DoCollision(Projectile pProjectile, Collider pOther, ref bool rCanDamage, ref bool rActiveSelf)
		{
			pProjectile.MyRigidbody.useGravity = true;
			pProjectile.HitboxCollider.enabled = false;
			pProjectile.PhysicsCollider.enabled = true;
			pProjectile.transform.position += pProjectile.MyRigidbody.velocity.normalized * -0.25f;
			rActiveSelf = false;
			base.DoCollision(pProjectile, pOther, ref rCanDamage, ref rActiveSelf);
			return false;
		}
	}
}