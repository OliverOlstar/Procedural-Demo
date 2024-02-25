
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public class DownloadSpeedMonitor
	{
		private static readonly float SAMPLE_PERIOD = 1.0f;
		private static readonly float SAMPLE_PERIOD_MIN = 0.5f;

		private int[] m_Buffer = new int[60];
		private ulong m_Bytes = 0UL;
		private float m_Time = 0.0f;

		public void Reset()
		{
			int length = m_Buffer.Length;
			for (int i = 0; i < length; i++)
			{
				m_Buffer[i] = 0;
			}
			m_Bytes = 0UL;
			m_Time = 0.0f;
		}

		public void Update(float time, ulong bytes)
		{
			UpdateInternal(time, bytes, SAMPLE_PERIOD);

		}

		public void Finish(float time, ulong bytes)
		{
			// Maybe its a good idea to take a sample even if a full period didn't finish?
			UpdateInternal(time, bytes, SAMPLE_PERIOD_MIN);
		}

		private void UpdateInternal(float time, ulong bytes, float samplePeriod)
		{
			float deltaTime = time - m_Time;
			if (deltaTime < samplePeriod)
			{
				return;
			}
			int deltaBytes = (int)(bytes - m_Bytes);
			float mb = deltaBytes / 1024.0f / 1024.0f;
			int speed = Mathf.RoundToInt(mb / deltaTime);
			m_Buffer[Mathf.Min(speed, m_Buffer.Length - 1)]++;
			m_Time = time;
			m_Bytes = bytes;
		}

		public override string ToString()
		{
			return string.Join(",", m_Buffer);
		}
	}
}
