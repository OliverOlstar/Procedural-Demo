
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ActValueInt
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[System.Serializable]
	public class Lambda : ActValue.IInt
	{
		public enum Round
		{
			Nearest,
			Floor,
			Ceilling
		}

		[SerializeField, UberPicker.AssetNonNull]
		private FloatLambdaBase m_Value = null;

		[SerializeField]
		private Round m_RoundToInt = Round.Nearest;

		public int GetValue(Act2.ITreeContext context, Act2.ITreeEvent treeEvent)
		{
			m_Value.TryEvaluate(context, treeEvent, out float value);
			switch (m_RoundToInt)
			{
				case Round.Floor:
					return Mathf.FloorToInt(value);
				case Round.Ceilling:
					return Mathf.CeilToInt(value);
				default:
					return Mathf.RoundToInt(value);
			}
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
