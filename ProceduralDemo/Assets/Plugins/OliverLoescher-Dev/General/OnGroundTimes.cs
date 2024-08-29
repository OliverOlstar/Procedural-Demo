using ODev.Util;

namespace ODev
{
	[System.Serializable]
    public class OnGroundTimes
    {
		private OnGround m_OnGround;

		private float m_GroundTime = 0.0f;
		private float m_SlopeTime = 0.0f;
		private float m_AirTime = 0.0f;

		public float TimeOnGround => m_OnGround.IsOnGround ? m_GroundTime : 0.0f;
		public float TimeOffGround => m_OnGround.IsOnGround ? 0.0f : m_GroundTime;
		public float TimeOnSlope => m_OnGround.IsOnSlope ? m_SlopeTime : 0.0f;
		public float TimeOffSlope => m_OnGround.IsOnSlope ? 0.0f : m_SlopeTime;
		public float TimeInAir => m_OnGround.IsInAir ? m_AirTime : 0.0f;
		public float TimeOutOfAir => m_OnGround.IsInAir ? 0.0f : m_AirTime;

		internal void Initalize(OnGround pOnGround)
		{
			m_OnGround = pOnGround;
			m_OnGround.OnGroundEnterEvent.AddListener(OnGroundEvent);
			m_OnGround.OnGroundExitEvent.AddListener(OnGroundEvent);
			m_OnGround.OnSlopeEnterEvent.AddListener(OnSlopeEvent);
			m_OnGround.OnSlopeExitEvent.AddListener(OnSlopeEvent);
			m_OnGround.OnAirEnterEvent.AddListener(OnAirEvent);
			m_OnGround.OnAirExitEvent.AddListener(OnAirEvent);
		}

		internal void Destroy()
		{
			if (m_OnGround == null)
			{
				this.DevException("Already destroyed");
				return;
			}
			m_OnGround.OnGroundEnterEvent.RemoveListener(OnGroundEvent);
			m_OnGround.OnGroundExitEvent.RemoveListener(OnGroundEvent);
			m_OnGround.OnSlopeEnterEvent.RemoveListener(OnSlopeEvent);
			m_OnGround.OnSlopeExitEvent.RemoveListener(OnSlopeEvent);
			m_OnGround.OnAirEnterEvent.RemoveListener(OnAirEvent);
			m_OnGround.OnAirExitEvent.RemoveListener(OnAirEvent);
			m_OnGround = null;
		}

		private void OnGroundEvent() => m_GroundTime = 0.0f;
		private void OnSlopeEvent() => m_SlopeTime = 0.0f;
		private void OnAirEvent() => m_AirTime = 0.0f;

		internal void Tick(float pDeltaTime)
		{
			m_GroundTime += pDeltaTime;
			m_SlopeTime += pDeltaTime;
			m_AirTime += pDeltaTime;
		}
	}
}
