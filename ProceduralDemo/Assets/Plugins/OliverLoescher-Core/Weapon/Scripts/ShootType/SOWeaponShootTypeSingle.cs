using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/ShootType/Single")]
	public class SOWeaponShootTypeSingle : SOWeaponShootTypeBase
	{
		public override void ShootStart()
		{
			shoot.Invoke();
		}
		public override void ShootEnd() { }
		public override void OnUpdate(float pDeltaTime) { }
		public override void OnShoot() { }
	}
}