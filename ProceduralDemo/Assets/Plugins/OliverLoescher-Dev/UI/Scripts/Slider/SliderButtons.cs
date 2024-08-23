using UnityEngine;
using UnityEngine.UI;

namespace ODev.UI
{
	[RequireComponent(typeof(Slider))]
    public class SliderButtons : MonoBehaviour
    {
		[SerializeField, Min(Util.Math.NEARZERO)]
		private float m_IncreaseDelta = 0.1f;
		[SerializeField, Min(Util.Math.NEARZERO)]
		private float m_DecreaseDelta = 0.1f;

		[SerializeField]
		private Button m_ButtonIncrease = null;
		[SerializeField]
		private Button m_ButtonDecrease = null;

		private Slider m_Slider = null;

		private void Awake()
		{
			m_ButtonIncrease.onClick.AddListener(OnIncreaseClicked);
			m_ButtonDecrease.onClick.AddListener(OnDecreaseClicked);
			m_Slider = GetComponent<Slider>();
		}

		private void OnDestroy()
		{
			m_ButtonIncrease.onClick.RemoveListener(OnIncreaseClicked);
			m_ButtonDecrease.onClick.RemoveListener(OnDecreaseClicked);
		}

		protected virtual void OnIncreaseClicked() => m_Slider.value += m_IncreaseDelta;
		protected virtual void OnDecreaseClicked() => m_Slider.value -= m_DecreaseDelta;

		public void SetDelta(float pIncrease, float pDecrease)
		{
			m_IncreaseDelta = Mathf.Max(0.0f, pIncrease);
			m_DecreaseDelta = Mathf.Max(0.0f, pDecrease);
		}
	}
}
