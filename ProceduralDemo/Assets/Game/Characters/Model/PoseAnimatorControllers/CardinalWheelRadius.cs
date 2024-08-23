using System.Collections;
using System.Collections.Generic;
using ODev.Util;
using UnityEngine;

[System.Serializable]
public class CardinalWheelRadius : PoseAnimatorControllerBase
{
	[SerializeField]
	private bool m_Enabled = true;

	private readonly List<(int animationHandle, float radius)> m_Radei = new();

	public void AddWheelRadius(int pAnimationHandle, float pRadius)
	{
		m_Radei.Add((pAnimationHandle, pRadius));
	}

	protected override void Setup() { }
	public override void Destroy() { }

	public override void Tick(float pDeltaTime)
	{
		if (!m_Enabled)
		{
			return;
		}

		float radius = 0.0f;
		float weight;
		for (int i = 0; i < m_Radei.Count; i++)
		{
			if (i == 0)
			{
				radius = m_Radei[i].radius;
				continue;
			}
			
			weight = Animator.GetWeight(m_Radei[i].animationHandle).Weight01.Clamp01();
			if (weight >= 0.0f)
			{
				radius = Mathf.Lerp(radius, m_Radei[i].radius, weight);
			}
		}
		Controller.Wheel.SetRadius(radius);
	}
}
