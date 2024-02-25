using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ActValue
{
	public interface IFloat
	{
		float GetValue(Act2.ITreeContext context, Act2.ITreeEvent treeEvent);
		bool RequiresEvent(out System.Type eventType);
	}

	// Base class to attach inspector
	public class Base { }

	[MovedFrom(true, sourceAssembly: "Core")]
	[System.Serializable]
	public class Float : Base
	{
		[SerializeReference]
		private IFloat m_Value = null;

		public Float(float f)
		{
			m_Value = new ActValueFloat.Constant(f);
		}

		public float GetValue(Act2.ITreeContext context, Act2.ITreeEvent treeEvent)
		{
			if (m_Value == null)
			{
				return 0.0f;
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
