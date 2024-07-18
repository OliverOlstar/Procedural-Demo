using System.Collections;
using System.Collections.Generic;
using ODev;
using UnityEngine;

public class PlayerRoot : MonoBehaviour
{
	[SerializeField]
	private InputBridge_PlayerCharacter m_Input = null;
	[SerializeField]
	private CharacterMovement m_Movement = null;
	[SerializeField]
	private OnGround m_OnGround = null;
	[SerializeField]
	private CharacterInventory m_Inventory = new();
	[SerializeField]
	private PlayerAbilities m_Abilities = new();

	public InputBridge_PlayerCharacter Input => m_Input;
	public CharacterMovement Movement => m_Movement;
	public OnGround OnGround => m_OnGround;
	public CharacterInventory Inventory => m_Inventory;
	public PlayerBuildingInventory Buildings => PlayerBuildingInventory.Instance;
	public PlayerAbilities Abilities => m_Abilities;

	private void Start()
	{
		m_Inventory.Initalize();
		m_Abilities.Initalize(this);
	}

	private void OnDestroy()
	{
		m_Inventory.Destroy();
		m_Abilities.Destroy();
	}
}
