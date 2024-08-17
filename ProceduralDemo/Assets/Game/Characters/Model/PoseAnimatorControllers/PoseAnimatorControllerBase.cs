using System.Collections;
using System.Collections.Generic;
using ODev.PoseAnimator;
using UnityEngine;

[System.Serializable]
public abstract class PoseAnimatorControllerBase
{
	private PlayerRoot m_Root = null;
	protected PlayerRoot Root => m_Root;

	private PoseAnimator m_Animator = null;
	protected PoseAnimator Animator => m_Animator;

	private PoseAnimatorController m_Controller = null;
	protected PoseAnimatorController Controller => m_Controller;

	protected abstract void Setup();
	public abstract void Destroy();
	public abstract void Tick(float pDeltaTime);

	public void Setup(PlayerRoot pRoot, PoseAnimator pAnimator, PoseAnimatorController pController)
	{
		m_Root = pRoot;
		m_Animator = pAnimator;
		m_Controller = pController;
		Setup();
	}
}
