using UnityEngine;
using Sirenix.OdinInspector;

namespace OCore
{
	[RequireComponent(typeof(Health))]
	public class DamageFlash : MonoBehaviour
	{
		private Health m_Health = null;

		[SerializeField, DisableInPlayMode]
		private Renderer[] m_Renderers;
		private Color[] m_InitalColors;

		[Header("Time")]
		[SerializeField, Range(Util.Math.NEARZERO, 1.0f)]
		private float flashSeconds = 0.1f;
		[SerializeField, Range(Util.Math.NEARZERO, 5.0f)]
		private float flashDeathSeconds = 0.1f;

		private void Start()
		{
			m_Health = GetComponent<Health>();

			m_Health.OnValueLoweredEvent.AddListener(OnDamaged);
			m_Health.OnValueRaisedEvent.AddListener(OnHealed);
			m_Health.OnValueOutEvent.AddListener(OnDeath);

			m_InitalColors = new Color[m_Renderers.Length];
			for (int i = 0; i < m_Renderers.Length; i++)
			{
				if (m_Renderers[i].material.HasProperty("_Color"))
				{
					m_InitalColors[i] = m_Renderers[i].material.color;
				}
			}
		}

		public void OnDamaged(float pValue, float pChange) { SetColor(m_Health.m_DamageColor, flashSeconds); }
		public void OnHealed(float pValue, float pChange) { SetColor(m_Health.m_HealColor, flashSeconds); }
		public void OnDeath() { SetColor(m_Health.m_DeathColor, flashDeathSeconds); }

		public void SetColor(Color pColor, float pSeconds)
		{
			for (int i = 0; i < m_Renderers.Length; i++)
			{
				if (m_Renderers[i].material.HasProperty("_Color"))
				{
					m_Renderers[i].material.color = pColor;
				}
			}
			CancelInvoke();
			Invoke(nameof(ResetColor), pSeconds);
		}

		private void ResetColor()
		{
			for (int i = 0; i < m_Renderers.Length; i++)
			{
				m_Renderers[i].material.color = m_InitalColors[i];
			}
		}
	}
}
