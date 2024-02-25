using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ActValueInt
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[System.Serializable]
	public class ActVar : ActValue.IInt, ActValue.IBool
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

		public int GetValue(Act2.ITreeContext context, Act2.ITreeEvent treeEvent)
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
				return 0;
			}
			return vars.GetVarValue(m_Var);
		}

		public bool GetBool(Act2.ITreeContext context, Act2.ITreeEvent treeEvent)
		{
			int value = GetValue(context, treeEvent);
			return value > 0;
		}

		public bool RequiresEvent(out System.Type eventType)
		{
			eventType = null;
			return false;
		}

		public override string ToString() => m_Var == null ? "NULL" : m_Var.Name;
	}
}
