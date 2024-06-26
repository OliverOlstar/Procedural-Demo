using UnityEngine;

namespace OCore.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/ShootType/Single")]
	public class SOWeaponShootTypeSingle : SOWeaponShootTypeBase
	{
		public override void ShootStart()
		{
			m_Shoot.Invoke();
		}
		public override void ShootEnd() { }
		public override void OnUpdate(float pDeltaTime) { }
		public override void OnShoot() { }
	}
}