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

		public override void Play(CueContext pContext)
		{
			// TODO Scale by distance
			CameraShaker.Instance.ShakeOnce(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);
		}
	}
}
