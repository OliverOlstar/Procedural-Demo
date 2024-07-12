using System.Collections.Generic;
using UnityEngine;
using ODev.Input;
using UnityEngine.InputSystem;
using InputSystem = InputSystem_Game;

public class InputBridge_PlayerCharacter : InputBridge_Base
{
	[SerializeField]
	private InputModule_Vector2 moveInput = new(); 

	public InputModule_Vector2 Move => moveInput;

	public override InputActionMap Actions => InputSystem.Instance.PlayerCharacter.Get();
	public override IEnumerable<IInputModule> GetAllInputModules()
	{
		yield return moveInput;
	}

	protected override void Awake()
	{
		moveInput.Initalize(InputSystem.Instance.PlayerCharacter.Move, IsValid);

		base.Awake();
	}
}
