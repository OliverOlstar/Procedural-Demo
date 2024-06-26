using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace OCore.UI
{
	[RequireComponent(typeof(Slider))]
	public class SliderText : MonoBehaviour
	{
		// https://docs.microsoft.com/en-us/previous-versions/dotnet/netframework-3.0/dwhawy9k(v=vs.85)?redirectedfrom=MSDN
		[Tooltip("Xn - n number of decimal place \nX format - F (fixed-point), D (decimal), C (currency) P (percent), etc")]
		public string TextFormat = "F2";
		public string PreText = "";
		public string PostText = "";
		public bool AddPlusIfPositive = false;

		[Space, SerializeField] 
		private TMP_Text m_Text = null;

		private Slider m_Slider = null;

		void Awake()
		{
			m_Slider = GetComponent<Slider>();
			m_Slider.onValueChanged.AddListener(OnValueChanged);
			OnValueChanged(m_Slider.value);
		}

		private void OnDestroy()
		{
			m_Slider.onValueChanged.RemoveListener(OnValueChanged);
		}

		private void OnValidate()
		{
			if (m_Slider == null && !TryGetComponent(out m_Slider))
			{
				return;
			}
			OnValueChanged(m_Slider.value);
		}

		public void OnValueChanged(float pValue)
		{
			string plus = string.Empty;
			if (AddPlusIfPositive && pValue > 0.0f)
			{
				plus = "+";
			}
			m_Text.text = $"{PreText}{plus}{pValue.ToString(TextFormat)}{PostText}";
		}
	}
}