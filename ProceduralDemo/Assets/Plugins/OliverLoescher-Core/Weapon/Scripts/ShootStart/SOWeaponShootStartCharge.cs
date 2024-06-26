using System.Collections;
using UnityEngine;

namespace OCore.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Start/Charge")]
	public class SOWeaponShootStartCharge : SOWeaponShootStartBase
	{
		[SerializeField]
		private float m_ChargeSeconds = 0;

		private Coroutine m_Coroutine = null;

		public override void ShootStart()
		{
			Util.Mono.Stop(ref m_Coroutine);
			m_Coroutine = Util.Mono.Start(ShootDelayed());
		}
		public override void ShootEnd()
		{
			Util.Mono.Stop(ref m_Coroutine);
		}

		public IEnumerator ShootDelayed()
		{
			yield return new WaitForSeconds(m_ChargeSeconds);
			ShootType.ShootStart();
		}
	}
}