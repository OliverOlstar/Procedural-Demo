
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ActVarBehaviour : MonoBehaviour
{
	public class Values : Dictionary<string, float>
	{
		public Values() : base() { }
		public Values(int size) : base(size) { }
		public Values(IEnumerable<KeyValuePair<string, float>> vars) : base(vars) { }
	}

	public class References : Dictionary<string, UnityEngine.Object>
	{
		public References() : base() { }
		public References(int size) : base(size) { }
		public References(IEnumerable<KeyValuePair<string, UnityEngine.Object>> vars) : base(vars) { }
	}

	public class Vectors : Dictionary<string, Vector3>
	{
		public Vectors() : base() { }
		public Vectors(int size) : base(size) { }
		public Vectors(IEnumerable<KeyValuePair<string, Vector3>> vars) : base(vars) { }
	}

	[System.Serializable]
	public class ActVarInit
	{
		[UberPicker.AssetNonNull]
		public SOActVar Var = null;
		public int StartValue = 1;

		public bool HasMin = false;

		[Core.Conditional(nameof(HasMin), true)]
		public float MinValue = 0.0f;

		public bool HasMax = false;
		[Core.Conditional(nameof(HasMax), true)]
		public float MaxValue = 1.0f;
	}

#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.DrawWithUnity]
#endif
	[SerializeField]
	private List<ActVarInit> m_InitVars = new List<ActVarInit>();

	private Dictionary<string, ActVarInit> m_MinMaxVars = new Dictionary<string, ActVarInit>();

	private Values m_Vars = new Values();
	public IReadOnlyDictionary<string, float> Vars => m_Vars;

	private Values m_Timers = new Values();
	public IReadOnlyDictionary<string, float> Timers => m_Timers;

	private References m_References = new References();
	public IReadOnlyDictionary<string, UnityEngine.Object> Refs => m_References;

	private Vectors m_Vectors = new Vectors();
	public IReadOnlyDictionary<string, Vector3> Vector3s => m_Vectors;

	private float m_InitialTimeStamp = 0.0f;

	public static ActVarBehaviour Get(GameObject gameObject)
	{
		ActVarBehaviour vars = gameObject.GetComponent<ActVarBehaviour>();
		if (vars != null)
		{
			return vars;
		}
		else
		{
			return gameObject.AddComponent<ActVarBehaviour>();
		}
	}

	protected virtual void Awake()
	{
		foreach (ActVarInit init in m_InitVars)
		{
			if (init.Var != null)
			{
				if (init.HasMin || init.HasMax)
				{
					m_MinMaxVars.Add(init.Var.Name, init);
				}
			}
		}
	}
	
	private void OnEnable()
	{
		foreach (ActVarInit init in m_InitVars)
		{
			if (init.Var != null)
			{
				m_Vars.Add(init.Var.Name, init.StartValue);
			}
		}
		m_InitialTimeStamp = Time.time;
	}

	private void OnDisable()
	{
		m_Vars.Clear();
		m_Timers.Clear();
		m_References.Clear();
		m_Vectors.Clear();
	}

	public int GetVarValue(SOActVar var) => GetVarValue(var.Name);
	public int GetVarValue(string var)
	{
		return m_Vars.TryGetValue(var, out float value) ? Mathf.RoundToInt(value) : 0;
	}

	public float GetVarFloat(SOActVar var) => GetVarFloat(var.Name);
	public float GetVarFloat(string var)
	{
		return m_Vars.TryGetValue(var, out float value) ? value : 0.0f;
	}

	public bool GetVarBool(SOActVar var) => GetVarBool(var.Name);
	public bool GetVarBool(string var)
	{
		return m_Vars.TryGetValue(var, out float value) ? (value > Core.Util.EPSILON) : false;
	}

	public void SetVarValue(SOActVar var, float value)
	{
		SetVarValue(var.Name, value);
	}
	public void SetVarValue(SOActVar var, int value)
	{
		SetVarValue(var.Name, value);
	}
	public void SetVarValue(SOActVar var, bool value)
	{
		SetVarValue(var.Name, value ? 1.0f : 0.0f);
	}
	public void SetVarValue(string var, float value)
	{
		if (string.IsNullOrEmpty(var))
		{
			return;
		}
		value = ClampValue(var, value);
		if (!m_Vars.TryGetValue(var, out float initialValue))
		{
			m_Vars.Add(var, value);
		}
		else if (Core.Util.Approximately(initialValue, value))
		{
			return; // Early out to avoid adding a snapshot to the debugger if the value doesn't actually change
		}
		else
		{
			m_Vars[var] = value;
		}
		ActVarDebugData.AddVarSnapShot(this, var);
	}

	public void VarMath(SOActVar var, SOActVar.Operator op, float value)
	{
		if (var == null)
		{
			return;
		}

		float currentValue = GetVarValue(var);
		switch (op)
		{
			case SOActVar.Operator.Add:
				currentValue += value;
				break;
			case SOActVar.Operator.Subtract:
				currentValue -= value;
				break;
		}

		SetVarValue(var, currentValue);
	}

	private float ClampValue(string var, float value)
	{
		if (m_MinMaxVars.TryGetValue(var, out ActVarInit minMax))
		{
			if (minMax.HasMax)
			{
				value = Mathf.Min(value, minMax.MaxValue);
			}
			if (minMax.HasMin)
			{
				value = Mathf.Max(value, minMax.MinValue);
			}
		}
		return value;
	}

	public float GetTime(SOActVar var) => GetTime(var.Name);
	public float GetTime(string var)
	{
		float timeStamp = m_Timers.TryGetValue(var, out float value) ? value : m_InitialTimeStamp;
		float time = Time.time - timeStamp;
		return time;
	}
	public void StartTimer(SOActVar var)
	{
		StartTimer(var.Name);
	}
	public void StartTimer(string var)
	{
		if (string.IsNullOrEmpty(var))
		{
			return;
		}
		if (m_Timers.ContainsKey(var))
		{
			m_Timers[var] = Time.time;
		}
		else
		{
			m_Timers.Add(var, Time.time);
		}
		ActVarDebugData.AddVarTimerShot(this, var);
	}

	public void SetReference(SOActVar var, UnityEngine.Object obj) => SetReference(var.Name, obj);
	public void SetReference(string var, UnityEngine.Object obj)
	{
		if (string.IsNullOrEmpty(var))
		{
			return;
		}
		if (!m_References.TryGetValue(var, out UnityEngine.Object initialValue))
		{
			m_References.Add(var, obj);
		}
		else if (obj == initialValue)
		{
			return; // Early out to avoid adding a snapshot to the debugger if the value doesn't actually change
		}
		else if (obj == null && initialValue != null)
		{
			m_References.Remove(var); // Remove null items
		}
		else
		{
			m_References[var] = obj;
		}
		ActVarDebugData.AddVarReferenceShot(this, var);
	}
	public void ClearReference(SOActVar var) => SetReference(var, null);
	public void ClearReference(string var) => SetReference(var, null);

	public bool TryGetReference<T>(SOActVar var, out T reference) where T : UnityEngine.Object 
		=> TryGetReference(var.Name, out reference);
	public bool TryGetReference<T>(string var, out T reference) where T : UnityEngine.Object
	{
		if (!m_References.TryGetValue(var, out UnityEngine.Object obj))
		{
			reference = null;
			return false;
		}
		reference = obj as T;
		return reference != null;
	}

	public bool HasReference<T>(SOActVar var) where T : UnityEngine.Object
		=> TryGetReference<T>(var.Name, out _);
	public bool HasReference<T>(string var) where T : UnityEngine.Object
		=> TryGetReference<T>(var, out _);

	public void SetVector(SOActVar var, Vector3 vector) => SetVector(var.Name, vector);
	public void SetVector(string var, Vector3 vector)
	{
		if (string.IsNullOrEmpty(var))
		{
			return;
		}
		if (!m_Vectors.TryGetValue(var, out Vector3 initialValue))
		{
			m_Vectors.Add(var, vector);
		}
		else if (Core.Util.Approximately(vector, initialValue))
		{
			return; // Early out to avoid adding a snapshot to the debugger if the value doesn't actually change
		}
		else
		{
			m_Vectors[var] = vector;
		}
		ActVarDebugData.AddVarReferenceShot(this, var);
	}
	public bool TryGetVector(SOActVar var, out Vector3 vector) => TryGetVector(var.Name, out vector);
	public bool TryGetVector(string var, out Vector3 vector) => m_Vectors.TryGetValue(var, out vector);
}
