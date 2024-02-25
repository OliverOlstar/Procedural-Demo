
using UnityEngine;
using System.Collections.Generic;

namespace Act2
{
	public static class TreeDirtyTimestamps
	{
		private static readonly System.DateTime ORIGIN = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

		public static double Timestamp => (System.DateTime.UtcNow - ORIGIN).TotalMilliseconds;

		private static Dictionary<IActObject, double> s_TreeModifiedTime = new Dictionary<IActObject, double>();

		public static void SetDirty(IActObject tree)
		{
			if (!s_TreeModifiedTime.ContainsKey(tree))
			{
				s_TreeModifiedTime.Add(tree, Timestamp);
			}
			else
			{
				s_TreeModifiedTime[tree] = Timestamp;
			}
		}

		public static bool IsDirty(IActObject source, double timestamp)
		{
			s_TreeModifiedTime.TryGetValue(source, out double lastMod);
			return timestamp < lastMod;
		}
	}
}
