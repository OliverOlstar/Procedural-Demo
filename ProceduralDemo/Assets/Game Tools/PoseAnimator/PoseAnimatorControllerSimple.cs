using System.Collections;
using System.Collections.Generic;
using ODev.Picker;
using UnityEngine;

namespace ODev.PoseAnimator
{
    public class PoseAnimatorControllerSimple : UpdateableMonoBehaviour
	{
		[SerializeField]
		private PoseAnimator m_Animator = null;
		[SerializeField, AssetNonNull]
		private SOPoseAnimation m_Animation = null;

		[Space, SerializeField, Range(0.0f, 1.0f)]
		private float m_Progress01 = 0.0f;
		[SerializeField]
		private float m_Progress = 0.0f;

		private int m_Handle = -1;

		private float Progress => m_Progress + m_Progress01;

		private void Start()
		{
			m_Handle = m_Animator.Add(m_Animation);
		}

		protected override void Tick(float pDeltaTime)
		{
			m_Animator.SetWeight(m_Handle, Progress);
		}
	}
}
