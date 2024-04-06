using UnityEngine;
using UnityEngine.UI;

public class UnimaFromToPercentContext : IUnimaContext
{
	[SerializeField]
	private float m_From;
	public float FromValue { get { return m_From; } }

	[SerializeField]
	private float m_To;
	public float ToValue { get { return m_To; } }

	[SerializeField]
	private float m_Duration;
	public float Duration { get { return m_Duration; } }

	public UnimaFromToPercentContext(float fromValue, float toValue, float duration = 0f)
	{
		m_From = fromValue;
		m_To = toValue;
		m_Duration = duration;
	}

}

[CreateAssetMenu(menuName = "Unimate/Core/Animate Slider")]
public class UnimateAnimateSlider : UnimateTween<UnimateAnimateSlider, UnimateAnimateSlider.Player>
{
	[SerializeField]
	private float m_Duration = 1.0f;
	public override float Duration => m_Duration;

	public override bool UpdateBeforeStart => false;
	public override bool Loop => false;

	protected override string OnEditorValidate(GameObject gameObject)
	{
		return gameObject.GetComponent<Slider>() == null ? $"Requires Slider component" : null;
	}

	private void SetDuration(float duration)
	{
		m_Duration = duration;
	}

	public class Player : UnimaTweenPlayer<UnimateAnimateSlider>
	{
		private UnimaFromToPercentContext m_Context = null;
		private Slider m_Slider = null;
		private float m_StartNumber = 0;
		private float m_EndNumber = 0;
		private float m_Number = 0;

		protected override bool TryPlay(IUnimaContext context)
		{
			m_Context = context as UnimaFromToPercentContext;
			if (m_Context == null)
			{
				return false;
			}
			if (!GameObject.TryGetComponent(out m_Slider))
			{
				return false;
			}
			m_StartNumber = m_Context.FromValue;
			m_EndNumber = m_Context.ToValue;
			m_Number = m_StartNumber;
			m_Slider.value = m_StartNumber;
			if (m_Context.Duration > 0f)
			{
				Animation.SetDuration(m_Context.Duration);
			}
			return true;
		}
		

		protected override void OnUpdateTween(float normalizedTime, float loopingTime)
		{
			float number = Mathf.Lerp(m_StartNumber, m_EndNumber, normalizedTime);
			if (number != m_Number)
			{
				m_Number = number;
				m_Slider.value = number;
			}
		}

		protected override void OnEndTween(bool interrupted)
		{
			m_Slider.value = m_EndNumber; // Make sure value gets set
		}
	}
}
