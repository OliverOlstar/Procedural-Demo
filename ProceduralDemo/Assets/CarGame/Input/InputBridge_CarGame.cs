using System.Collections;
using System.Collections.Generic;
using ODev.Input;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputBridge_CarGame : InputBridge_Base
{
    [SerializeField]
    private InputModule_Scroll m_SteerInput = new();
    [SerializeField]
    private InputModule_Scroll m_AccelerateInput = new();
    [SerializeField]
    private InputModule_Toggle m_BreakInput = new();
    [SerializeField]
    private InputModule_Trigger m_JumpInput = new();

    public InputModule_Scroll Steer => m_SteerInput;
    public InputModule_Scroll Accelerate => m_AccelerateInput;
    public InputModule_Toggle Break => m_BreakInput;
    public InputModule_Trigger Jump => m_JumpInput;

    public override InputActionMap Actions => InputSystem_CarGame.Instance.Car.Get();
    public override IEnumerable<IInputModule> GetAllInputModules()
    {
        yield return m_SteerInput;
        yield return m_AccelerateInput;
        yield return m_BreakInput;
        yield return m_JumpInput;
    }

    protected override void Awake()
    {
        PlayerInput_CarGame.CarActions input = InputSystem_CarGame.Instance.Car;
        m_SteerInput.Initalize(input.Steer, IsValid);
        m_AccelerateInput.Initalize(input.Accelerate, IsValid);
        m_BreakInput.Initalize(input.Break, IsValid);
        m_JumpInput.Initalize(input.Jump, IsValid);

        base.Awake();
    }
}
