using System;
using System.Collections;
using System.Collections.Generic;
using ODev;
using UnityEngine;

public class PlayerBuildingInventory : MonoBehaviourSingleton<PlayerBuildingInventory>
{
	public struct Item
	{
		public SOBuildingItem Data { get; internal set; }
		public int Count { get; internal set; }

		public Item(SOBuildingItem pData, int pCount)
		{
			Data = pData;
			Count = pCount;
		}
	}

	private readonly Dictionary<string, Item> m_Items = new();

	public void AddItem(SOBuildingItem pItem)
	{
		string itemName = pItem.name;
		if (!m_Items.TryGetValue(itemName, out Item value))
		{
			value.Data = pItem;
		}
		value.Count++;
		m_Items[pItem.name] = value;
	}

	/// <returns>Remaining count of the item</returns>
	public int RemoveItem(SOBuildingItem pItem)
	{
		string itemName = pItem.name;
		if (!m_Items.TryGetValue(itemName, out Item value))
		{
			return 0;
		}
		value.Count = Mathf.Max(0, value.Count - 1);
		m_Items[itemName] = value;
		return value.Count;
	}

	public bool HasItem(SOBuildingItem pItem, out int oCount)
	{
		string itemName = pItem.name;
		if (!m_Items.TryGetValue(itemName, out Item value))
		{
			oCount = 0;
			return false;
		}
		oCount = value.Count;
		return true;
	}

	public IEnumerable<Item> GetItems()
	{
		foreach (Item item in m_Items.Values)
		{
			yield return item;
		}
	}
}