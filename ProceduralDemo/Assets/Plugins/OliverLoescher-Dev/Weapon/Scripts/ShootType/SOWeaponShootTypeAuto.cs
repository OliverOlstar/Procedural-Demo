using UnityEngine;

namespace ODev.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/ShootType/Auto")]
	public class SOWeaponShootTypeAuto : SOWeaponShootTypeBase
	{
		[SerializeField, Min(0.0f)]
		private float m_SecondsBetweenShots = 0.1f;

		private bool m_IsShooting = false;

		public override void ShootStart()
		{
			m_IsShooting = true;
			m_Shoot.Invoke();
		}

		public override void ShootEnd()
		{
			m_IsShooting = false;
		}

		public override void OnUpdate(float pDeltaTime)
		{
			if (m_IsShooting && Time.time >= NextCanShootTime)
			{
				m_Shoot.Invoke();
			}
		}

		public override void OnShoot()
		{
			NextCanShootTime = Time.time + m_SecondsBetweenShots;
		}
	}
}