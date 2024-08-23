using System.Collections;
using System.Collections.Generic;
using ODev.Util;
using UnityEngine;

[System.Serializable]
public class CenterOfMassBounce : PoseAnimatorControllerBase
{
	[SerializeField]
	private Transform m_CenterOfMass = null;

	[Space, SerializeField]
	private bool m_Enabled = true;
	[SerializeField]
	private float m_BounceHeight = 0.25f;

	private readonly List<(int animationHandle, float height)> m_Heights = new();
	private float m_Height = 0.0f;

	public void AddBounce(int pAnimationHandle, float pRadius)
	{
		m_Heights.Add((pAnimationHandle, pRadius));
	}

	protected override void Setup() { }
	public override void Destroy() { }

	public override void Tick(float pDeltaTime)
	{
		if (!m_Enabled)
		{
			return;
		}

		CalculateHeight();

		float x = (Controller.Wheel.Angle % 90.0f) / 90.0f;
		float y = 0.0f;
		if (!Root.Movement.VelocityXZ.sqrMagnitude.IsNearZero())
		{
			y = Mathf.Abs(Mathf.Sin(x * Mathf.PI)) * m_BounceHeight * m_Height;
			// ODev.Util.Debug.Log($"Mag {magnitude}, height {m_BounceHeight * m_Height}", typeof(PoseAnimatorLocomotion));
		}
		m_CenterOfMass.transform.localPosition = new Vector3(0.0f, y, 0.0f);
	}

	private void CalculateHeight()
	{
		if (!Root.OnGround.IsOnGround)
		{
			m_Height = 0.0f;
			return;
		}
		float weight;
		m_Height = 0.0f;
		for (int i = 0; i < m_Heights.Count; i++)
		{
			weight = Animator.GetWeight(m_Heights[i].animationHandle).Weight01.Clamp01();
			if (weight >= 0.0f)
			{
				m_Height = Mathf.Lerp(m_Height, m_Heights[i].height, weight);
			}
		}
	}
}
