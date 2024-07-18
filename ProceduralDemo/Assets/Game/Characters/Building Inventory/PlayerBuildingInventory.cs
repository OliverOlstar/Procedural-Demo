using System;
using System.Collections;
using System.Collections.Generic;
using ODev;
using UnityEngine;

public class PlayerBuildingInventory : MonoBehaviourSingleton<PlayerBuildingInventory>
{
	private readonly Dictionary<string, (SOBuildingItem, int)> m_Items = new();

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void AddItem(SOBuildingItem pItem)
	{
		string itemName = pItem.name;
		if (!m_Items.TryGetValue(itemName, out (SOBuildingItem, int) value))
		{
			value.Item1 = pItem;
		}
		value.Item2++;
		m_Items[pItem.name] = value;
	}

	public bool HasItem(SOBuildingItem pItem, out int oCount)
	{
		string itemName = pItem.name;
		if (!m_Items.TryGetValue(itemName, out (SOBuildingItem, int) value))
		{
			oCount = 0;
			return false;
		}
		oCount = value.Item2;
		return true;
	}
}
