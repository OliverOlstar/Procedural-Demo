using System.Collections.Generic;
using BandoWare.GameplayTags;
using ODev.Picker;
using ODev.PoseAnimator;
using ODev.VariableSOs;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterEquipableItem", menuName = "Game/CharacterEquipableItem")]
public class CharacterEquipableItemSO : ScriptableObject
{
	[SerializeField] private GameplayTag m_Tag;
	[SerializeField, AssetNonNull] private EmptyVariableSO m_HolsterLocatorId;
	[SerializeField, AssetNonNull] private EmptyVariableSO m_EquippedLocatorId;
	[SerializeField] private CharacterEquippedItemView m_Prefab;
	[SerializeField, AssetNonNull] private SOCharacterAbility[] m_Abilities;
	[SerializeField, AssetNonNull] private SOPoseAnimatorConfig m_AnimatorConfig;

	public GameplayTag Tag => m_Tag;
	public EmptyVariableSO HolsterLocatorId => m_HolsterLocatorId;
	public EmptyVariableSO EquippedLocatorId => m_EquippedLocatorId;
	public CharacterEquippedItemView Prefab => m_Prefab;
	public IReadOnlyList<SOCharacterAbility> Abilities => m_Abilities;
	public SOPoseAnimatorConfig AnimatorConfig => m_AnimatorConfig;
}
