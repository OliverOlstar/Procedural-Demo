namespace ODev
{
	public static class PauseSystem
    {
        public delegate void PauseEvent();

        public static PauseEvent s_OnPause;
        public static PauseEvent s_OnUnpause;
		private static bool s_IsPaused = false;

		public static bool IsPaused => s_IsPaused;

		public static void Pause(bool pPause)
        {
            if (s_IsPaused != pPause)
            {
				s_IsPaused = pPause;
                if (s_IsPaused)
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