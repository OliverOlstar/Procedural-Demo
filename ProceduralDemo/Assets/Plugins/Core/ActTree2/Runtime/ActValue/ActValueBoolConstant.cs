using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ActValueBool
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[System.Serializable]
	public class Constant : ActValue.IBool
	{
		[SerializeField]
		private bool m_Constant = true;

		public Constant() { }

		public Constant(bool b)
		{
			m_Constant = b;
		}

		public bool GetBool(Act2.ITreeContext context, Act2.ITreeEvent treeEvent)
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
