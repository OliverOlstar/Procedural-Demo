using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public struct TimedItemProperties
	{
		public float TotalTimeRange;
		public float ValidTimeRange;
	}

	public static class TimedItemUtil
	{
		private static readonly float DEFAULT_TIME_RANGE = 2.0f;

		public static List<(Track, int)> GetSortedTracks(List<Track> tracks)
		{
			List<(Track, int)> sortedTracks = new List<(Track, int)>();
			for (int i = 0; i < tracks.Count; i++)
			{
				if (tracks[i] != null) // Possible this can happen if a track gets removed from code but serialized in trees
				{
					sortedTracks.Add((tracks[i], i));
				}
			}
			sortedTracks.Sort(CompareTracks);
			return sortedTracks;
		}
		private static int CompareTracks((Track, int) a, (Track, int) b)
		{
			return Track.CompareTracks(a.Item1, b.Item1);
		}

		public static TimedItemProperties GetTimedItemProperties(IEnumerable<ITimedItem> timedItems)
		{
			float slaveTime = 0.0f;
			float approxSlaveTime = 0.0f;
			float masterTime = 0.0f;
			float approxMasterTime = 0.0f;
			bool positiveMaster = false;
			bool negativeMaster = false;
			bool anyMaster = false;

			foreach (ITimedItem track in timedItems)
			{
				if (track == null)
				{
					continue;
				}
				float time;
				bool negativeTime = false; // Negative times have to be approximates as they are decided at runtime
				if (track.HasEndEvent())
				{
					time = track._EditorDisplayEndTime();
					if (time < 0.0f)
					{
						time = track.GetStartTime() + DEFAULT_TIME_RANGE;
						negativeTime = true;
					}
				}
				else
				{
					time = track.GetStartTime();
					if (time < 0.0f)
					{
						time = DEFAULT_TIME_RANGE;
						negativeTime = true;
					}
				}
				if (negativeTime)
				{
					approxSlaveTime = Mathf.Max(approxSlaveTime, time);
				}
				else
				{
					slaveTime = Mathf.Max(slaveTime, time);
				}
				if (track.IsMajor())
				{
					anyMaster = true;
					if (negativeTime)
					{
						negativeMaster = true;
						approxMasterTime = Mathf.Max(approxMasterTime, time);
					}
					else
					{
						positiveMaster = true;
						masterTime = Mathf.Max(masterTime, time);
					}
				}
			}

			// Don't want to include approximate slave times unless there are no master tracks
			float totalTime = Mathf.Max(slaveTime, masterTime, approxMasterTime, Core.Util.SPF30);
			if (!anyMaster)
			{
				totalTime = Mathf.Max(totalTime, approxSlaveTime);
			}
			// If we have master tracks that are only positive, then we want to validate that slave tracks are within the time range of our positive masters, otherwise they might not have a change to start/end properly
			// If a master track has negative end time then it will keep the node alive long enough for any track to play so all timings are valid
			// Also if there are no positive master tracks then entire time range is valid, nothing to validate against
			float validTime;
			if (positiveMaster && !negativeMaster)
			{
				validTime = masterTime;
			}
			else
			{
				// All time ranges are valid, scale the time range by a magic number so the negative tracks will always be longer than the positive tracks
				totalTime *= 1.15f;
				validTime = totalTime;
			}

			TimedItemProperties properties = new TimedItemProperties
			{
				TotalTimeRange = totalTime, // Total time range of all tracks
				ValidTimeRange = validTime, // Total time range of master track
			};
			return properties;
		}
	}
}

