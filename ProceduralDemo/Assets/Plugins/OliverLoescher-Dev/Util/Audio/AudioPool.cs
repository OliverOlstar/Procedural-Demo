using ODev.Util;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ODev
{
	public class AudioPool : MonoBehaviourSingleton<AudioPool>
    {
		private static int s_NameIndex = 0;

		private readonly List<AudioSource> m_Sources = new();
		private int m_LastIndex = -1;

		[SerializeField]
		private AudioMixerGroup m_AudioMixerGroup = null;
		[Space, SerializeField]
		private Transform m_ActiveSourcesTransform;
		[SerializeField]
		private Transform m_ReleasedSourcesTransform;

        protected override void Awake()
        {
			base.Awake();
			Reset();
        }

        private void Reset()
		{
			if (m_ActiveSourcesTransform == null)
			{
				m_ActiveSourcesTransform = new GameObject("Active").transform;
				m_ActiveSourcesTransform.SetParent(transform);
			}
			if (m_ReleasedSourcesTransform == null)
			{
				m_ReleasedSourcesTransform = new GameObject("Released").transform;
				m_ReleasedSourcesTransform.SetParent(transform);
			}
		}

        public static AudioSource ClaimSource()
		{
			AudioPool pool = Instance;
			int index = pool.GetFreeSourceIndex();
			AudioSource source = pool.m_Sources[index];
			pool.m_Sources.RemoveAt(index);
			source.transform.SetParent(pool.m_ReleasedSourcesTransform);
			return source;
		}
		
		public static void ReturnSource(AudioSource pSource)
		{
			AudioPool pool = Instance;
			pool.m_Sources.Add(pSource);
			pSource.transform.SetParent(pool.m_ActiveSourcesTransform);
		}

		public AudioSource GetFreeSource()
		{
			int index = GetFreeSourceIndex();
			return m_Sources[index];
		}
		
		private int GetFreeSourceIndex()
		{
			m_LastIndex++;
			if (m_LastIndex >= m_Sources.Count)
			{
				m_LastIndex = 0;
			}

			int index = Func.IndexOf(m_Sources, m_LastIndex, (AudioSource pSource) =>
			{
				if (pSource == null)
				{
					m_Sources.Remove(pSource);
					this.LogWarning("AudioSource was destroyed, removing from pool");
					return false;
				}
				return !pSource.isPlaying;
			});
			if (index >= 0)
			{
				m_LastIndex = index;
				return index; // Found
			}

			AudioSource source = CreateNewSource(); // New
			m_Sources.Add(source);
			return m_Sources.Count - 1;
		}

		private AudioSource CreateNewSource()
		{
			GameObject gameObject = new($"Pooled AudioSource ({s_NameIndex++})");
			gameObject.transform.SetParent(m_ActiveSourcesTransform);
			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.outputAudioMixerGroup = m_AudioMixerGroup;
			return audioSource;
		}
	}
}
