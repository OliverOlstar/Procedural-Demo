using UnityEngine;

namespace ODev.Weapon
{
	public abstract class SOWeaponSpreadExpandBase : SOWeaponSpreadBase
	{
		[SerializeField, Range(0.0f, 1.0f)]
		private float m_SpreadIncrease = 0.4f;
		[SerializeField, Range(Util.Math.NEARZERO, 3)]
		private float m_SpreadDecrease = 0.6f;

		protected float m_Spread01 = 0.0f;

		public override void OnShoot()
		{
			m_Spread01 = Mathf.Min(1, m_Spread01 + m_SpreadIncrease);
		}

		public override void OnUpdate(in float pDeltaTime)
		{
			m_Spread01 = Mathf.Max(0, m_Spread01 - (Time.deltaTime * m_SpreadDecrease));
		}
	}
}