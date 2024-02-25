using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ActValueInt
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[System.Serializable]
	public class Constant : ActValue.IInt
	{
		[SerializeField]
		private int m_Constant = 0;

		public Constant() { }

		public Constant(int i)
		{
			m_Constant = i;
		}

		public int GetValue(Act2.ITreeContext context, Act2.ITreeEvent treeEvent)
		{
			return m_Constant;
		}

		public bool RequiresEvent(out System.Type eventType)
		{
			eventType = null;
			return false;
		}

		public override string ToString() => m_Constant.ToString();
	}
}
