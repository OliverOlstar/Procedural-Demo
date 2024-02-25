
using UnityEngine;
using System.Collections.Generic;

namespace Act2
{
	public interface ITreeContext
	{

	}

	public interface ITreeEvent
	{

	}

	public class Params
	{
		public enum State
		{
			None,
			Failed,
			Interrupted,
			Ended,
			Playing,
		}

		private Dictionary<int, State> m_State = new Dictionary<int, State>();

		private ITreeEvent m_Event = null;
		public ITreeEvent Event => m_Event;
		public void SetEvent(ITreeEvent treeEvent) => m_Event = treeEvent;

		private bool m_Processed = false;
		public void Process() { m_Processed = true; }

		public Params(ITreeEvent treeEvent)
		{
			m_Event = treeEvent ?? throw new System.ArgumentNullException("Act2.Params() Event cannot be null");
		}

		public override string ToString() => $"ActParams({m_Event})";

		public State GetState()
		{
			State state = m_Processed ? State.Failed : State.None;
			foreach (State s in m_State.Values)
			{
				switch (s)
				{
					case State.Playing:
						return State.Playing;
					case State.Ended:
						state = State.Ended;
						break;
					case State.Interrupted:
						if (state != State.Ended)
						{
							state = State.Interrupted;
						}
						break;
				}
			}
			return state;
		}

		public void Play(int nodeID)
		{
			if (nodeID == Act2.Node.ROOT_ID) // Ignore root node because it doesn't stop playing
			{
				return;
			}
			if (m_State.ContainsKey(nodeID))
			{
				// We expect the state could be finished because a node can sequence to itself
				State previousState = m_State[nodeID];
				if (previousState != State.Ended && previousState != State.Interrupted)
				{
					Debug.LogError(Core.Str.Build(ToString(), ".Play() Node ",
						nodeID.ToString(), " already has state ", previousState.ToString()));
				}
				else
				{
					m_State[nodeID] = State.Playing;
				}
				//Debug.Log(Core.Str.Build(this.ToString(), " ActParams.Play() ", nodeID.ToString(), " ", m_State.Count.ToString(), " ", Time.time.ToString(), "\n", DebugStateToString()));
				return;
			}
			m_State.Add(nodeID, State.Playing);
			//Debug.Log(Core.Str.Build(this.ToString(), " ActParams.Play() ", nodeID.ToString(), " ", m_State.Count.ToString(), " ", Time.time.ToString(), "\n", DebugStateToString()));
		}

		public void Stop(int nodeID, bool interrupted)
		{
			if (nodeID == Act2.Node.ROOT_ID) // Ignore root node because it doesn't stop playing
			{
				return;
			}
			if (!m_State.ContainsKey(nodeID))
			{
				Debug.LogError(Core.Str.Build(ToString(), ".Stop() Node ", nodeID.ToString(), " not found"));
				return;
			}
			State current = m_State[nodeID];
			if (current != State.Playing)
			{
				Debug.LogError(Core.Str.Build(ToString(), ".Stop() Node ",
					nodeID.ToString(), " is already ", m_State[nodeID].ToString()));
				return;
			}
			m_State[nodeID] = interrupted ? State.Interrupted : State.Ended;
			//Debug.Log(Core.Str.Build(this.ToString(), " ActParams.Stop() ", nodeID.ToString(), " ", m_State.Count.ToString(), " ", Time.time.ToString(), "\n", DebugStateToString()));
		}

		public void Reset()
		{
			if (GetState() == State.Playing)
			{
				Debug.LogError(Core.Str.Build(ToString(), ".Reset() Cannot reset params that are still sequenced ", DebugStateToString()));
				return;
			}

			m_Processed = false;
			m_State.Clear();
		}

		public T Copy<T>() where T : Params
		{
			T copy = MemberwiseClone() as T;
			if (copy == null)
			{
				throw new System.ArgumentException($"ActParams.Copy() Type {GetType().Name} is not assignable to type {typeof(T).Name}");
			}
			copy.m_State = new Dictionary<int, State>(); // Copy cannot point to our state dictionary
			copy.Reset();
			return copy;
		}

		public string DebugStateToString()
		{
			foreach (int key in m_State.Keys)
			{
				Core.Str.AddLine(key.ToString(), " ", m_State[key].ToString());
			}
			return Core.Str.Finish();
		}
	}
}
