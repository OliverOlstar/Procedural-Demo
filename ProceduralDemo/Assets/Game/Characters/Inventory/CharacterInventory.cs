using System.Collections.Generic;

[System.Serializable]
public class CharacterInventory
{
	[System.Serializable]
	public abstract class Item
	{
		private readonly string m_Id;

		public string Id => m_Id;

		public abstract void OnGain();
		public abstract void OnLost();
		public abstract void OnStackIncrease();
		public abstract void OnStackDecrease();
	}

	private readonly Dictionary<string, int> m_ItemCounts = new();
	private readonly List<Item> m_Items = new();

	public void Initalize()
	{

	}

	public void Destroy()
	{

	}

	public void AddItem(Item pItem)
	{
		if (m_ItemCounts.ContainsKey(pItem.Id))
		{
			m_ItemCounts[pItem.Id]++;
			return;
		}
		m_Items.Add(pItem);
		m_ItemCounts.Add(pItem.Id, 1);
	}

	public void RemoveItem(Item pItem)
	{
		if (!m_ItemCounts.TryGetValue(pItem.Id, out int count))
		{
			return;
		}
		if (count > 1)
		{
			m_ItemCounts[pItem.Id]--;
			return;
		}
		m_ItemCounts.Remove(pItem.Id);
		m_Items.Remove(pItem);
	}
}
