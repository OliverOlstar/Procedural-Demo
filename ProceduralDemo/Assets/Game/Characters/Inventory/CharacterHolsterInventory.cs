using ODev.Picker;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class CharacterHolsterInventory
{
	[SerializeField] private CharacterModelLocatorsManager m_Locators;
	[SerializeField] private InputActionReference[] m_InputActions = new InputActionReference[4];

	[SerializeField, AssetNonNull] private CharacterEquippedItemEvent m_HolsterItemAddedEvent;
	[SerializeField, AssetNonNull] private CharacterEquippedItemEvent m_HolsterItemRemovedEvent;

	private CharacterEquippedItem[] m_HolsterSlots;

	public void Initalize()
	{
		m_HolsterSlots = new CharacterEquippedItem[m_InputActions.Length];
	}

	public void Dispose()
	{

	}

	public bool TryAddItem(CharacterEquippedItem pItem)
	{
		for (int i = 0; i < m_HolsterSlots.Length; i++)
		{
			if (m_HolsterSlots[i] == null)
			{
				continue;
			}
			m_HolsterSlots[i] = pItem;
		}

		var view = Object.Instantiate(pItem.Data.Prefab);
		view.Initalize(pItem, m_Locators);

		m_HolsterItemAddedEvent.Fire(pItem);
		return true;
	}

	public bool TryAddItem(CharacterEquipableItemSO pItem)
	{
		CharacterEquippedItem equippedItem = new(pItem);
		return TryAddItem(equippedItem);
	}

	public bool RemoveItem(int pCreatedId)
	{
		for (int i = 0; i < m_HolsterSlots.Length; i++)
		{
			var item = m_HolsterSlots[i];
			if (item == null || item.CreatedId != pCreatedId)
			{
				continue;
			}
			m_HolsterSlots[i] = null;
			m_HolsterItemRemovedEvent.Fire(item);
			return true;
		}
		return false;
	}
}