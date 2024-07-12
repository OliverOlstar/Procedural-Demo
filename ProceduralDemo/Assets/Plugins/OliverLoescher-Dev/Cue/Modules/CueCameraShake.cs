using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

namespace ODev.Cue
{
	[System.Serializable]
    public class CueCameraShake : CueModule
	{
		// [SerializeField]
		// private bool Is3D = true;
		
		[SerializeField, Tooltip("The intensity of the shake.")]
		private float m_Magnitude = 2.5f;
		[SerializeField, Tooltip("Roughness of the shake. Lower values are smoother, higher values are more jarring.")]
		private float m_Roughness = 4.0f;
		[SerializeField, Tooltip("How long to fade in the shake, in seconds.")]
		private float m_FadeInTime = 0.1f;
		[SerializeField, Tooltip("How long to fade out the shake, in seconds.")]
		private float m_FadeOutTime = 0.75f;
		[SerializeField, Tooltip("How much this shake influences position.")]
		private Vector3 m_PosInfluence = Vector3.one * 0.15f;
		[SerializeField, Tooltip("How much this shake influences rotation.")]
		private Vector3 m_RotInfluence = Vector3.one;

		[Header("Distance")]
		[SerializeField]
		private float m_InnerDistance = 0.0f;
		[SerializeField]
		private float m_MaxDistance = float.PositiveInfinity;

		protected override void PlayInternal(in CueContext pContext, in SOCue pParent)
		{
			float distance = (pContext.Point - MainCamera.Position).magnitude;
			float distanceScalar = 1 - Util.Func.SmoothStep(m_InnerDistance, m_MaxDistance, distance);
			// Debug.Log("DistanceScale " + distanceScalar + " | Distance " + distance);
			CameraShaker.Instance.ShakeOnce(m_Magnitude * distanceScalar, m_Roughness, m_FadeInTime, m_FadeOutTime, m_PosInfluence, m_RotInfluence);
		}
	}
}
