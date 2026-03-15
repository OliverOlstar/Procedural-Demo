using ODev.Picker;
using ODev.Util;
using UnityEngine;

public class CharacterEquippedItemView : MonoBehaviour
{
	[SerializeField, AssetNonNull] private CharacterEquippedItemEvent m_ItemRemovedEvent;
	[SerializeField, AssetNonNull] private CharacterEquippedItemVariableSO m_EquippedItemVariable;

	private CharacterModelLocator m_HolsterLocator;
	private CharacterModelLocator m_EquippedLocator;
	private CharacterEquippedItem m_Item;

	public CharacterEquippedItemView Initalize(CharacterEquippedItem pItem, CharacterModelLocatorsManager pLocatorsManager)
	{
		if (!pLocatorsManager.TryGetFreeLocator(pItem.Data.HolsterLocatorId, out m_HolsterLocator))
		{
			this.DevException($"Failed to get locator with the Id {pItem.Data.HolsterLocatorId}, consider adding more");
			// TODO: Drop equipped or last equipped item instead and use it's spot
			return this;
		}
		if (!pLocatorsManager.TryGetFreeLocator(pItem.Data.EquippedLocatorId, out m_EquippedLocator))
		{
			this.DevException($"Failed to get locator with the Id {pItem.Data.EquippedLocatorId}, this should never happen");
			return this;
		}
		m_Item = pItem;

		OnEquippedItemChanged();

		return this;
	}

	private void OnEnable()
	{
		m_ItemRemovedEvent.Register(OnItemRemoved);
		m_EquippedItemVariable.OnValueChanged += OnEquippedItemChanged;
	}

	private void OnDisable()
	{
		m_ItemRemovedEvent.UnRegister(OnItemRemoved);
		m_EquippedItemVariable.OnValueChanged -= OnEquippedItemChanged;
	}

	private void OnEquippedItemChanged(CharacterEquippedItem _ = default)
	{
		if (IsEquipped())
		{
			MoveToEquipped();
		}
		else
		{
			MoveToHolster();
		}
	}

	private void MoveToEquipped()
	{
		m_EquippedLocator.Attach(transform, false);
	}

	private void MoveToHolster()
	{
		m_HolsterLocator.Attach(transform, false);
	}

	private void OnItemRemoved(CharacterEquippedItem item)
	{
		Destroy(gameObject); // TODO: Object pool this
	}

	private bool IsEquipped()
	{
		return m_EquippedItemVariable.Value.CreatedId == m_Item.CreatedId;
	}
}
