using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Start/Charge")]
	public class SOWeaponShootStartCharge : SOWeaponShootStartBase
	{
		[SerializeField]
		private float chargeSeconds = 0;

		private Coroutine coroutine = null;

		public override void ShootStart()
		{
			Util.Mono.Stop(ref coroutine);
			coroutine = Util.Mono.Start(ShootDelayed());
		}
		public override void ShootEnd()
		{
			Util.Mono.Stop(ref coroutine);
		}

		public IEnumerator ShootDelayed()
		{
			yield return new WaitForSeconds(chargeSeconds);
			shootType.ShootStart();
		}
	}
}