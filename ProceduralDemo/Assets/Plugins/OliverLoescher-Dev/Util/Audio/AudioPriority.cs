namespace ODev
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
		private static readonly int[] s_PriorityInt = new int[]
		{
			130,
			130,
			140,
			200,
			150,
			100,
		};

		public static int ToInt(this Enum pEnum) => s_PriorityInt[(int)pEnum];		
    }
}
