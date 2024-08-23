using UnityEngine;

namespace ODev.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Spread/None")]
	public class SOWeaponSpreadNone : SOWeaponSpreadBase
	{
		public override Vector3 ApplySpread(Vector3 pDirection) => pDirection;
		public override void OnShoot() { }
		public override void OnUpdate(in float pDeltaTime) { }
	}
}
