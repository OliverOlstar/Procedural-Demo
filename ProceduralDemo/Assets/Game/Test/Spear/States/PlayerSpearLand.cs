using ODev;
using UnityEngine;

[System.Serializable]
public class PlayerSpearLand : PlayerSpearController
{
	private readonly TransformFollower m_Follower = new();

	internal override PlayerSpear.State State => PlayerSpear.State.Landed;

	internal void Start(Transform pAttachTo, Vector3 pHitPoint)
	{
		m_Follower.Start(pAttachTo, Transform, pHitPoint, true, ODev.Util.Mono.Type.Fixed, ODev.Util.Mono.Priorities.World, Spear);
	}

	internal override void Stop()
	{
		m_Follower.Stop();
	}
}
