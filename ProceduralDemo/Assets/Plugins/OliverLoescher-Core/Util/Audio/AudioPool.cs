using OliverLoescher.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher
{
    public class AudioPool : MonoBehaviourSingleton<AudioPool>
    {
		private List<AudioSource> Sources = new List<AudioSource>();

		private static int NameIndex = -1;
		private int LastIndex = -1;

		private Transform ActiveSourcesTransform;
		private Transform ReleasedSourcesTransform;

		private void Start()
		{
			ActiveSourcesTransform = new GameObject("Active").transform;
			ActiveSourcesTransform.SetParent(transform);
			ReleasedSourcesTransform = new GameObject("Released").transform;
			ReleasedSourcesTransform.SetParent(transform);
		}

		public static AudioSource ClaimSource()
		{
			AudioPool pool = Instance;
			int index = pool.GetFreeSourceIndex();
			AudioSource source = pool.Sources[index];
			pool.Sources.RemoveAt(index);
			source.transform.SetParent(pool.ReleasedSourcesTransform);
			return source;
		}
		
		public static void ReturnSource(AudioSource pSource)
		{
			AudioPool pool = Instance;
			pool.Sources.Add(pSource);
			pSource.transform.SetParent(pool.ActiveSourcesTransform);
		}

		public AudioSource GetFreeSource()
		{
			int index = GetFreeSourceIndex();
			return Sources[index];
		}
		
		private int GetFreeSourceIndex()
		{
			LastIndex++;
			if (LastIndex >= Sources.Count)
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
