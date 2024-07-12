using UnityEngine;
using Sirenix.OdinInspector;
using ODev.Util;

namespace ODev.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Weapon Data")]
	public class SOWeapon : ScriptableObject
	{
		public enum BulletType 
		{
			Projectile,
			RaycastProjectile,
			Raycast
		}

		public enum AmmoType
		{
			Limited,
			Unlimited,
			Null
		}

		[Title("Display")]
		[SerializeField]
		private string m_DisplayName = "Untitled";
		[SerializeField, TextArea]
		private string m_Description = "";

		[Title("Type")]
		[SerializeField]
		private BulletType m_MyBulletType = BulletType.Projectile;

		[SerializeField, Min(1)]
		private int m_ProjectilesPerShot = 1;

		[SerializeField, ShowIfGroup("Proj", Condition = "@bulletType != BulletType.Raycast"), TitleGroup("Proj/Projectile")]
		private GameObject m_ProjectilePrefab = null;

		[SerializeField, ShowIfGroup("Ray", Condition = "@bulletType == BulletType.Raycast"), TitleGroup("Ray/Raycast")]
		private float m_Range = 5.0f;
		[SerializeField, TitleGroup("Ray/Raycast"), AssetsOnly]
		private GameObject m_HitFXPrefab = null;
		[SerializeField, ShowIf("@bulletType != BulletType.Projectile")]
		private LayerMask m_LayerMask = new();

		[Title("Stats")]
		[SerializeField, Required, InlineEditor]
		private SOWeaponShootStartBase m_ShootStart = null;

		[Header("Spread")]
		[SerializeField, Required, InlineEditor]
		private SOWeaponSpreadBase m_Spread = null;

		[Header("Force")]
		[SerializeField]
		private Vector3 m_RecoilForce = Vector3.zero;

		[Title("Ammo")]
		[SerializeField]
		private AmmoType m_MyAmmoType = AmmoType.Null;
		[SerializeField, ShowIf("@ammoType == AmmoType.Limited"), MinValue("@clipAmmo")]
		private int m_TotalAmmo = 12;
		[SerializeField, ShowIf("@ammoType != AmmoType.Null"), Min(1)]
		private int m_ClipAmmo = 4;

		[Space]
		[SerializeField, ShowIf("@ammoType != AmmoType.Null"), Min(0)]
		private float m_ReloadDelaySeconds = 0.6f;
		[SerializeField, ShowIf("@ammoType != AmmoType.Null"), Min(0.01f)]
		private float m_ReloadIntervalSeconds = 0.2f;

		[Title("Audio")]
		[SerializeField]
		private Audio.AudioPiece m_ShotSound = new();
		[SerializeField]
		private Audio.AudioPiece m_FailedShotSound = new();

		[Space]
		[SerializeField, ShowIf("@ammoType != AmmoType.Null")]
		private Audio.AudioPiece m_ReloadSound = new();
		[SerializeField, ShowIf("@ammoType != AmmoType.Null")]
		private Audio.AudioPiece m_OutOfAmmoSound = new();
		[SerializeField, ShowIf("@ammoType != AmmoType.Null")]
		private Audio.AudioPiece m_OnReloadedSound = new();

		public string DisplayName => m_DisplayName;
		public string Description => m_Description;
		public BulletType MyBulletType => m_MyBulletType;
		public int ProjectilesPerShot => m_ProjectilesPerShot;
		public GameObject ProjectilePrefab => m_ProjectilePrefab;
		public float Range => m_Range;
		public GameObject HitFXPrefab => m_HitFXPrefab;
		public LayerMask LayerMask => m_LayerMask;
		public SOWeaponShootStartBase ShootStart => m_ShootStart;
		public SOWeaponSpreadBase Spread => m_Spread;
		public Vector3 RecoilForce => m_RecoilForce;
		public AmmoType MyAmmoType => m_MyAmmoType;
		public int TotalAmmo => m_TotalAmmo;
		public int ClipAmmo => m_ClipAmmo;
		public float ReloadDelaySeconds => m_ReloadDelaySeconds;
		public float ReloadIntervalSeconds => m_ReloadIntervalSeconds;
		public Audio.AudioPiece ShotSound => m_ShotSound;
		public Audio.AudioPiece FailedShotSound => m_FailedShotSound;
		public Audio.AudioPiece ReloadSound => m_ReloadSound;
		public Audio.AudioPiece OutOfAmmoSound => m_OutOfAmmoSound;
		public Audio.AudioPiece OnReloadedSound => m_OnReloadedSound;
	}
}