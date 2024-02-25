using System.Collections.Generic;

namespace Core
{
	public abstract class StateSwitch
	{
		public delegate void StateChangeDelegate(bool state);

		public abstract event StateChangeDelegate OnStateChanged;
		public abstract bool State { get; }
		public abstract int Count { get; }
	}

	public class StateSwitch<T> : StateSwitch
	{
		public static implicit operator bool(StateSwitch<T> stateSwitch)
		{
			return stateSwitch.State;
		}

		public sealed override event StateChangeDelegate OnStateChanged;
		public sealed override bool State => m_Keys.Count > 0;
		public sealed override int Count => m_Keys.Count;
		public IEnumerable<T> Elements => m_Keys;

		private HashSet<T> m_Keys;

		public StateSwitch()
		{
			m_Keys = new HashSet<T>();
		}

		public StateSwitch(IEqualityComparer<T> comparer)
		{
			m_Keys = new HashSet<T>(comparer);
		}

		public bool SwitchOn(T key)
		{
			if (!m_Keys.Add(key))
			{
				return false;
			}
			if (m_Keys.Count == 1)
			{
				OnStateChanged?.Invoke(true);
			}
			return m_Keys.Count == 1;
		}

		public bool SwitchOff(T key)
		{
			if (!m_Keys.Remove(key))
			{
				return false;
			}
			if (m_Keys.Count == 0)
			{
				OnStateChanged?.Invoke(false);
			}
			return m_Keys.Count == 0;
		}

		public bool ContainsKey(T key)
		{
			return m_Keys.Contains(key);
		}
	}

	public class InverseStateSwitch<T> : StateSwitch
	{
		public static implicit operator bool(InverseStateSwitch<T> stateSwitch)
		{
			return stateSwitch.State;
		}

		public sealed override event StateChangeDelegate OnStateChanged;
		public sealed override bool State => m_Keys.Count >= m_OnCount;
		public sealed override int Count => m_Keys.Count;
		public IEnumerable<T> Elements => m_Keys;

		private int m_OnCount;
		private HashSet<T> m_Keys;

		public InverseStateSwitch(int onCount)
		{
			m_OnCount = onCount;
			m_Keys = new HashSet<T>();
		}

		public InverseStateSwitch(int onCount, IEqualityComparer<T> comparer)
		{
			m_OnCount = onCount;
			m_Keys = new HashSet<T>(comparer);
		}

		public bool SwitchOn(T key)
		{
			if (!m_Keys.Add(key))
			{
				return false;
			}
			if (m_Keys.Count == m_OnCount)
			{
				OnStateChanged?.Invoke(true);
			}
			return m_Keys.Count == m_OnCount;
		}

		public bool SwitchOff(T key)
		{
			if (!m_Keys.Remove(key))
			{
				return false;
			}
			if (m_Keys.Count == m_OnCount - 1)
			{
				OnStateChanged?.Invoke(false);
			}
			return m_Keys.Count == m_OnCount - 1;
		}

		public bool ContainsKey(T key)
		{
			return m_Keys.Contains(key);
		}
	}
}
