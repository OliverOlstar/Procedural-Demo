using ODev.Picker;
using ODev.Util;
using UnityEngine;

namespace ODev.PoseAnimator
{
	public class PoseAnimator : MonoBehaviour
	{
		[SerializeField] private Transform m_Root = null;
		[SerializeField, AssetNonNull] private SOPoseSkeleton m_Skeleton = null;
		[SerializeField, AssetNonNull] private SOPoseAnimatorConfig m_Config = null;

		private PoseSystem m_System;
		private int m_SystemHandle = -1;

		private void Start()
		{
			m_System = PoseSystemManager.Instance.GetOrCreatePoseSystem(m_Skeleton, m_Config);
			OnEnable();
		}

		private void OnDestroy()
		{
			OnDisable();
		}

		private void OnEnable()
		{
			if (m_System == null)
			{
				return;
			}
			m_SystemHandle = m_System.AddAnimator(m_Root);
		}

		private void OnDisable()
		{
			if (Func.IsApplicationQuitting || m_System == null)
			{
				return;
			}
			m_System.RemoveAnimator(m_SystemHandle);
			m_System = null;
			m_SystemHandle = -1;
		}

		public int GetHandle(SOPoseAnimation pAnimation)
		{
			if (!m_Config.TryGetIndex(pAnimation, out int handle))
			{
				this.DevException($"Animation {pAnimation.name} not found in this {nameof(PoseAnimator)}");
			}
			return handle;
		}

		public PoseWeight GetWeight(int pIndex)
		{
			return m_System.GetWeight(m_SystemHandle, pIndex);
		}

		public void SetWeight(int pIndex, float pProgress01, float pWeight01 = 1.0f)
		{
			m_System.SetWeight(m_SystemHandle, pIndex, new PoseWeight()
			{
				Progress01 = pProgress01,
				Weight01 = pWeight01
			});
		}

		public void ModifyWeight(int pIndex, float pProgressDelta, float pWeight01 = 1.0f)
		{
			float progress = GetWeight(pIndex).Progress01 + pProgressDelta;
			SetWeight(pIndex, progress, pWeight01);
		}

		public int PlayMontage(SOPoseMontage pMontage) => m_System.PlayMontage(m_SystemHandle, pMontage);
		public void CancelMontage(int pIndex) => m_System.CancelMontage(m_SystemHandle, pIndex);
	}
}
