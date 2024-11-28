using UnityEngine;

namespace ODev.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Start/InstantLimitedByFirerate")]
	public class SOWeaponShootStartInstantLimitedByFirerate : SOWeaponShootStartBase
	{
		public override void ShootStart()
		{
			if (ShootType.NextCanShootTime <= UnityEngine.Time.time)
			{
                ShootType.ShootStart();
			}
		}
	}
}