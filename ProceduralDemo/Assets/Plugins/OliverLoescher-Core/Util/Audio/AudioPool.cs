using OliverLoescher.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher
{
    public class AudioPool : MonoBehaviourSingleton<AudioPool>
    {
		private List<AudioSource> Sources;

		private static int NameIndex = 0;
		private int LastIndex = 0;

		private Transform ActiveSourcesTransform;
		private Transform ReleasedSourcesTransform;

		private void Start()
		{
			ActiveSourcesTransform = new GameObject("Active").transform;
			ActiveSourcesTransform.SetParent(transform);
			ReleasedSourcesTransform = new GameObject("Released").transform;
			ReleasedSourcesTransform.SetParent(transform);
		}

		// Basic
		public static void PlayOneShot(in AudioClip pClip, float pPitch = 1.0f, float pVolume01 = 1.0f, int pPriority = 128)
		{
			if (pClip == null)
			{
				LogWarning("Clip cannot be null", "PlayOneShot");
				return;
			}
			AudioSource source = Instance.GetFreeSource();
			SetValues(source, pClip, pPitch, pVolume01, pPriority, false);
			source.Play();
		}
		public static void PlayOneShot(in AudioClip pClip, in float pPitchMin, in float pPitchMax, float pVolume01 = 1.0f, int pPriority = 128)
			=> PlayOneShot(pClip, UnityEngine.Random.Range(pPitchMin, pPitchMax), pVolume01, pPriority);
		public static void PlayOneShot(in AudioClip pClip, in Vector2 pPitch, float pVolume01 = 1.0f, int pPriority = 128)
			=> PlayOneShot(pClip, Util.Random.Range(pPitch), pVolume01, pPriority);

		// Basic 3D
		public static void PlayOneShot(in AudioClip pClip, Vector3 pPoint, float pPitch = 1.0f, float pVolume01 = 1.0f, int pPriority = 128)
		{
			if (pClip == null)
			{
				LogWarning("Clip cannot be null", "PlayOneShot");
				return;
			}
			AudioSource source = Instance.GetFreeSource();
			SetValues(source, pClip, pPitch, pVolume01, pPriority, true);
			source.transform.position = pPoint;
			source.Play();
		}
		public static void PlayOneShot(in AudioClip pClip, Vector3 pPoint, in float pPitchMin, in float pPitchMax, float pVolume01 = 1.0f, int pPriority = 128)
			=> PlayOneShot(pClip, pPoint, UnityEngine.Random.Range(pPitchMin, pPitchMax), pVolume01, pPriority);
		public static void PlayOneShot(in AudioClip pClip, Vector3 pPoint, in Vector2 pPitch, float pVolume01 = 1.0f, int pPriority = 128)
			=> PlayOneShot(pClip, pPoint, Util.Random.Range(pPitch), pVolume01, pPriority);

		// Random Clip
		public static void PlayOneShot(in AudioClip[] pClips, float pPitch = 1.0f, float pVolume01 = 1.0f, int pPriority = 128)
			=> PlayOneShot(GetRandomClip(pClips), pPitch, pVolume01, pPriority);
		public static void PlayOneShot(in AudioClip[] pClips, in float pPitchMin, in float pPitchMax, float pVolume01 = 1.0f, int pPriority = 128)
			=> PlayOneShot(GetRandomClip(pClips), UnityEngine.Random.Range(pPitchMin, pPitchMax), pVolume01, pPriority);
		public static void PlayOneShot(in AudioClip[] pClips, in Vector2 pPitch, float pVolume01 = 1.0f, int pPriority = 128)
			=> PlayOneShot(GetRandomClip(pClips), Util.Random.Range(pPitch), pVolume01, pPriority);

		// Random Clip 3D
		public static void PlayOneShot(in AudioClip[] pClips, Vector3 pPoint, float pPitch = 1.0f, float pVolume01 = 1.0f, int pPriority = 128)
			=> PlayOneShot(GetRandomClip(pClips), pPoint, pPitch, pVolume01, pPriority);
		public static void PlayOneShot(in AudioClip[] pClips, Vector3 pPoint, in float pPitchMin, in float pPitchMax, float pVolume01 = 1.0f, int pPriority = 128)
			=> PlayOneShot(GetRandomClip(pClips), pPoint, UnityEngine.Random.Range(pPitchMin, pPitchMax), pVolume01, pPriority);
		public static void PlayOneShot(in AudioClip[] pClips, Vector3 pPoint, in Vector2 pPitch, float pVolume01 = 1.0f, int pPriority = 128)
			=> PlayOneShot(GetRandomClip(pClips), pPoint, Util.Random.Range(pPitch), pVolume01, pPriority);

		public AudioSource ClaimSource()
		{
			int index = GetFreeSourceIndex();
			AudioSource source = Sources[index];
			Sources.RemoveAt(index);
			source.transform.SetParent(ReleasedSourcesTransform);
			return source;
		}
		public void ReturnSource(AudioSource pSource)
		{
			Sources.Add(pSource);
			pSource.loop = false;
			pSource.transform.SetParent(ActiveSourcesTransform);
		}

		public static void SetValues(AudioSource pSource, AudioClip pClip, float pPitch = 1.0f, float pVolume01 = 1.0f, int pPriority = 128, bool pIs3D = true, bool pLoop = false)
		{
			pSource.clip = pClip;
			pSource.volume = pVolume01;
			pSource.pitch = pPitch;
			pSource.priority = pPriority;
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

		private AudioSource GetFreeSource()
		{
			int index = GetFreeSourceIndex();
			return Sources[index];
		}
		private int GetFreeSourceIndex()
		{
			LastIndex++;
			if (LastIndex == Sources.Count)
			{
				LastIndex = 0;
			}

			int index = Func.IndexOf(Sources, LastIndex, (AudioSource pSource) => !pSource.isPlaying);
			if (index >= 0)
			{
				LastIndex = index;
				return index; // Found
			}

			AudioSource source = CreateNewSource(); // New
			Sources.Add(source);
			return Sources.Count - 1;
		}

		private AudioSource CreateNewSource()
		{
			GameObject gameObject = new GameObject($"Pooled AudioSource ({NameIndex++})");
			gameObject.transform.SetParent(ActiveSourcesTransform);
			AudioSource audioSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
			return audioSource;
		}
	}
}
