using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unimate/Core/Rotate")]
public class UnimateRotation : UnimateTween<UnimateRotation, UnimateRotation.Player>
{
	[SerializeField]
	private AnimationCurve m_RotateCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
	[SerializeField]
	private Vector3 m_Axis = Vector3.forward;
	[SerializeField]
	private float m_Duration = 1.0f;
	public override float Duration => m_Duration;
	[SerializeField]
	private bool m_Loop = false;
	public override bool Loop => m_Loop;
	[SerializeField]
	private bool m_UpdateBeforeStart = false;
	public override bool UpdateBeforeStart => m_UpdateBeforeStart;

	protected override string OnEditorValidate(GameObject gameObject)
	{
		float lastKey = m_RotateCurve.keys[m_RotateCurve.length - 1].time;
		if (!Core.Util.Approximately(lastKey, 1.0f))
		{
			return "Curve keys should be in the range 0 to 1";
		}
		return null;
	}

	public class Player : UnimaTweenPlayer<UnimateRotation>
	{
		Quaternion m_InitialRotation = Quaternion.identity;

		protected override void OnStartTween()
		{
			m_InitialRotation = Transform.localRotation;
		}

		protected override void OnUpdateTween(float normalizedTime, float loopingTime)
		{
			float angle = 360.0f * Animation.m_RotateCurve.Evaluate(normalizedTime);
			Transform.localRotation = Quaternion.AngleAxis(angle, Animation.m_Axis) * m_InitialRotation;
		}
	}
}
