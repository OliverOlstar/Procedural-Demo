using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace OCore.UI
{
	[RequireComponent(typeof(Button))]
	public class ButtonText : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text m_Text = null;
		private Button m_Button;

		[SerializeField]
		private string[] m_Strings = new string[1];
		private int m_Index = 0;

		private void Awake()
		{
			m_Button = GetComponent<Button>();
			m_Button.onClick.AddListener(OnClick);
		}

		public void Reset()
		{
			if (m_Text == null || m_Strings.Length == 0)
			{
				return;
			}
			m_Text.text = m_Strings[^1];
		}

		private void OnDestroy()
		{
			m_Button.onClick.RemoveListener(OnClick);
		}

		public void OnClick()
		{
			m_Index++;
			if (m_Index == m_Strings.Length)
			{
				m_Index = 0;
			}
			m_Text.text = m_Strings[m_Index];
		}
	}
}