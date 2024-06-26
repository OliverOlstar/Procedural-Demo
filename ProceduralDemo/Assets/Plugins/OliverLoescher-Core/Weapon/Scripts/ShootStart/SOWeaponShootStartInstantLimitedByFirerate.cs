using UnityEngine;

namespace OCore.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Start/InstantLimitedByFirerate")]
	public class SOWeaponShootStartInstantLimitedByFirerate : SOWeaponShootStartBase
	{
		public override void ShootStart()
		{
			if (ShootType.NextCanShootTime <= Time.time)
			{
				ShootType.ShootStart();
			}
		}
	}
}