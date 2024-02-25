using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	// Accessing UnityEngine.Object.name causes GC allocs so this class caches it so we can use name to key Object without allocations
	public class SOCacheName : ScriptableObject
	{
		private string m_SOName = null;

		private int m_SONameHash = 0;

		public string Name
		{
			get
			{
				if (string.IsNullOrEmpty(m_SOName))
				{
					m_SOName = base.name;
				}
				return m_SOName;
			}
		}

		public int NameHash
		{
			get
			{
				if (m_SONameHash == 0)
				{
					m_SONameHash = Animator.StringToHash(Name);
				}
				return m_SONameHash;
			}
		}

		public void SetName(string name)
		{
			base.name = name;
			m_SOName = name;
			m_SONameHash = 0;
		}

		[System.Obsolete("Use 'Name' property instead it doesn't cause GC allocs", true)]
		public new string name => throw new System.InvalidOperationException("Use 'Name' property instead it doesn't cause GC allocs");

		protected virtual void OnValidate()
		{
			// Need to clear cached name if we get renamed at edit time
			m_SOName = null;
			m_SONameHash = 0;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}

			SOCacheName other = (SOCacheName)obj;
			return this.Name == other.Name;
		}
	}
}
