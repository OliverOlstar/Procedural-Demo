using UnityEngine;
using Sirenix.OdinInspector;

namespace OCore.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Projectile Data")]
	public class SOProjectile : ScriptableObject
	{
		// public enum BulletExplosion
		// {
		//	 Null,
		//	 ExplodeOnHit,
		//	 ExplodeOnDeath
		// }

		// public enum BulletHoming
		// {
		//	 Null,
		//	 HomingDamageables,
		//	 HomingRigidbodies
		// }

		// public enum BulletHomingMovement
		// {
		//	 RotateVelocity,
		//	 AddForce
		// }

		[Title("Raycast")]
		[SerializeField]
		private bool m_UseRaycast = false;
		[SerializeField, ShowIf("@useRaycast")]
		private LayerMask m_LayerMask = new();

		[Title("Damage")]
		[SerializeField]
		private int m_Damage = 1;
		[SerializeField, Range(0, 1)]
		private float m_CritChance01 = 0.1f;
		[SerializeField, HideIf("@critChance01 == 0")]
		private float m_CritDamageMultiplier = 2;

		[Title("Stats")]
		[SerializeField]
		private Vector2 m_LifeTime = new(4.0f, 4.5f);
		[SerializeField]
		private Vector2 m_ShootForce = new(5.0f, 5.0f);
		[SerializeField]
		private float m_BulletGravity = 0.0f;

		[Header("Collision")]
		[SerializeField, Required, InlineEditor]
		private SOProjectileCollisionBase m_ProjectileEnviromentCollision = null;
		[SerializeField, Required, InlineEditor]
		private SOProjectileCollisionBase m_ProjectileDamagableCollision = null;
		[SerializeField, Required, InlineEditor]
		private SOProjectileCollisionBase m_ProjectileLifeEnd = null;
		[SerializeField]
		private float m_HitForce = 8;

		public bool UseRaycast => m_UseRaycast;
		public LayerMask LayerMask => m_LayerMask;
		public int Damage => m_Damage;
		public float CritChance01 => m_CritChance01;
		public float CritDamageMultiplier => m_CritDamageMultiplier;
		public Vector2 LifeTime => m_LifeTime;
		public Vector2 ShootForce => m_ShootForce;
		public float BulletGravity => m_BulletGravity;
		public SOProjectileCollisionBase ProjectileEnviromentCollision => m_ProjectileEnviromentCollision;
		public SOProjectileCollisionBase ProjectileDamagableCollision => m_ProjectileDamagableCollision;
		public SOProjectileCollisionBase ProjectileLifeEnd => m_ProjectileLifeEnd;
		public float HitForce => m_HitForce;
	}
}