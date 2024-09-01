using System.Collections.Generic;
using UnityEngine;
using ODev.Input;
using UnityEngine.InputSystem;
using InputSystem = InputSystem_Game;

public class InputBridge_PlayerCharacter : InputBridge_Base
{
	[SerializeField]
	private InputModule_Vector2 moveInput = new();
	[SerializeField]
	private InputModule_Toggle jumpInput = new();
	[SerializeField]
	private InputModule_Toggle sprintInput = new();
	[SerializeField]
	private InputModule_Toggle crouchInput = new();
	[SerializeField]
	private InputModule_Trigger interactInput = new();
	[SerializeField]
	private InputModule_Trigger abilityPrimaryInput = new();
	[SerializeField]
	private InputModule_Trigger abilitySecondaryInput = new();

	public InputModule_Vector2 Move => moveInput;
	public InputModule_Toggle Jump => jumpInput;
	public InputModule_Toggle Sprint => sprintInput;
	public InputModule_Toggle Crouch => crouchInput;
	public InputModule_Trigger Interact => interactInput;
	public InputModule_Trigger AbilityPrimary => abilityPrimaryInput;
	public InputModule_Trigger AbilitySecondary => abilitySecondaryInput;

	public override InputActionMap Actions => InputSystem.Instance?.PlayerCharacter.Get();
	public override IEnumerable<IInputModule> GetAllInputModules()
	{
		yield return moveInput;
		yield return jumpInput;
		yield return sprintInput;
		yield return crouchInput;
		yield return interactInput;
		yield return abilityPrimaryInput;
		yield return abilitySecondaryInput;
	}

	protected override void Awake()
	{
		PlayerInput_Game.PlayerCharacterActions input = InputSystem.Instance.PlayerCharacter;
		moveInput.Initalize(input.Move, IsValid);
		jumpInput.Initalize(input.Jump, IsValid);
		sprintInput.Initalize(input.Sprint, IsValid);
		crouchInput.Initalize(input.Crouch, IsValid);
		interactInput.Initalize(input.Interact, IsValid);
		abilityPrimaryInput.Initalize(input.AbilityPrimary, IsValid);
		abilitySecondaryInput.Initalize(input.AbilitySecondary, IsValid);

		base.Awake();
	}
}
