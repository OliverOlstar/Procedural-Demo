using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ActValueFloat
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[System.Serializable]
	public class Constant : ActValue.IFloat
	{
		[SerializeField]
		private float m_Constant = 0.0f;

		public Constant() { }

		public Constant(float f)
		{
			m_Constant = f;
		}

		public float GetValue(Act2.ITreeContext context, Act2.ITreeEvent treeEvent)
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
