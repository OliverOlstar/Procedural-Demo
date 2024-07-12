using UnityEngine;

namespace ODev
{
	[RequireComponent(typeof(Health))]
	public class HealthDamageNumbers : MonoBehaviour
	{
		[SerializeField]
		private Transform m_Transform = null;
		[SerializeField]
		private Vector3 m_Offset = Vector3.up * 2;

		private Health m_Health;

		private void Awake()
		{
			if (m_Transform == null)
			{
				m_Transform = gameObject.transform;
			}

			m_Health = GetComponent<Health>();
			m_Health.OnValueLoweredEvent.AddListener(OnDamaged);
			m_Health.OnValueRaisedEvent.AddListener(OnHealed);
			m_Health.OnValueOutEvent.AddListener(OnDeath);
		}

		public void OnDamaged(float pValue, float pChange)
		{
			DisplayNumber(Mathf.CeilToInt(pChange), m_Health.m_DamageColor);
		}

		public void OnHealed(float pValue, float pChange)
		{
			DisplayNumber(Mathf.CeilToInt(pChange), m_Health.m_HealColor);
		}

		public void OnDeath()
		{
			DisplayNumber("DEATH", m_Health.m_DeathColor);
		}

		public void DisplayNumber(int pValue, Color pColor)
		{
			DisplayNumber(Mathf.Abs(pValue).ToString(), pColor);
		}

		public void DisplayNumber(string pText, Color pColor)
		{
			DamageNumbers.Instance.DisplayNumber(pText, m_Transform.position + m_Offset, pColor);
		}
	}
}