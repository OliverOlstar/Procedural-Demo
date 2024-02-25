#if UNITY_2021_1_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public abstract class SerialRefList<T> : ReorderableListBase<T>
	{
		[SerializeReference]
		private List<T> m_List = new List<T>();
		protected override List<T> List => m_List;
	}
}
#endif
