using UnityEngine;

namespace OCore.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/ShootType/Burst")]
	public class SOWeaponShootTypeBurst : SOWeaponShootTypeBase
	{
		[SerializeField, Min(2)]
		private int m_ShootCount = 3;
		[SerializeField, Min(0.0f)]
		private float m_SecondsBetweenShots = 0.1f;
		[SerializeField, Min(0.0f)]
		private float m_SecondsBetweenBurstShots = 0.1f;

		private bool m_IsShooting = false;
		private int m_ActiveCount = 0;

		public override void ShootStart()
		{
			if (m_IsShooting)
			{
				return;
			}
			m_IsShooting = true;

			m_ActiveCount = m_ShootCount;
			m_Shoot.Invoke();
		}

		public override void ShootEnd() { }

		public override void OnUpdate(float pDeltaTime)
		{
			if (m_IsShooting && Time.time >= NextCanShootTime)
			{
				m_Shoot.Invoke();
			}
		}

		public override void OnShoot()
		{
			m_ActiveCount--;
			if (m_ActiveCount > 0)
			{
				NextCanShootTime = Time.time + m_SecondsBetweenBurstShots;
				return;
			}
			NextCanShootTime = Time.time + m_SecondsBetweenShots;
			m_IsShooting = false;
		}
	}
}