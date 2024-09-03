using ODev;
using ODev.Util;
using UnityEngine;

public class SpringRope : UpdateableMonoBehaviour
{
	[Space, SerializeField]
	private Transform m_Target = null;
	[SerializeField]
	private Transform m_Source = null;

	[Space, SerializeField]
	private int m_Quality;
	[SerializeField]
	private float m_Damper;
	[SerializeField]
	private float m_Strength;
	[SerializeField]
	private float m_Velocity;
	[SerializeField]
	private float m_WaveCount;
	[SerializeField]
	private float m_WaveHeight;
	[SerializeField]
	private AnimationCurve m_AffectCurve;
	[SerializeField]
	private float m_PositionDampening = 12.0f;
	[SerializeField]
	private float m_MaxLength = float.PositiveInfinity;

	private bool m_IsActive = false;
	private float m_Value = 0.0f;
	private LineRenderer m_LineRenderer;
	private Vector3 m_CurrentPosition;

	void Awake()
	{
		m_LineRenderer = GetComponent<LineRenderer>();
	}

	public void Launch()
	{
		m_Value = 0.0f;
		m_Velocity = 15.0f;
		m_CurrentPosition = m_Source.position;
		m_LineRenderer.positionCount = m_Quality + 1;
		m_IsActive = true;
	}

	public void Return()
	{
		m_LineRenderer.positionCount = 0;
		m_IsActive = false;
	}

	protected override void Tick(float pDeltaTime)
	{
		// If not grappling, don't draw rope
		if (!m_IsActive)
		{
			return;
		}

		m_Value = Func.SpringDamper(m_Value, 0.0f, ref m_Velocity, m_Strength, m_Damper, pDeltaTime);

		Vector3 difference = m_Target.position - m_Source.position;
		difference = Vector3.ClampMagnitude(difference, m_MaxLength);
		Vector3 up = Quaternion.LookRotation(difference.normalized) * Vector3.up;

		m_CurrentPosition = Vector3.Lerp(m_CurrentPosition, m_Source.position + difference, pDeltaTime * m_PositionDampening);

		float inverseQuality = 1.0f / m_Quality;
		for (int i = 0; i < m_Quality + 1; i++)
		{
			float delta = i * inverseQuality;
			Vector3 offset = m_AffectCurve.Evaluate(delta) * m_Value * m_WaveHeight * Mathf.Sin(delta * m_WaveCount * Mathf.PI) * up;

			m_LineRenderer.SetPosition(i, Vector3.Lerp(m_Source.position, m_CurrentPosition, delta) + offset);
		}
	}

	public void SetMaxLength(float pLength)
	{
		m_MaxLength = pLength;
	}
}
