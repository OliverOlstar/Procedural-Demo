using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scriptionary
{
	public abstract class SOScriptionaryBase : ScriptableObject
	{
		public abstract IEnumerable<IScriptionaryItem> _EditorGetItems();
	}

	public abstract class SOScriptionaryGeneric<TItem> : SOScriptionaryBase where TItem : IScriptionaryItem
	{
		protected abstract List<TItem> GetRawItems();

		private Dictionary<string, TItem> m_Dict = null;

		private void OnValidate()
		{
			m_Dict = null; // Make sure this isn't stale when modified at edit time
		}

		private void Initialize()
		{
			if (m_Dict != null)
			{
				return;
			}
			List<TItem> items = GetRawItems();
			m_Dict = new Dictionary<string, TItem>(items.Count);
			foreach (TItem item in items)
			{
				m_Dict.Add(item.Key, item);
			}
		}

		public bool TryGet(string key, out TItem value)
		{
			Initialize();
			return m_Dict.TryGetValue(key, out value);

			//int length = m_Items.Count;
			//for (int i = 0; i < length; i++)
			//{
			//	TItem item = m_Items[i];
			//	if (item.Key == key)
			//	{
			//		value = item;
			//		return true;
			//	}
			//}
			//value = default;
			//return false;
		}

		public override IEnumerable<IScriptionaryItem> _EditorGetItems()
		{
			foreach (TItem item in GetRawItems())
			{
				yield return item;
			}
		}
	}

	public abstract class SOScriptionary<TItem> : SOScriptionaryGeneric<TItem> where TItem : IScriptionaryItem
	{
#if ODIN_INSPECTOR
		[Sirenix.OdinInspector.DrawWithUnity]
#elif UNITY_2021_OR_NEWER
		[Core.SerializedReferenceDrawer(Core.SerializedRefGUIStyle.Flat)]
#endif
		[SerializeReference]
		private List<TItem> m_Items = new List<TItem>();

		protected override List<TItem> GetRawItems() => m_Items;
	}

	/// <summary>Use SOScriptionarySimple when item type is not an interface, abstract class, or parent of a class hierarchy.
	/// This is especially useful when the item type has a custom PropertyDrawer.</summary>
	public abstract class SOScriptionarySimple<TItem> : SOScriptionaryGeneric<TItem> where TItem : IScriptionaryItem
	{
//#if ODIN_INSPECTOR
//		[Sirenix.OdinInspector.DrawWithUnity]
//#endif
		[SerializeField]
		private List<TItem> m_Items = new List<TItem>();

		protected override List<TItem> GetRawItems() => m_Items;
	}

	public interface IScriptionaryItem
	{
		public string Key { get; }
	}
}
