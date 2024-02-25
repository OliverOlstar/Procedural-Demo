using OliverLoescher.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher
{
    public class AudioSourcePool : MonoBehaviourSingleton<AudioSourcePool>
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

		private int GetFreeSourceIndex()
		{
			LastIndex++;
			if (LastIndex == Sources.Count)
			{
				LastIndex = 0;
			}

			// Find
			int index = Func.IndexOf(Sources, LastIndex, (AudioSource pSource) => !pSource.isPlaying);
			if (index >= 0)
			{
				LastIndex = index;
				return index;
			}

			// New
			AudioSource source = CreateNewSource();
			Sources.Add(source);
			return Sources.Count - 1;
		}

		private AudioSource CreateNewSource()
		{
			GameObject gameObject = new GameObject($"Pooled AudioSource ({NameIndex++})");
			gameObject.transform.SetParent(transform);
			AudioSource audioSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
			return audioSource;
		}


		public static void PlayOneShotRandomClip(in AudioSource pSource, in AudioClip[] pClips)
		{
			if (pSource == null)
			{
				LogWarning("PlayOneShotRandomClip() was given a null AudioSource", "PlayOneShotRandomClip");
				return;
			}

			AudioClip clip = Audio.GetRandomClip(pClips);
			if (clip == null)
			{
				LogWarning("GetRandomClip() returned a null reference - " + pSource.gameObject.name, "PlayOneShotRandomClip");
				return;
			}

			pSource.clip = clip;
			pSource.Play();
		}

		public static void PlayOneShotRandomClip(in AudioClip[] pClips, in float pPitchMin, in float pPitchMax)
		{
			pSource.pitch = UnityEngine.Random.Range(pPitchMin, pPitchMax);
			PlayOneShotRandomClip(pSource, pClips);
		}

		public static void PlayOneShotRandomClip(in AudioClip[] pClips, in Vector2 pitch)
		{
			pSource.pitch = Util.Random.Range(pitch);
			PlayOneShotRandomClip(pSource, pClips);
		}

		public static void PlayOneShotRandomClip(in AudioClip[] pClips, in float pPitchMin, in float pPitchMax, in float pVolume)
		{
			pSource.volume = pVolume;
			pSource.pitch = UnityEngine.Random.Range(pPitchMin, pPitchMax);
			PlayOneShotRandomClip(pSource, pClips);
		}

		public static void PlayOneShotRandomClip(in AudioClip[] pClips, in Vector2 pPitch, in float pVolume)
		{
			pSource.volume = pVolume;
			pSource.pitch = Util.Random.Range(pPitch);
			PlayOneShotRandomClip(pSource, pClips);
		}
	}
}
