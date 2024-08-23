using System.Collections;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using ODev;

public class DamageNumbers : MonoBehaviourSingleton<DamageNumbers>
{
	[SerializeField, DisableInPlayMode]
	private GameObject m_TextPrefab = null;
	[SerializeField, DisableInPlayMode]
	private int m_TextCount = 12;
	private TMP_Text[] m_Texts = new TMP_Text[0];
	private Coroutine[] m_TextRoutines = new Coroutine[0];
	private int m_Index = 0;

	[Space]
	[SerializeField]
	private AnimationCurve m_PositionCurve;
	[SerializeField]
	private AnimationCurve m_AlphaCurve;
	[SerializeField]
	private float m_Seconds = 0.5f;
	[SerializeField]
	private float m_RandomOffset = 0.2f;

	private void Start()
	{
		m_Texts = new TMP_Text[m_TextCount];
		m_TextRoutines = new Coroutine[m_TextCount];
		for (int i = 0; i < m_TextCount; i++)
		{
			m_Texts[i] = Instantiate(m_TextPrefab, transform).GetComponent<TMP_Text>();
			m_Texts[i].gameObject.SetActive(false);
		}
	}

	public void DisplayNumber(string pText, Vector3 pPosition, Color pColor)
	{
		m_Index++;
		if (m_Index == m_TextCount)
		{
			m_Index = 0;
		}

		m_Texts[m_Index].gameObject.SetActive(true);
		m_Texts[m_Index].text = pText;
		//textsMat[index].SetColor("_EmissionColor", pColor);

		Vector2 offset = 0.5f * m_RandomOffset * Random.insideUnitCircle;
		pPosition += new Vector3(offset.x, 0, offset.y);

		if (m_TextRoutines[m_Index] != null)
		{
			StopCoroutine(m_TextRoutines[m_Index]);
		}

		m_TextRoutines[m_Index] = StartCoroutine(TextRoutine(m_Texts[m_Index], pPosition, pColor));
	}

	private IEnumerator TextRoutine(TMP_Text pText, Vector3 pPosition, Color pColor)
	{
		float progress = 0;
		while (progress < 2)
		{
			pColor.a = m_AlphaCurve.Evaluate(progress);
			pText.color = pColor;

			Vector3 curPosition = pPosition + new Vector3(0, m_PositionCurve.Evaluate(progress), 0);
			pText.transform.position = curPosition;

			progress += Time.deltaTime / m_Seconds;
			yield return new WaitForEndOfFrame();
		}
		pText.gameObject.SetActive(false);
	}
}
