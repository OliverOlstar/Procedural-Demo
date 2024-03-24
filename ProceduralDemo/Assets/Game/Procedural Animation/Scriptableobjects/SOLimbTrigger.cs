using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverLoescher.Util;

namespace PA
{
	[CreateAssetMenu(fileName = "New PA Limb Trigger", menuName = "Procedural Animation/Limb/Trigger")]
	public class SOLimbTrigger : ScriptableObject
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

		public void Init() { }

		public void Tick(float pDeltaTime)
		{
			if (MyTarget.CurrentState == PATarget.State.Idle)
			{
				UpdateIdle();
			}
		}

		public float GetTickPriority() => (MyTarget.TargetPosition - MyTarget.CurrentPosition).sqrMagnitude;

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

		public void DrawGizmos()
		{
			if (MyTarget == null)
			{
				return;
			}
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(MyTarget.TargetPosition, MaxDistance);
		}
	}
}
