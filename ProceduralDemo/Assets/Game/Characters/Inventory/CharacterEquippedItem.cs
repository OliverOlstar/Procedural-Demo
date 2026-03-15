using UnityEngine;

public class CharacterEquippedItem
{
	private readonly CharacterEquipableItemSO m_Data;
	private readonly int m_CreatedId;

	public CharacterEquipableItemSO Data => m_Data;
	public int CreatedId => m_CreatedId;

	public CharacterEquippedItem(CharacterEquipableItemSO pItem)
	{
		m_Data = pItem;
		m_CreatedId = Random.Range(int.MinValue, int.MaxValue);
	}
}
