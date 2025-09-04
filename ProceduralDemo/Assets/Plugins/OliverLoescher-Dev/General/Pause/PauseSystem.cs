using ODev.Util;

namespace ODev
{
	public static class PauseSystem
	{
		public delegate void PauseEvent();

		public static PauseEvent s_OnPause;
		public static PauseEvent s_OnUnpause;
		private static int s_PausedCount = 0;

		public static bool IsPaused => s_PausedCount > 0;

		public static void Pause(bool pPause)
		{
			bool wasPaused = IsPaused;
			s_PausedCount += pPause ? 1 : -1;
			if (s_PausedCount < 0)
			{
				typeof(PauseSystem).DevException($"{nameof(s_PausedCount)}: {s_PausedCount} is below zero");
				return;
			}
			typeof(PauseSystem).Log($"{nameof(s_PausedCount)}: {s_PausedCount}");

			if (wasPaused != IsPaused)
			{
				if (IsPaused)
				{
					s_OnPause?.Invoke();
				}
				else
				{
					s_OnUnpause?.Invoke();
				}
			}
		}
	}
}