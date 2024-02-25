using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace OliverLoescher.Util
{
	// Static Util class for general playing audio functions
	public static class Audio
	{
		[System.Serializable]
		public class AudioPiece
		{
			[SerializeField]
			private AudioClip[] Clips = new AudioClip[0];
			[SerializeField, Range(0, 1)] 
			private float Volume = 1.0f;
			[SerializeField, MinMaxSlider(0, 3, true)]
			private Vector2 Pitch = new Vector2(0.9f, 1.2f);
			[SerializeField]
			private AudioPriority.Enum Priority = AudioPriority.Enum.None;

			public void Play() => PlayOneShot(Clips, Pitch, Volume, Priority);
			public void Play(in Vector3 pPoint) => PlayOneShot(Clips, pPoint, Pitch, Volume, Priority);
		}

		// Basic
		public static void PlayOneShot(in AudioClip pClip, float pPitch = 1.0f, float pVolume01 = 1.0f, AudioPriority.Enum pPriority = AudioPriority.Enum.None)
		{
			if (pClip == null)
			{
				Debug.LogWarning("Clip cannot be null", "PlayOneShot", typeof(Audio));
				return;
			}
			AudioSource source = AudioPool.Instance.GetFreeSource();
			SetValues(source, pClip, pPitch, pVolume01, pPriority, false);
			source.Play();
		}
		public static void PlayOneShot(in AudioClip pClip, in float pPitchMin, in float pPitchMax, float pVolume01 = 1.0f, AudioPriority.Enum pPriority = AudioPriority.Enum.None)
			=> PlayOneShot(pClip, UnityEngine.Random.Range(pPitchMin, pPitchMax), pVolume01, pPriority);
		public static void PlayOneShot(in AudioClip pClip, in Vector2 pPitch, float pVolume01 = 1.0f, AudioPriority.Enum pPriority = AudioPriority.Enum.None)
			=> PlayOneShot(pClip, Random.Range(pPitch), pVolume01, pPriority);

		// Basic 3D
		public static void PlayOneShot(in AudioClip pClip, Vector3 pPoint, float pPitch = 1.0f, float pVolume01 = 1.0f, AudioPriority.Enum pPriority = AudioPriority.Enum.None)
		{
			if (pClip == null)
			{
				Debug.LogWarning("Clip cannot be null", "PlayOneShot", typeof(Audio));
				return;
			}
			AudioSource source = AudioPool.Instance.GetFreeSource();
			SetValues(source, pClip, pPitch, pVolume01, pPriority, true);
			source.transform.position = pPoint;
			source.Play();
		}
		public static void PlayOneShot(in AudioClip pClip, Vector3 pPoint, in float pPitchMin, in float pPitchMax, float pVolume01 = 1.0f, AudioPriority.Enum pPriority = AudioPriority.Enum.None)
			=> PlayOneShot(pClip, pPoint, UnityEngine.Random.Range(pPitchMin, pPitchMax), pVolume01, pPriority);
		public static void PlayOneShot(in AudioClip pClip, Vector3 pPoint, in Vector2 pPitch, float pVolume01 = 1.0f, AudioPriority.Enum pPriority = AudioPriority.Enum.None)
			=> PlayOneShot(pClip, pPoint, Random.Range(pPitch), pVolume01, pPriority);

		// Random Clip
		public static void PlayOneShot(in AudioClip[] pClips, float pPitch = 1.0f, float pVolume01 = 1.0f, AudioPriority.Enum pPriority = AudioPriority.Enum.None)
			=> PlayOneShot(GetRandomClip(pClips), pPitch, pVolume01, pPriority);
		public static void PlayOneShot(in AudioClip[] pClips, in float pPitchMin, in float pPitchMax, float pVolume01 = 1.0f, AudioPriority.Enum pPriority = AudioPriority.Enum.None)
			=> PlayOneShot(GetRandomClip(pClips), UnityEngine.Random.Range(pPitchMin, pPitchMax), pVolume01, pPriority);
		public static void PlayOneShot(in AudioClip[] pClips, in Vector2 pPitch, float pVolume01 = 1.0f, AudioPriority.Enum pPriority = AudioPriority.Enum.None)
			=> PlayOneShot(GetRandomClip(pClips), Random.Range(pPitch), pVolume01, pPriority);

		// Random Clip 3D
		public static void PlayOneShot(in AudioClip[] pClips, Vector3 pPoint, float pPitch = 1.0f, float pVolume01 = 1.0f, AudioPriority.Enum pPriority = AudioPriority.Enum.None)
			=> PlayOneShot(GetRandomClip(pClips), pPoint, pPitch, pVolume01, pPriority);
		public static void PlayOneShot(in AudioClip[] pClips, Vector3 pPoint, in float pPitchMin, in float pPitchMax, float pVolume01 = 1.0f, AudioPriority.Enum pPriority = AudioPriority.Enum.None)
			=> PlayOneShot(GetRandomClip(pClips), pPoint, UnityEngine.Random.Range(pPitchMin, pPitchMax), pVolume01, pPriority);
		public static void PlayOneShot(in AudioClip[] pClips, Vector3 pPoint, in Vector2 pPitch, float pVolume01 = 1.0f, AudioPriority.Enum pPriority = AudioPriority.Enum.None)
			=> PlayOneShot(GetRandomClip(pClips), pPoint, Random.Range(pPitch), pVolume01, pPriority);
		
		public static void SetValues(AudioSource pSource, AudioClip pClip, float pPitch = 1.0f, float pVolume01 = 1.0f, AudioPriority.Enum pPriority = AudioPriority.Enum.None, bool pIs3D = true, bool pLoop = false)
		{
			pSource.clip = pClip;
			pSource.volume = pVolume01;
			pSource.pitch = pPitch;
			pSource.priority = pPriority.ToInt();
			pSource.loop = pLoop;
			pSource.spatialBlend = pIs3D ? 1.0f : 0.0f;
		}
		
		public static AudioClip GetRandomClip(in AudioClip[] pClips)
		{
			if (pClips.Length == 0)
			{
				return null;
			}
			return pClips[UnityEngine.Random.Range(0, pClips.Length)];
		}
	}
}