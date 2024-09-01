using System.Collections;
using System.Collections.Generic;
using ODev;
using UnityEngine;

public class PlayerRoot : MonoBehaviour, PlayerModeController.IMode
{
	[SerializeField]
	private PlayerModeController m_Mode = null;
	[SerializeField]
	private InputBridge_PlayerCharacter m_Input = null;
	[SerializeField]
	private CharacterMovement m_Movement = null;
	[SerializeField]
	private OnGround m_OnGround = null;
	[SerializeField]
	private CharacterOnWall m_OnWall = null;
	[SerializeField]
	private ThirdPersonCamera m_Camera = null;
	[SerializeField]
	private PoseAnimatorController m_Animator = null;
	[SerializeField]
	private PlayerSpearBehaviour m_Spear = null;
	[SerializeField]
	private CharacterInventory m_Inventory = new();
	[SerializeField]
	private PlayerAbilities m_Abilities = new();

	public PlayerModeController Mode => m_Mode;
	public InputBridge_PlayerCharacter Input => m_Input;
	public CharacterMovement Movement => m_Movement;
	public OnGround OnGround => m_OnGround;
	public CharacterOnWall OnWall => m_OnWall;
	public ThirdPersonCamera Camera => m_Camera;
	public PoseAnimatorController Animator => m_Animator;
	public PlayerSpearBehaviour Spear => m_Spear;
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

	void PlayerModeController.IMode.DisableMode()
	{
		m_Input.gameObject.SetActive(false);
		m_Camera.gameObject.SetActive(false);
	}
	void PlayerModeController.IMode.EnableMode()
	{
		m_Input.gameObject.SetActive(true);
		m_Camera.gameObject.SetActive(true);
	}
}
