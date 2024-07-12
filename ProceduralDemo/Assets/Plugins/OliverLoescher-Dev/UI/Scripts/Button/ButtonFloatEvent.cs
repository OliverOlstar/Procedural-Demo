using ODev.Util;
using UnityEngine;
using UnityEngine.UI;

namespace ODev.UI
{
	public class ButtonFloatEvent : MonoBehaviour
	{
		[SerializeField]
		private float m_Value = 0.0f;
		[SerializeField, Min(Math.NEARZERO)]
		private float m_IncreaseDelta = 0.1f;
		[SerializeField, Min(Math.NEARZERO)]
		private float m_DecreaseDelta = 0.1f;
		[SerializeField]
		private Vector2 m_Clamp = new(0.0f, 1.0f);

		[Space, SerializeField]
		private Button m_ButtonIncrease = null;
		[SerializeField]
		private Button m_ButtonDecrease = null;

		[Space]
		public UnityEventsUtil.FloatEvent OnChange;

		public float Value => m_Value;

		protected virtual void Awake()
		{
			m_ButtonIncrease.onClick.AddListener(OnIncreaseClicked);
			m_ButtonDecrease.onClick.AddListener(OnDecreaseClicked);
		}

		protected virtual void OnDestroy()
		{
			m_ButtonIncrease.onClick.RemoveListener(OnIncreaseClicked);
			m_ButtonDecrease.onClick.RemoveListener(OnDecreaseClicked);
		}

		protected virtual void OnValidate()
		{
			m_Value = Math.Clamp(m_Value, m_Clamp);
		}

		protected virtual void OnIncreaseClicked() => ModifyValue(m_IncreaseDelta);
		protected virtual void OnDecreaseClicked() => ModifyValue(-m_DecreaseDelta);
		protected virtual void ModifyValue(float pDelta)
		{
			m_Value = (m_Value + pDelta).Clamp(m_Clamp);
			OnChange.Invoke(m_Value);
		}
	}
}
