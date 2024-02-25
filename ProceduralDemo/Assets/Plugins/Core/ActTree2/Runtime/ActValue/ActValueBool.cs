using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ActValue
{
	public interface IBool
	{
		bool GetBool(Act2.ITreeContext context, Act2.ITreeEvent treeEvent);
		bool RequiresEvent(out System.Type eventType);
	}

	[MovedFrom(true, sourceAssembly: "Core")]
	[System.Serializable]
	public class Bool : Base
	{
		[SerializeReference]
		private IBool m_Value = null;

		public Bool(bool b)
		{
			m_Value = new ActValueBool.Constant(b);
		}

		public bool GetValue(Act2.ITreeContext context, Act2.ITreeEvent treeEvent)
		{
			if (m_Value == null)
			{
				return false;
			}
			return m_Value.GetBool(context, treeEvent);
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
