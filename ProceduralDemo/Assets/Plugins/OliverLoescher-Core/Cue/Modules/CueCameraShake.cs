using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

namespace OliverLoescher.Cue
{
	[System.Serializable]
    public class CueCameraShake : CueModule
	{
		// [SerializeField]
		// private bool Is3D = true;
		
		[SerializeField, Tooltip("The intensity of the shake.")]
		private float Magnitude = 2.5f;
		[SerializeField, Tooltip("Roughness of the shake. Lower values are smoother, higher values are more jarring.")]
		private float Roughness = 4.0f;
		[SerializeField, Tooltip("How long to fade in the shake, in seconds.")]
		private float FadeInTime = 0.1f;
		[SerializeField, Tooltip("How long to fade out the shake, in seconds.")]
		private float FadeOutTime = 0.75f;
		[SerializeField, Tooltip("How much this shake influences position.")]
		private Vector3 PosInfluence = Vector3.one * 0.15f;
		[SerializeField, Tooltip("How much this shake influences rotation.")]
		private Vector3 RotInfluence = Vector3.one;
		[Header("Distance")]
		[SerializeField]
		private float InnerDistance = 0.0f;
		[SerializeField]
		private float MaxDistance = float.PositiveInfinity;

		protected override void PlayInternal(in CueContext pContext, in SOCue pParent)
		{
			float distance = (pContext.Point - MainCamera.Position).magnitude;
			float distanceScalar = 1 - Util.Func.SmoothStep(InnerDistance, MaxDistance, distance);
			// Debug.Log("DistanceScale " + distanceScalar + " | Distance " + distance);
			CameraShaker.Instance.ShakeOnce(Magnitude * distanceScalar, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);
		}
	}
}
