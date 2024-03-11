using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverLoescher.Util;

public class PATrigger : MonoBehaviour, IPALimb
{
	[Header("Values"), SerializeField]
	private float MaxDistance = 5.0f;

	[Header("References"), SerializeField]
	private PATarget MyTarget;
	[SerializeField]
	private PATarget[] OppositeTargets;

	private Vector3 CurrentPosition
	{
		get => MyTarget.CurrentPosition;
		set => MyTarget.CurrentPosition = value;
	}

	void IPALimb.Init(PARoot pRoot) { }

	void IPALimb.Tick(float pDeltaTime)
	{
		if (MyTarget.CurrentState == PATarget.State.Idle)
		{
			UpdateIdle();
		}
    }

	float IPALimb.GetTickPriority() => (MyTarget.TargetPosition - MyTarget.CurrentPosition).sqrMagnitude;

	private void UpdateIdle()
	{
		if (MyTarget.CurrentState == PATarget.State.Stepping)
		{
			return;
		}
		foreach (PATarget leg in OppositeTargets)
		{
			if (leg != null && leg.CurrentState == PATarget.State.Stepping)
			{
				return;
			}
		}

        if (Math.DistanceXZGreaterThan(CurrentPosition, MyTarget.TargetPosition, MaxDistance))
		{
			MyTarget.TriggerMove();
		}
	}

	void IPALimb.DrawGizmos()
	{
		if (MyTarget == null)
		{
			return;
		}
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(MyTarget.TargetPosition, MaxDistance);
	}
}
