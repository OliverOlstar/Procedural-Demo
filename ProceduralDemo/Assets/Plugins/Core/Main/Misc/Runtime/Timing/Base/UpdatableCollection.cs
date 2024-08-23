using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public sealed partial class Chrono : MonoBehaviour
	{
		private abstract class UpdatableCollection
		{
			internal abstract void Update();

			protected abstract void ResetInternal();
			protected abstract bool RegisterInternal(IBaseUpdatable baseUpdatable, int priority);
			protected abstract bool DeregisterInternal(IBaseUpdatable baseUpdatable);

			internal void Reset()
			{
				ResetInternal();
			}

			internal bool Register(IBaseUpdatable baseUpdatable, int priority)
			{
				return RegisterInternal(baseUpdatable, priority);
			}

			internal bool Deregister(IBaseUpdatable baseUpdatable)
			{
				return DeregisterInternal(baseUpdatable);
			}
		}

		private sealed class UpdatableCollection<TUpdatable> : UpdatableCollection, IComparer<int>
			where TUpdatable : IBaseUpdatable
		{
			private const int MIN_ARRAY_SIZE = 8;

			private Action<TUpdatable> m_UpdateAction;
			private SortedDictionary<int, HashSet<TUpdatable>> m_Updatables;
			private Dictionary<TUpdatable, int> m_Priorities;
			private Dictionary<TUpdatable, DateTime> m_LastUpdateTimes;
			private Dictionary<Type, string> m_ProfilerKeys;
			private int[] m_Keys;
			private TUpdatable[] m_Values;
			private bool m_KeysAreDirty;

			internal UpdatableCollection(Action<TUpdatable> updateAction)
			{
				m_UpdateAction = updateAction;
				m_Updatables = new SortedDictionary<int, HashSet<TUpdatable>>(this);
				m_Priorities = new Dictionary<TUpdatable, int>();
				m_LastUpdateTimes = new Dictionary<TUpdatable, DateTime>();
				m_ProfilerKeys = new Dictionary<Type, string>();
				m_Keys = new int[MIN_ARRAY_SIZE];
				m_Values = new TUpdatable[MIN_ARRAY_SIZE];
				m_KeysAreDirty = true;
			}

			internal sealed override void Update()
			{
				UpdateKeys();
				for (int i = 0; i < m_Updatables.Count; ++i)
				{
					if (!m_Updatables.TryGetValue(m_Keys[i], out HashSet<TUpdatable> values))
					{
						continue;
					}
					UpdateValues(values);
					int count = values.Count;
					for (int j = 0; j < count; ++j)
					{
						TUpdatable updatable = m_Values[j];
						if (updatable.DeltaTime <= 0D)
						{
#if !RELEASE
							UnityEngine.Profiling.Profiler.BeginSample(m_ProfilerKeys[updatable.GetType()]);
#endif
							m_UpdateAction?.Invoke(updatable);
#if !RELEASE
							UnityEngine.Profiling.Profiler.EndSample();
#endif
						}
						else
						{
							if (m_LastUpdateTimes.TryGetValue(updatable, out DateTime lastUpdate))
							{
								if ((UtcNow - lastUpdate).TotalSeconds < updatable.DeltaTime)
								{
									continue;
								}
							}
							m_LastUpdateTimes[updatable] = UtcNow;
#if !RELEASE
							UnityEngine.Profiling.Profiler.BeginSample(m_ProfilerKeys[updatable.GetType()]);
#endif
							m_UpdateAction?.Invoke(updatable);
#if !RELEASE
							UnityEngine.Profiling.Profiler.EndSample();
#endif
						}
					}
				}
			}

			protected sealed override void ResetInternal()
			{
				m_Updatables.Clear();
				m_Priorities.Clear();
				m_LastUpdateTimes.Clear();
				Array.Resize(ref m_Keys, MIN_ARRAY_SIZE);
				Array.Resize(ref m_Values, MIN_ARRAY_SIZE);
				m_UpdateAction = null;
			}

			protected sealed override bool RegisterInternal(IBaseUpdatable baseUpdatable, int priority)
			{
				if (!(baseUpdatable is TUpdatable updatable))
				{
					string format = "Tried registering an {0} of the wrong type (received {1}).";
					Debug.LogErrorFormat(format, typeof(TUpdatable).Name, baseUpdatable.GetType().Name);
					return false;
				}
				if (!m_Updatables.TryGetValue(priority, out HashSet<TUpdatable> hashset))
				{
					hashset = new HashSet<TUpdatable>();
					m_Updatables.Add(priority, hashset);
					m_KeysAreDirty = true;
				}
				bool added = hashset.Add(updatable);
				m_Priorities[updatable] = priority;
				Type type = updatable.GetType();
				if (!m_ProfilerKeys.ContainsKey(type))
				{
					m_ProfilerKeys[type] = type.Name;
				}
				updatable?.OnRegistered();
				return added;
			}

			protected sealed override bool DeregisterInternal(IBaseUpdatable baseUpdatable)
			{
				if (!(baseUpdatable is TUpdatable updatable))
				{
					string format = "Tried deregistering an {0} of the wrong type (received {1}).";
					Debug.LogErrorFormat(format, typeof(TUpdatable).Name, baseUpdatable.GetType().Name);
					return false;
				}
				if (!m_Priorities.TryGetValue(updatable, out int priority))
				{
					Debug.LogErrorFormat("Tried deregistering an {0} but couldn't find its priority!", typeof(TUpdatable).Name);
					return false;
				}
				if (!m_Updatables.TryGetValue(priority, out HashSet<TUpdatable> updatables))
				{
					Debug.LogErrorFormat("Tried deregistering an {0} with priority {1} but couldn't find the HashSet<>!", typeof(TUpdatable).Name, priority);
					return false;
				}
				// If we decide to remove an empty updatables collection here, also make sure to set m_KeysAreDirty to true
				bool removed = updatables.Remove(updatable);
				m_Priorities.Remove(updatable);
				updatable?.OnDeregistered();
				return removed;
			}

			private void UpdateKeys()
			{
				// It's not enough to just check the length of the Keys, as the length 
				// could be the same but the contents have changed i.e. 
				// [0, 1, 2] => [1, 2, 3] same length but the keys have still changed -- Josh M. 
				if (m_Updatables.Count > m_Keys.Length || m_KeysAreDirty)
				{
					int length = m_Keys.Length;
					while (m_Updatables.Count > length)
					{
						length <<= 1;
					}
					Array.Resize(ref m_Keys, length);
					int index = 0;
					foreach (int key in m_Updatables.Keys)
					{
						m_Keys[index] = key;
						index++;
					}
					m_KeysAreDirty = false;
				}
			}

			private void UpdateValues(HashSet<TUpdatable> values)
			{
				if (values.Count > m_Values.Length)
				{
					int length = m_Values.Length;
					while (values.Count > length)
					{
						length <<= 1;
					}
					Array.Resize(ref m_Values, length);
				}
				values.CopyTo(m_Values, 0, values.Count);
			}

			int IComparer<int>.Compare(int x, int y)
			{
				return x.CompareTo(y);
			}
		}
	}
}
