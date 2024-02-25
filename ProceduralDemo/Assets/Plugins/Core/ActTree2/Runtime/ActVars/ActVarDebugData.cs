
using System.Collections.Generic;
using UnityEngine;

public class CircularList<T>
{
	T[] array = null;
	int stopIndex = 0;

	public CircularList(int size)
	{
		array = new T[size];
	}

	public int Count
	{
		get { return Mathf.Min(stopIndex, array.Length); }
	}

	public T this[int index]
	{
		get
		{
			if (index < 0 || index >= Count)
			{
				Debug.LogError("Index out of range");
				return default(T);
			}
			int startIndex = stopIndex - Count;
			int i = (startIndex + index) % array.Length;
			return array[i];
		}
	}

	public void Add(T add)
	{
		int i = stopIndex % array.Length;
		array[i] = add;
		stopIndex++;
	}
}

public class ActVarDebugData
{
	static readonly int MAX_SHOTS = 50;

	public struct DebugInfo
	{
		public ActVarBehaviour behaviour;
		public CircularList<SnapShot> snapShots;

		public DebugInfo(ActVarBehaviour behaviour, int snapShotCount)
		{
			this.behaviour = behaviour;
			this.snapShots = new CircularList<SnapShot>(snapShotCount);
		}
	}

	public struct SnapShot
	{
		public enum VarType
		{
			Value = 0,
			Timer,
			Reference
		}

		public float TimeStamp;
		public string VarName;
		public VarType Type;
		public ActVarBehaviour.Values Vars;
		public ActVarBehaviour.Values Timers;
		public Dictionary<string, string> References;

		public SnapShot(string varName, VarType type, ActVarBehaviour behaviour, float timeStamp)
		{
			TimeStamp = timeStamp;
			VarName = varName;
			Type = type;
			Vars = new ActVarBehaviour.Values(behaviour.Vars);
			Timers = new ActVarBehaviour.Values(behaviour.Timers);
			References = new Dictionary<string, string>(behaviour.Refs.Count);
			foreach (KeyValuePair<string, Object> pair in behaviour.Refs)
			{
				string objName = 
					ReferenceEquals(pair.Value, null) ? "[Null]" :
					pair.Value == null ? "[Destroyed]" :
					pair.Value.name;
				References.Add(pair.Key, objName);
			}
			foreach (KeyValuePair<string, Vector3> v in behaviour.Vector3s)
			{
				Vars[v.Key + ".x"] = v.Value.x;
				Vars[v.Key + ".y"] = v.Value.y;
				Vars[v.Key + ".z"] = v.Value.z;
			}
		}
	}

	public static Dictionary<string, DebugInfo> s_Info = new Dictionary<string, DebugInfo>();

	public static void AddVarSnapShot(ActVarBehaviour behaviour, string var) => AddSnapShot(behaviour, var, SnapShot.VarType.Value);
	public static void AddVarTimerShot(ActVarBehaviour behaviour, string var) => AddSnapShot(behaviour, var, SnapShot.VarType.Timer);
	public static void AddVarReferenceShot(ActVarBehaviour behaviour, string var) => AddSnapShot(behaviour, var, SnapShot.VarType.Reference);

	private static void AddSnapShot(ActVarBehaviour behaviour, string var, SnapShot.VarType type)
	{
		if (!Application.isEditor)
		{
			return;
		}
		DebugInfo? info = null;
		if (!s_Info.ContainsKey(behaviour.name))
		{
			info = new DebugInfo(behaviour, MAX_SHOTS);
			s_Info.Add(behaviour.name, info.Value);
		}
		else
		{
			info = s_Info[behaviour.name];
		}
		SnapShot shot = new SnapShot(var, type, behaviour, Time.time);
		info.Value.snapShots.Add(shot);
	}
}
