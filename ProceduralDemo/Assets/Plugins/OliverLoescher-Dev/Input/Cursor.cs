using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ODev.Input
{
    public class Cursor : Singleton<Cursor>
    {
		private int m_ConfinedCount = 0;
		private int m_LockedCount = 0;

		public static void AddLocked()
		{
			Cursor instance = Instance;
			if (instance == null)
			{
				return;
			}
			instance.m_LockedCount++;
			instance.UpdateLockState();
		}
		public static void RemoveLocked()
		{
			Cursor instance = Instance;
			if (instance == null)
			{
				return;
			}
			instance.m_LockedCount--;
			instance.UpdateLockState();
		}

		public static void AddConfined()
		{
			Cursor instance = Instance;
			if (instance == null)
			{
				return;
			}
			instance.m_ConfinedCount++;
			instance.UpdateLockState();
		}
		public static void RemoveConfined()
		{
			Cursor instance = Instance;
			if (instance == null)
			{
				return;
			}
			instance.m_ConfinedCount--;
			instance.UpdateLockState();
		}

		private void UpdateLockState()
		{
			if (m_ConfinedCount > 0)
			{
				UnityEngine.Cursor.lockState = CursorLockMode.Confined;
				return;
			}
			if (m_LockedCount > 0)
			{
				UnityEngine.Cursor.lockState = CursorLockMode.Locked;
				return;
			}
			UnityEngine.Cursor.lockState = CursorLockMode.None;
		}
	}
}
