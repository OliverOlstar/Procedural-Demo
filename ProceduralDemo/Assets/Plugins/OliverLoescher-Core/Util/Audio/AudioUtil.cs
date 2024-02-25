using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace OliverLoescher.Util
{
	// Static Util class for general playing audio functions
	public static class Audio
	{
		public static AudioClip GetRandomClip(in AudioClip[] pClips)
		{
			if (pClips.Length == 0)
			{
				return null;
			}

			return pClips[UnityEngine.Random.Range(0, pClips.Length)];
		}

		public static void PlayOneShotRandomClip(in AudioSource pSource, in AudioClip[] pClips)
		{
			if (pSource == null)
			{
				LogWarning("PlayOneShotRandomClip() was given a null AudioSource");
				return;
			}

			AudioClip clip = GetRandomClip(pClips);
			if (clip == null)
			{
				LogWarning("GetRandomClip() returned a null reference - " + pSource.gameObject.name);
				return;
			}

			pSource.clip = clip;
			pSource.Play();
		}

		public static void PlayOneShotRandomClip(in AudioSource pSource, in AudioClip[] pClips, in float pPitchMin, in float pPitchMax)
		{
			pSource.pitch = UnityEngine.Random.Range(pPitchMin, pPitchMax);
			PlayOneShotRandomClip(pSource, pClips);
		}

		public static void PlayOneShotRandomClip(in AudioSource pSource, in AudioClip[] pClips, in Vector2 pitch)
		{
			pSource.pitch = Random.Range(pitch);
			PlayOneShotRandomClip(pSource, pClips);
		}

		public static void PlayOneShotRandomClip(in AudioSource pSource, in AudioClip[] pClips, in float pPitchMin, in float pPitchMax, in float pVolume)
		{
			pSource.volume = pVolume;
			pSource.pitch = UnityEngine.Random.Range(pPitchMin, pPitchMax);
			PlayOneShotRandomClip(pSource, pClips);
		}

		public static void PlayOneShotRandomClip(in AudioSource pSource, in AudioClip[] pClips, in Vector2 pPitch, in float pVolume)
		{
			pSource.volume = pVolume;
			pSource.pitch = Random.Range(pPitch);
			PlayOneShotRandomClip(pSource, pClips);
		}
		

		[System.Serializable]
		public class AudioPiece
		{
			public AudioClip[] Clips = new AudioClip[0];
			[Range(0, 1)] 
			public float Volume = 1.0f;
			[MinMaxSlider(0, 3, true)]
			public Vector2 Pitch = new Vector2(0.9f, 1.2f);

			public void Play(in AudioSource pSource) => PlayOneShotRandomClip(pSource, Clips, Pitch, Volume);
			public void Play(in Vector3 pPoint)
			{
				throw new System.NotImplementedException();
				//AudioSource.PlayClipAtPoint(Clips[0]);
				//PlayOneShotRandomClip(source.GetSource(), Clips, Pitch, Volume);
			}
		}

		#region Helpers
		private static void Log(string message) => UnityEngine.Debug.Log($"[AudioUtil] {message}");
		private static void LogWarning(string message) => UnityEngine.Debug.LogWarning($"[AudioUtil] {message}");
		private static void LogError(string message) => UnityEngine.Debug.LogError($"[AudioUtil] {message}");
		#endregion Helpers
	}
}