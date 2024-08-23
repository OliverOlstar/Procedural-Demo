using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ODev.Weapon.Multiplayer
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Weapon Multiplayer Data")]
	public class SOWeaponMultiplayer : SOWeapon
	{
		[Title("Multiplayer")]
		public GameObject dummyPrefab = null;
	}
}