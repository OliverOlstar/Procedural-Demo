using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scriptionary
{
	public abstract class ScriptionaryItemReferenceBase
	{
		// For attaching property drawer
	}

	public abstract class ScriptionaryItemReference<TSO, TItem> : ScriptionaryItemReferenceBase
		where TSO : SOScriptionaryGeneric<TItem>
		where TItem : IScriptionaryItem
	{
		[SerializeField, UberPicker.AssetNonNull]
		private TSO m_Scriptionary = null;

		[SerializeField]
		private string m_Key = string.Empty;

		public TItem GetValue()
		{
			if (m_Scriptionary == null)
			{
				Core.DebugUtil.DevException($"{GetType().Name}.GetValue() Scriptionary reference is null");
				return default;
			}
			if (!m_Scriptionary.TryGet(m_Key, out TItem value))
			{
				Core.DebugUtil.DevException($"{GetType().Name}.GetValue() Key '{m_Key}' in {m_Scriptionary.name}");
				return default;
			}
			return value;
		}
	}
}
