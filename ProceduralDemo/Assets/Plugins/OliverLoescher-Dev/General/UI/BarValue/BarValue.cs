using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using ODev.Util;

public class BarValue : MonoBehaviour
{
	public enum FadeOutType
	{
		Null,
		OnlyWhenMax,
		Always
	}

	[HideInInspector]
	public GameObject Root => transform.parent.parent.gameObject; // Override .gameObject to ensure they are referencing the root object

	[SerializeField]
	private Image m_TopBar = null;
	[SerializeField]
	private Image m_BottemBar = null;
	[SerializeField, ShowIf("@doFadeIn")] private Image backgroudBar = null;
	private Coroutine m_MoveRoutine = null;

	[Header("Colors")]
	[Tooltip("Leave null if color changing is not desired")]
	[SerializeField]
	private Image m_ColouringImage = null;
	[SerializeField]
	private Image m_SecondColouringImage = null;
	[HideIf("@coloringImage == null"), SerializeField]
	private Color m_DefaultColour = new(1.0f, 0.0f, 0.0f, 1.0f);
	[HideIf("@secondColoringImage == null"), SerializeField]
	private Color m_SecondDefaultColour = new(0.0f, 0.0f, 1.0f, 1.0f);
	[HideIf("@coloringImage == null || doHealColor == false"), SerializeField]
	private Color m_HealColour = new(0.0f, 1.0f, 0.0f, 1.0f);
	[HideIf("@coloringImage == null"), SerializeField]
	private Color m_ToggledColour = new(0.0f, 0.0f, 1.0f, 1.0f);
	[HideIf("@secondColoringImage == null"), SerializeField]
	private Color m_SecondToggledColour = new(0.0f, 0.0f, 1.0f, 1.0f);
	[HideIf("@coloringImage == null"), SerializeField]
	private Color m_InactiveColour = new(0.25f, 0.25f, 0.25f, 1.0f);
	[HideIf("@secondColoringImage == null"), SerializeField]
	private Color m_SecondInactiveColour = new(0.0f, 0.0f, 1.0f, 1.0f);

	[Header("Timings")]
	[SerializeField, Min(0.0f)]
	private float m_Delay = 0.75f;
	[Tooltip("Seconds for bar to fill from 0% to 100% (0% to 50% will take half the amount of seconds)")]
	[SerializeField, DisableInPlayMode, Min(Math.NEARZERO)]
	private float m_Seconds = 1.0f;
	private float m_InverseSeconds;

	[Space]
	[ShowIf("@doColorFades && coloringImage != null"), DisableInPlayMode, SerializeField, Min(Math.NEARZERO)]
	private float m_ColourSeconds = 0.1f;
	private float m_InverseColourSeconds;

	[Space]
	[SerializeField, DisableInPlayMode, ShowIf("@doFadeIn"), Min(0.0f)]
	private float m_FadeInSeconds = 1.0f;
	private float m_InverseFadeInSeconds;
	[SerializeField, Min(0.0f), ShowIf("@doFadeIn && fadeOutType != FadeOutType.Null")]
	private float m_FadeOutDelay = 2.0f;
	[SerializeField, DisableInPlayMode, ShowIf("@doFadeIn && fadeOutType != FadeOutType.Null"), Min(0.0f)]
	private float m_FadeOutSeconds = 1.0f;
	private float m_InverseFadeOutSeconds;

	[Header("Options")]
	[SerializeField]
	private bool m_UseCurve = false;
	[SerializeField, ShowIf("@useCurve")]
	private AnimationCurve m_ValueCurve = new(new Keyframe(0.0f, 0.0f, -1.0f, 1.0f), new Keyframe(1.0f, 1.0f, -1.0f, 1.0f));
	[HideIf("@coloringImage == null"), SerializeField]
	private bool m_DoHealColor = false;
	[HideIf("@coloringImage == null"), SerializeField]
	private bool m_DoColorFades = false;
	[SerializeField]
	private bool m_DoFadeIn = false;
	[SerializeField, ShowIf("@doFadeIn")]
	private FadeOutType m_FadeOutType = FadeOutType.Null;

	private bool m_IsToggled = false;

	private Coroutine m_ColourRoutine = null;
	private Coroutine m_SecondColourRoutine = null;

	private Coroutine m_FadeRoutine = null;
	private bool m_IsFadedOut = false;
	private float m_FadeOutTime = 0.0f;

	private void Awake()
	{
		m_InverseSeconds = 1 / m_Seconds;
		m_InverseColourSeconds = 1 / m_ColourSeconds;
		m_InverseFadeInSeconds = 1 / m_FadeInSeconds;
		m_InverseFadeOutSeconds = 1 / m_FadeOutSeconds;
		InitValue(1.0f);
	}

	public void InitValue(float pValue01)
	{
		if (m_UseCurve)
		{
			pValue01 = m_ValueCurve.Evaluate(pValue01);
		}
		if (m_MoveRoutine != null)
		{
			StopCoroutine(m_MoveRoutine);
		}
		m_BottemBar.fillAmount = pValue01;
		m_TopBar.fillAmount = pValue01;
		SetColor(m_DefaultColour, m_SecondDefaultColour, true);
		FadeInit();
	}

	private void Update()
	{
		UpdateFade();
	}

	private void OnEnable()
	{
		if (m_IsToggled)
		{
			SetColor(m_ToggledColour, m_SecondToggledColour);
		}
		else
		{
			SetColor(m_DefaultColour, m_SecondDefaultColour);
		}
	}

	private void OnDisable()
	{
		if (m_ColourRoutine != null)
		{
			StopCoroutine(m_ColourRoutine);
		}
		SetColor(m_InactiveColour, m_SecondInactiveColour, true);
	}

	public void SetToggled(bool pToggled)
	{
		m_IsToggled = pToggled;
		if (m_IsToggled)
		{
			SetColor(m_ToggledColour, m_SecondToggledColour);
		}
		else
		{
			SetColor(m_DefaultColour, m_SecondDefaultColour);
		}
	}

	public void SetValue(float pValue01)
	{
		if (!isActiveAndEnabled)
		{
			SetValueInstant(pValue01);
			return;
		}

		if (m_UseCurve)
		{
			pValue01 = m_ValueCurve.Evaluate(pValue01);
		}

		if (m_MoveRoutine != null)
		{
			StopCoroutine(m_MoveRoutine);
		}

		if (pValue01 > m_TopBar.fillAmount)
		{
			m_MoveRoutine = StartCoroutine(TopBarRoutine(pValue01));
			m_BottemBar.fillAmount = pValue01;

			if (m_DoHealColor)
			{
				SetColor(m_IsToggled ? m_ToggledColour : m_HealColour);
			}
		}
		else
		{
			m_MoveRoutine = StartCoroutine(BottemBarRoutine(pValue01));
			m_TopBar.fillAmount = pValue01;

			if (m_DoHealColor)
			{
				if (m_IsToggled)
				{
					SetColor(m_ToggledColour, m_SecondToggledColour);
				}
				else
				{
					SetColor(m_DefaultColour, m_SecondDefaultColour);
				}
			}
		}

		ResetFade();
	}

	public void SetValueInstant(float pValue01)
	{
		if (m_UseCurve)
		{
			pValue01 = m_ValueCurve.Evaluate(pValue01);
		}

		if (m_MoveRoutine != null)
		{
			StopCoroutine(m_MoveRoutine);
		}

		m_BottemBar.fillAmount = pValue01;
		m_TopBar.fillAmount = pValue01;
	}

	private IEnumerator TopBarRoutine(float pValue01)
	{
		yield return new WaitForSeconds(m_Delay);

		while (m_TopBar.fillAmount < pValue01)
		{
			m_TopBar.fillAmount = Mathf.Min(pValue01, m_TopBar.fillAmount + (Time.deltaTime * m_InverseSeconds));
			yield return null;
		}
	}

	private IEnumerator BottemBarRoutine(float pValue01)
	{
		yield return new WaitForSeconds(m_Delay);

		while (m_BottemBar.fillAmount > pValue01)
		{
			m_BottemBar.fillAmount = Mathf.Max(pValue01, m_BottemBar.fillAmount - (Time.deltaTime * m_InverseSeconds));
			yield return null;
		}
	}

	#region Color
	public void SetColor(Color pColor, bool pInstant = false)
	{
		SetColor(m_ColouringImage, pColor, ref m_ColourRoutine, pInstant);
	}

	public void SetColor(Color pColor, Color pSecondColor, bool pInstant = false)
	{
		SetColor(m_ColouringImage, pColor, ref m_ColourRoutine, pInstant);
		SetColor(m_SecondColouringImage, pSecondColor, ref m_SecondColourRoutine, pInstant);
	}

	private void SetColor(Image pImage, Color pColor, ref Coroutine pRoutine, bool pInstant = false)
	{
		if (pImage == null)
		{
			return;
		}
		if (!m_DoColorFades || pInstant || !isActiveAndEnabled)
		{
			SetImageColor(pImage, pColor);
			return;
		}

		if (pRoutine != null)
		{
			StopCoroutine(pRoutine);
		}
		pRoutine = StartCoroutine(SetColorRoutine(pImage, pColor));
	}

	private IEnumerator SetColorRoutine(Image pImage, Color pColor)
	{
		Color startColor = pImage.color;
		if (startColor == pColor)
		{
			yield break;
		}

		float progress01 = 0;
		while (progress01 < 1)
		{
			progress01 += Time.deltaTime * m_InverseColourSeconds;
			SetImageColor(pImage, Color.Lerp(startColor, pColor, progress01));
			yield return null;
		}
	}

	// This is called any time the color of the image is to be set
	private void SetImageColor(Image pImage, Color pColor)
	{
		pColor.a = pImage.color.a;
		pImage.color = pColor;
	}
	#endregion

	#region Fading
	private void FadeInit()
	{
		if (!m_DoFadeIn) // If should do any fading
		{
			m_FadeOutType = FadeOutType.Null;
			return;
		}
		SetImageAlpha(m_TopBar, 0.0f);
		SetImageAlpha(m_BottemBar, 0.0f);
		SetImageAlpha(backgroudBar, 0.0f);
		m_IsFadedOut = true;
	}

	private void ResetFade()
	{
		if (!m_DoFadeIn) // If should do any fading
		{
			return;
		}

		if (m_IsFadedOut) // Fade In
		{
			if (m_FadeRoutine != null)
			{
				StopCoroutine(m_FadeRoutine);
			}
			m_FadeRoutine = StartCoroutine(FadeRoutine(false, m_InverseFadeInSeconds));
		}
		
		if (m_FadeOutType != FadeOutType.Null) // Fade Out
		{
			m_FadeOutTime = Time.time + m_FadeOutDelay;
		}
	}

	private void UpdateFade()
	{
		if (m_FadeOutType != FadeOutType.Always && (m_TopBar.fillAmount != 1 || m_FadeOutType != FadeOutType.OnlyWhenMax) || m_IsFadedOut || Time.time < m_FadeOutTime)
		{
			return;
		}
		if (m_FadeRoutine != null)
		{
			StopCoroutine(m_FadeRoutine);
		}
		m_FadeRoutine = StartCoroutine(FadeRoutine(true, m_InverseFadeOutSeconds));
	}

	private IEnumerator FadeRoutine(bool pOut, float pInverseSeconds)
	{
		m_IsFadedOut = pOut;

		// Fade
		float progress01 = 0;
		while (progress01 < 1)
		{
			progress01 += Time.deltaTime * pInverseSeconds;
			float v;

			if (pOut)
			{
				v = Mathf.Lerp(1, 0, progress01);
			}
			else
			{
				v = Mathf.Lerp(0, 1, progress01);
			}

			SetImageAlpha(m_TopBar, v);
			SetImageAlpha(m_BottemBar, v);
			SetImageAlpha(backgroudBar, v);
			yield return null;
		}
	}

	private void SetImageAlpha(Image pImage, float pAlpha)
	{
		Color c = pImage.color;
		c.a = pAlpha;
		pImage.color = c;
	}
	#endregion
}