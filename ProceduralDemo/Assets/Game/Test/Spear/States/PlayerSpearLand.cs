using ODev;
using UnityEngine;

[System.Serializable]
public class PlayerSpearLand : PlayerSpearController
{
	private readonly TransformFollower m_Follower = new();

	public override PlayerSpear.State State => PlayerSpear.State.Landed;

	public void Start(Transform pAttachTo)
	{
		m_Follower.Start(pAttachTo, Transform, Transform.position, true, ODev.Util.Mono.Type.Fixed, ODev.Util.Mono.Priorities.World, Spear);
	}

	public override void Stop()
	{
		m_Follower.Stop();
	}
}
