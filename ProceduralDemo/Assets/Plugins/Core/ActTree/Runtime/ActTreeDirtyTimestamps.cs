
using UnityEngine;
using System.Collections.Generic;

public static class ActTreeDirtyTimestamps
{
	private static readonly System.DateTime ORIGIN = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

	public static double Timestamp => (System.DateTime.UtcNow - ORIGIN).TotalMilliseconds;

	private static Dictionary<ActTree, double> s_TreeModifiedTime = new Dictionary<ActTree, double>();

	public static void SetDirty(ActTree tree)
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

	public static bool IsDirty(ActTree source, double timestamp)
	{
		s_TreeModifiedTime.TryGetValue(source, out double lastMod);
		return timestamp < lastMod;
	}
}
