using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public struct TrackPlayer
	{
		private IReadOnlyList<Track> m_Tracks;
		public int TrackCount => m_Tracks.Count;
		private ITreeEvent m_TreeEvent;

		private int m_TrackCount; // Micro optimization to cache m_Tracks.Count, but also saves null check m_Tracks before looping

		public void StateEnter(TrackList tracks, float timeScale, ITreeEvent treeEvent)
		{
			m_Tracks = tracks.List;
			m_TrackCount = m_Tracks.Count;
			m_TreeEvent = treeEvent;
			for (int i = 0; i < m_TrackCount; i++)
			{
				m_Tracks[i].StateEnter(m_TreeEvent, timeScale);
			}
		}

		public void UpdateAll(float timer, float scaledDeltaTime, float timeScale)
		{
			for (int i = 0; i < m_TrackCount; i++)
			{
				Track track = m_Tracks[i];
				track.StateUpdate(m_TreeEvent, timer, scaledDeltaTime, timeScale);
			}
		}

		public void UpdateIndividual(int index, float timer, float scaledDeltaTime, float timeScale)
		{
			Track track = m_Tracks[index];
			track.StateUpdate(m_TreeEvent, timer, scaledDeltaTime, timeScale);
		}

		public bool KeepAlive()
		{
			for (int i = 0; i < m_TrackCount; i++)
			{
				Track track = m_Tracks[i];
				if (track.KeepSequencerAlive())
				{
					return true;
				}
			}
			return false;
		}

		public void StateExit(bool interrupted)
		{
			for (int i = 0; i < m_TrackCount; i++)
			{
				m_Tracks[i].StateExit(m_TreeEvent, interrupted);
			}
			m_TrackCount = 0;
			m_Tracks = null;
		}
	}
}
