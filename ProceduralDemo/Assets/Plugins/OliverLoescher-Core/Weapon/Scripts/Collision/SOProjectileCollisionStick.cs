using UnityEngine;

namespace OCore.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Collision/Stick")]
	public class SOProjectileCollisionStick : SOProjectileCollisionBase
	{
		public override bool DoCollision(Projectile pProjectile, Collider pOther, ref bool rCanDamage, ref bool rActiveSelf)
		{
			pProjectile.MyRigidbody.isKinematic = true;
			if (!pOther.gameObject.isStatic)
			{
				pProjectile.transform.SetParent(pOther.transform);
			}
			rCanDamage = false;
			rActiveSelf = false;
			base.DoCollision(pProjectile, pOther, ref rCanDamage, ref rActiveSelf);
			return false;
		}
	}
}
