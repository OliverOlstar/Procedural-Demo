
using UnityEngine;
using System.Collections.Generic;

namespace Act
{
	[System.Serializable]
	public class Node
	{
		public const int INVALID_ID = -1;
		public const int ROOT_ID = 0;

		[SerializeField]
		string m_Name = Core.Str.EMPTY;
		public string GetName() { return m_ID == ROOT_ID ? "Root" : m_Name; }

		[SerializeField]
		int m_ID = INVALID_ID;
		public int GetID() { return m_ID; }

		[SerializeField]
		int m_PointerID = INVALID_ID;
		public int GetPointerID() { return m_PointerID; }

		public Node(int id)
		{
			m_ID = id;
		}

		public override string ToString()
		{
			return "Node(" + m_Name + " : " + m_ID + ")";
		}
	}
}
