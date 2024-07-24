using System.Collections.Generic;
using UnityEngine;
using ODev.Input;
using UnityEngine.InputSystem;
using InputSystem = InputSystem_Game;

public class InputBridge_BuildMode : InputBridge_Base
{
	[SerializeField]
	private InputModule_Trigger m_ExitInput = new();
	[SerializeField]
	private InputModule_Trigger m_PlaceInput = new();
	[SerializeField]
	private InputModule_Scroll m_RotateInput = new();

	public InputModule_Trigger Exit => m_ExitInput;
	public InputModule_Trigger Place => m_PlaceInput;
	public InputModule_Scroll Rotate => m_RotateInput;

	public override InputActionMap Actions => InputSystem.Instance.BuildMode.Get();
	public override IEnumerable<IInputModule> GetAllInputModules()
	{
		yield return m_ExitInput;
		yield return m_PlaceInput;
		yield return m_RotateInput;
	}

	protected override void Awake()
	{
		m_ExitInput.Initalize(InputSystem.Instance.BuildMode.Exit, IsValid);
		m_PlaceInput.Initalize(InputSystem.Instance.BuildMode.Place, IsValid);
		m_RotateInput.Initalize(InputSystem.Instance.BuildMode.Rotate, IsValid);

		base.Awake();
	}
}
