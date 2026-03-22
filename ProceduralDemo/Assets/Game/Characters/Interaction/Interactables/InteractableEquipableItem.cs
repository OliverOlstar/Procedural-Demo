using ODev.Picker;
using UnityEngine;

public class InteractableEquipableItem : InteractableItem
{
	[SerializeField, AssetNonNull] private CharacterEquipableItemSO m_Item;
	[SerializeField, AssetNonNull] private CharacterEquippedItemEvent m_EquipItemEvent;

	public override void Interact(PlayerRoot pPlayer)
	{
		m_EquipItemEvent.Fire(new CharacterEquippedItem(m_Item));
		base.Interact(pPlayer);
	}
}
