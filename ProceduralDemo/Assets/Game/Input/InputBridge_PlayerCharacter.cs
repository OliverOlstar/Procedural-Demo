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
	private InputModule_Trigger jumpInput = new();

	public InputModule_Vector2 Move => moveInput;
	public InputModule_Trigger Jump => jumpInput;

	public override InputActionMap Actions => InputSystem.Instance.PlayerCharacter.Get();
	public override IEnumerable<IInputModule> GetAllInputModules()
	{
		yield return moveInput;
		yield return jumpInput;
	}

	protected override void Awake()
	{
		moveInput.Initalize(InputSystem.Instance.PlayerCharacter.Move, IsValid);
		jumpInput.Initalize(InputSystem.Instance.PlayerCharacter.Jump, IsValid);

		base.Awake();
	}
}
