using UnityEngine;
using UnityEngine.UI;

namespace OCore
{
	public class HUDDamageFlash : MonoBehaviour
	{
		[SerializeField]
		private Image m_Image = null;
		[SerializeField, Range(Util.Math.NEARZERO, 1.0f)]
		private float m_FlashSeconds = 0.1f;

		private Color m_InitalColor;
		private Health m_Health;

		private void Start()
		{
			m_InitalColor = m_Image.color;
			m_Image.gameObject.SetActive(false);

			m_Health = GetComponent<Health>();

			m_Health.OnValueLoweredEvent.AddListener(OnDamaged);
			m_Health.OnValueRaisedEvent.AddListener(OnHealed);
		}

		public void OnDamaged(float pValue, float pChange)
		{
			SetColor(Color.Lerp(m_Health.m_DamageColor, m_Health.m_DeathColor, 1 - (m_Health.Value / m_Health.MaxValue)), m_FlashSeconds);
		}

		public void OnHealed(float pValue, float pChange)
		{
			SetColor(m_Health.m_HealColor, m_FlashSeconds);
		}

		public void SetColor(Color pColor, float pSeconds)
		{
			m_Image.color = pColor;
			m_Image.gameObject.SetActive(true);

			CancelInvoke();
			Invoke(nameof(ResetColor), pSeconds);
		}

		private void ResetColor()
		{
			m_Image.color = m_InitalColor;
			m_Image.gameObject.SetActive(false);
		}
	}
}