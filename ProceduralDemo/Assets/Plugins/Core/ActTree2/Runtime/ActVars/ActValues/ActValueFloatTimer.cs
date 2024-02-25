using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ActValueFloat
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[System.Serializable]
	public class ActTimer : ActValue.IFloat
	{
		public enum Source
		{
			Local = 0,
			Global
		}

		[SerializeField]
		private Source m_TimerSource = Source.Local;

		[SerializeField, UberPicker.AssetNonNull]
		private SOActVar m_Timer = null;

		public float GetValue(Act2.ITreeContext context, Act2.ITreeEvent treeEvent)
		{
			ActVarBehaviour vars = null;
			switch (m_TimerSource)
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
			return vars.GetTime(m_Timer);
		}

		public bool RequiresEvent(out System.Type eventType)
		{
			eventType = null;
			return false;
		}

		public override string ToString() => m_Timer == null ? "NULL" : m_Timer.Name;
	}
}
