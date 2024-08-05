using System.Collections;
using System.Collections.Generic;
using ODev;
using ODev.Picker;
using ODev.PoseAnimator;
using UnityEngine;

public class PoseAnimatorController : UpdateableMonoBehaviour
{
	[SerializeField]
	private PoseAnimator m_Animator = null;
	[SerializeField, Asset]
	private SOPoseAnimation m_Animation = null;

	private void Start()
	{
		m_Animator.Add(m_Animation);
	}

	protected override void Tick(float pDeltaTime)
	{
		throw new System.NotImplementedException();
	}
}
