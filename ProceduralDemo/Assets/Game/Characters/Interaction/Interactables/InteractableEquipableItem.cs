using UnityEngine;

public class InteractableEquipableItem : InteractableItem
{
	[SerializeField, ODev.Picker.AssetNonNull] private CharacterEquipableItemSO m_Item = null;

	public override void Interact(PlayerRoot pPlayer)
	{
		pPlayer.EquippedInventory.EquipItem(m_Item);
		base.Interact(pPlayer);
	}
}
