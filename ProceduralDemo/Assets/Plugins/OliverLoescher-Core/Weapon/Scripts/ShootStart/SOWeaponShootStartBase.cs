using UnityEngine;
using System;
using Sirenix.OdinInspector;

namespace OCore.Weapon
{
	public abstract class SOWeaponShootStartBase : ScriptableObject
	{
		[SerializeField, Required, InlineEditor]
		private SOWeaponShootTypeBase m_ShootType = null;

		public SOWeaponShootTypeBase ShootType => m_ShootType;

		public virtual void ShootStart() => m_ShootType.ShootStart();
		public virtual void ShootEnd() => m_ShootType.ShootEnd();
		public virtual void OnUpdate(float pDelta) => m_ShootType.OnUpdate(pDelta);
		public virtual void OnShoot() => m_ShootType.OnShoot();

		public virtual SOWeaponShootStartBase Init(Action pShoot)
		{
			m_ShootType = Instantiate(m_ShootType).Init(pShoot);
			return this;
		}
	}
}