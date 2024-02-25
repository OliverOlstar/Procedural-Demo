using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public interface IActVarContext : Act2.ITreeContext
{
	ActVarBehaviour ActVarBehaviour { get; }
}

namespace ActValueFloat
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[System.Serializable]
	public class ActVar : ActValue.IFloat
	{
		public enum Source
		{
			Local = 0,
			Global
		}

		[SerializeField]
		private Source m_VarSource = Source.Local;

		[SerializeField, UberPicker.AssetNonNull]
		private SOActVar m_Var = null;

		public float GetValue(Act2.ITreeContext context, Act2.ITreeEvent treeEvent)
		{
			ActVarBehaviour vars = null;
			switch (m_VarSource)
			{
				case Source.Global:
					vars = GlobalActVarBehaviour.GetOrCreate();
					break;
				default:
					if (context is IActVarContext varContext)
					{
						vars = varContext.ActVarBehaviour;
					}
					break;
			}
			if (vars == null)
			{
				return 0.0f;
			}
			return vars.GetVarFloat(m_Var);
		}

		public bool RequiresEvent(out System.Type eventType)
		{
			eventType = null;
			return false;
		}

		public override string ToString() => m_Var == null ? "NULL" : m_Var.Name;
	}
}
