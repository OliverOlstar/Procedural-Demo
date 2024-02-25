using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher
{
    public static class AudioPriority
    {
		public enum Enum
		{
			None,
			Player,
			Dialog,
			UI,
			Music,
			Ambiance
		}
		
		// 0 - 256
		private static readonly int[] PriorityInt = new int[]
		{
			130,
			130,
			140,
			200,
			150,
			100,
		};

		public static int ToInt(this Enum pEnum) => PriorityInt[(int)pEnum];		
    }
}
