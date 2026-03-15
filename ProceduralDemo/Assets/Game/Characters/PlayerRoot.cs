using ODev;
using UnityEngine;

public class PlayerRoot : MonoBehaviour, PlayerModeController.IMode
{
	[SerializeField] private PlayerModeController m_Mode = null;
	[SerializeField] private InputBridge_PlayerCharacter m_Input = null;
	[SerializeField] private CharacterMovement m_Movement = null;
	[SerializeField] private OnGround m_OnGround = null;
	[SerializeField] private CharacterOnWall m_OnWall = null;
	[SerializeField] private ThirdPersonCamera m_Camera = null;
	[SerializeField] private PoseAnimatorController m_Animator = null;
	[SerializeField] private CharacterInventory m_Inventory = new();
	[SerializeField] private PlayerAbilities m_Abilities = new();
	[SerializeField] private PlayerSpearBehaviour m_Spear = null;
	[SerializeField] private CharacterHolsterInventory m_HolsterInventory = new();
	[SerializeField] private CharacterEquippedInventory m_EquippedInventory = new();

	public PlayerModeController Mode => m_Mode;
	public InputBridge_PlayerCharacter Input => m_Input;
	public CharacterMovement Movement => m_Movement;
	public OnGround OnGround => m_OnGround;
	public CharacterOnWall OnWall => m_OnWall;
	public ThirdPersonCamera Camera => m_Camera;
	public PoseAnimatorController Animator => m_Animator;
	public CharacterInventory Inventory => m_Inventory;
	public PlayerBuildingInventory Buildings => PlayerBuildingInventory.Instance;
	public PlayerAbilities Abilities => m_Abilities;
	public PlayerSpearBehaviour Spear => m_Spear;
	public CharacterHolsterInventory HolsterInventory => m_HolsterInventory;
	public CharacterEquippedInventory EquippedInventory => m_EquippedInventory;

	private void Start()
	{
		m_Inventory.Initalize();
		m_Abilities.Initalize(this);
		m_HolsterInventory.Initalize();
		m_EquippedInventory.Initalize(this);
		m_Spear.Initalize(this);
	}

	private void OnDestroy()
	{
		m_Inventory.Dispose();
		m_Abilities.Destroy();
		m_HolsterInventory.Dispose();
		m_EquippedInventory.Dispose();
		m_Spear.Destroy();
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
