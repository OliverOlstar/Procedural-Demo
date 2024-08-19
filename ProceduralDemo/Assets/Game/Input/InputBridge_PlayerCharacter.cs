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
	private InputModule_Trigger interactInput = new();

	public InputModule_Vector2 Move => moveInput;
	public InputModule_Toggle Jump => jumpInput;
	public InputModule_Toggle Sprint => sprintInput;
	public InputModule_Trigger Interact => interactInput;

	public override InputActionMap Actions => InputSystem.Instance?.PlayerCharacter.Get();
	public override IEnumerable<IInputModule> GetAllInputModules()
	{
		yield return moveInput;
		yield return jumpInput;
		yield return sprintInput;
		yield return interactInput;
	}

	protected override void Awake()
	{
		PlayerInput_Game.PlayerCharacterActions input = InputSystem.Instance.PlayerCharacter;
		moveInput.Initalize(input.Move, IsValid);
		jumpInput.Initalize(input.Jump, IsValid);
		sprintInput.Initalize(input.Sprint, IsValid);
		interactInput.Initalize(input.Interact, IsValid);

		base.Awake();
	}
}
