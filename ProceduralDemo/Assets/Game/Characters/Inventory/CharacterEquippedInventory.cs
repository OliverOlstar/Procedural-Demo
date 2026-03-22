using BandoWare.GameplayTags;
using UnityEngine;

[System.Serializable]
public class CharacterEquippedInventory
{
	[SerializeField] private CharacterEquippedItemVariableSO m_EquippedItemVariable;
	[SerializeField] private GameObjectGameplayTagContainer m_GameplayTagsContainer;

	private CharacterHolsterInventory m_HolsterInventory;
	private PlayerAbilities m_Abilities;

	public void Initalize(PlayerRoot pRoot)
	{
		m_Abilities = pRoot.Abilities;
		m_HolsterInventory = pRoot.HolsterInventory;
	}

	public void Dispose()
	{
		
	}

	public void EquipItem(CharacterEquipableItemSO pItem)
	{
		if (m_EquippedItemVariable.Value != null)
		{
			// TODO: If has item equipped but it doesn't have a holster. Drop it!
		}

		CharacterEquippedItem equippedItem = new(pItem);
		m_EquippedItemVariable.SetValue(equippedItem);
		m_HolsterInventory.TryAddItem(equippedItem);
		m_Abilities.AddAbilities(pItem.Abilities);
		m_GameplayTagsContainer.GameplayTagContainer.AddTag(pItem.Tag);
	}

	public void UnEquipCurrentItem()
	{
		if (m_EquippedItemVariable.Value == null)
		{
			return;
		}
		var equippedItem = m_EquippedItemVariable.Value.Data;
		m_Abilities.RemoveAbilities(equippedItem.Abilities);
		m_GameplayTagsContainer.GameplayTagContainer.RemoveTag(equippedItem.Tag);
		m_EquippedItemVariable.SetValue(null);
		// TODO: If has item equipped but it doesn't have a holster. Drop it!
	}
}
