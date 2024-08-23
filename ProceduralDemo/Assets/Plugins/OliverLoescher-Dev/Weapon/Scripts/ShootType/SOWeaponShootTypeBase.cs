using UnityEngine;
using System;

namespace ODev.Weapon
{
	public abstract class SOWeaponShootTypeBase : ScriptableObject
	{
		[HideInInspector]
		public float NextCanShootTime { get; protected set; } = 0.0f;
		
		protected Action m_Shoot = null;

		public abstract void ShootStart();
		public abstract void ShootEnd();
		public abstract void OnUpdate(float pDeltaTime);
		public abstract void OnShoot();

		public virtual SOWeaponShootTypeBase Init(Action pShoot)
		{
			m_Shoot = pShoot;
			return this;
		}
	}
}