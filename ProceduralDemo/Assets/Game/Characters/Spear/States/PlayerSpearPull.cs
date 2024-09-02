using ODev.Util;
using UnityEngine;

[System.Serializable]
public class PlayerSpearPull : PlayerSpearController
{
	[SerializeField]
	private float m_Seconds = 0.5f;
	[SerializeField]
	private Easing.EaseParams m_Easing = new();

	private Anim.IAnimation m_AnimHandle = null;

	internal override PlayerSpear.State State => PlayerSpear.State.Pulling;

	internal void Start(Transform pToTarget)
	{
		Vector3 startPosition = Transform.position;

		m_AnimHandle = Anim.Play(m_Easing, m_Seconds, Anim.Type.Physics, 
		progress01 => 
		{
			progress01 = Easing.Ease(m_Easing, progress01);
			Transform.position = Vector3.LerpUnclamped(startPosition, pToTarget.position, progress01);
		},
		progress01 =>
		{
			Spear.Store();
		});
	}

	internal override void Stop()
	{
		m_AnimHandle.Cancel();
		m_AnimHandle = null;
	}
}
