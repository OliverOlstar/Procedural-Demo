using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ODev.UI
{
	[RequireComponent(typeof(Button))]
	public class ButtonToggleEvent : MonoBehaviour
	{
		private Button m_Button;
		public UnityEventsUtil.BoolEvent m_OnToggle;
		public UnityEvent m_OnDotoggle;
		public UnityEvent m_OnUntoggle;
		private bool m_Toggle = false;

		private void Awake()
		{
			m_Button = GetComponent<Button>();
			m_Button.onClick.AddListener(OnClick);
		}

		private void OnDestroy()
		{
			m_Button.onClick.RemoveListener(OnClick);
		}

		public void OnClick()
		{
			m_Toggle = !m_Toggle;
			m_OnToggle.Invoke(m_Toggle);
			if (m_Toggle)
			{
				m_OnDotoggle.Invoke();
			}
			else
			{
				m_OnUntoggle.Invoke();
			}
		}
	}
}
