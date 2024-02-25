using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ActValue
{
	public interface IInt
	{
		int GetValue(Act2.ITreeContext context, Act2.ITreeEvent treeEvent);
		bool RequiresEvent(out System.Type eventType);
	}

	[MovedFrom(true, sourceAssembly: "Core")]
	[System.Serializable]
	public class Int : Base
	{
		[SerializeReference]
		private IInt m_Value = null;

		public Int(int i)
		{
			m_Value = new ActValueInt.Constant(i);
		}

		public int GetValue(Act2.ITreeContext context, Act2.ITreeEvent treeEvent)
		{
			if (m_Value == null)
			{
				return 0;
			}
			return m_Value.GetValue(context, treeEvent);
		}

		public bool RequiresEvent(out System.Type eventType)
		{
			if (m_Value == null)
			{
				eventType = null;
				return false;
			}
			return m_Value.RequiresEvent(out eventType);
		}

		public override string ToString()
		{
			return m_Value == null ? "NULL" : m_Value.ToString();
		}
	}
}
