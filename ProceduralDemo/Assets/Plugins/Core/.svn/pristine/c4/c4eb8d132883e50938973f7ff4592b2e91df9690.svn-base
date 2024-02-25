using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public class TrackList
	{
		private List<Track> m_Tracks = new List<Track>();
		public IReadOnlyList<Track> List => m_Tracks;

		private bool m_HasMajorTrack = false;
		public bool HasMajorTrack => m_HasMajorTrack;

		public TrackList(IActNode node)
		{
			foreach (Track track in node.Tracks)
			{
				if (track == null)
				{
					Debug.LogWarning($"TrackList() Null track in node {node}");
					continue;
				}
				if (track == null)
				{
					continue;
				}
				if (!track.IsActive())
				{
					continue;
				}
				m_Tracks.Add(track);
				if (!m_HasMajorTrack && track.IsMajor())
				{
					m_HasMajorTrack = true;
				}
			}
		}

		public void Initialize(IActObject tree, IActNodeRuntime node, ITreeContext context)
		{
			for (int i = 0; i < m_Tracks.Count; i++)
			{
				Track track = m_Tracks[i];
				if (!track.Initialize(tree, node, context))
				{
					if (track.IsMajor())
					{
						Debug.LogWarning(tree.name + " ActTreeRT.Initialize() Master track " + track + " in " + node +
							" failed to initialize and will be removed from the tree, removing a master track will break the intended logic of this tree");
					}
					m_Tracks.RemoveAt(i);
					i--;
				}
			}
		}

		public bool TryHandleEvent(Params newParams)
		{
			for (int i = 0; i < m_Tracks.Count; i++)
			{
				Track track = m_Tracks[i];
				if (track.IsPlaying() && track.TryHandleEvent(newParams))
				{
					return true;
				}
			}
			return false;
		}
	}
}
