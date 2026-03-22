using ODev.Picker;
using ODev.PoseAnimator;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New EquipItem Ability", menuName = "Character/Ability/Player EquipItem")]
public class SOPlayerAbilityEquipItem : SOCharacterAbility
{
	[SerializeField, AssetNonNull] private CharacterEquippedItemEvent m_EquipItemEvent;
	[SerializeField, AssetNonNull] private CharacterEquippedItemVariableSO m_EquippedItemVariable;
	
	[Space, SerializeField, AssetNonNull] private SOPoseMontage m_InMontage = null;
	[SerializeField, AssetNonNull] private SOPoseMontage m_OutMontage = null;


	public CharacterEquippedItemEvent EquipItemEvent => m_EquipItemEvent;
	public CharacterEquippedItemVariableSO EquippedItemVariable => m_EquippedItemVariable;
	public SOPoseMontage InMontage => m_InMontage;
	public SOPoseMontage OutMontage => m_OutMontage;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilityEquipItem(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilityEquipItem : CharacterAbility<SOPlayerAbilityEquipItem>
{
	private float m_TimeElapsed;
	private CharacterEquippedItem m_ToEquipItem;

	public PlayerAbilityEquipItem(PlayerRoot pPlayer, SOPlayerAbilityEquipItem pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : 
		base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	protected override void Initalize()
	{
		Data.EquipItemEvent.Register(OnQueuedEquipItemChanged);
	}

	protected override void DestroyInternal()
	{
		Data.EquipItemEvent.Register(OnQueuedEquipItemChanged);
	}

	private void OnQueuedEquipItemChanged(CharacterEquippedItem pItem)
	{
		m_ToEquipItem = pItem;
		if (IsActive)
		{
			Deactivate();
		}
		Root.Abilities.TryActivateAbilityInstance(this);
	}

	protected override void ActivateInternal()
	{
		Root.Animator.PlayMontage(Data.InMontage);
		m_TimeElapsed = 0.0f;

		EarlyUnEquippingItem();
		EarlyEquippingItem();
	}

	public override void ActiveTick(float pDeltaTime)
	{
		m_TimeElapsed += pDeltaTime;
		float seconds = Data.InMontage.TotalSeconds - Data.InMontage.FadeOutSeconds;
		if (m_TimeElapsed >= seconds)
		{
			Deactivate();
			return;
		}
	}

	protected override void DeactivateInternal()
	{
		FinishUnEquippingItem(Data.EquippedItemVariable.Value);
		FinishEquippingItem();

		if (Data.EquippedItemVariable.Value != null && Data.EquippedItemVariable.Value.Data.AnimatorConfig != null)
		{
			Root.Animator.SwitchConfig(Data.EquippedItemVariable.Value.Data.AnimatorConfig);
		}
		Root.Animator.PlayMontage(Data.OutMontage);
	}

	private void EarlyEquippingItem()
	{
		if (m_ToEquipItem == null)
		{
			return;
		}
		Root.Abilities.AddAbilities(m_ToEquipItem.Data.Abilities);
		Root.GameplayTags.AddTag(m_ToEquipItem.Data.Tag);
	}

	private void FinishEquippingItem()
	{
		Data.EquippedItemVariable.SetValue(m_ToEquipItem);
		m_ToEquipItem = null;

		if (Data.EquippedItemVariable.Value == null)
		{
			return;
		}
		Root.HolsterInventory.TryAddItem(Data.EquippedItemVariable.Value);
	}

	public void EarlyUnEquippingItem()
	{
		var item = Data.EquippedItemVariable.Value;
		if (item == null)
		{
			return;
		}
		Root.Abilities.RemoveAbilities(item.Data.Abilities);
		Root.GameplayTags.RemoveTag(item.Data.Tag);
	}

	public void FinishUnEquippingItem(CharacterEquippedItem pItem)
	{
		if (pItem == null)
		{
			return;
		}
		// TODO: If has item equipped but it doesn't have a holster. Drop it!
	}
}