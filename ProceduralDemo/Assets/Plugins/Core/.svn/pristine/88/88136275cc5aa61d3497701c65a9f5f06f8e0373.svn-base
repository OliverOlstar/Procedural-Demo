using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ActValueFloat
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[System.Serializable]
	public class Lambda : ActValue.IFloat
	{
		[SerializeField, UberPicker.AssetNonNull]
		private FloatLambdaBase m_Value = null;

		public float GetValue(Act2.ITreeContext context, Act2.ITreeEvent treeEvent)
		{
			m_Value.TryEvaluate(context, treeEvent, out float value);
			return value;
		}

		public bool RequiresEvent(out System.Type eventType)
		{
			if (m_Value != null && m_Value.RequiresEvent(out eventType))
			{
				return true;
			}
			eventType = null;
			return false;
		}

		public override string ToString() => m_Value == null ? "NULL" : m_Value.name;
	}
}
