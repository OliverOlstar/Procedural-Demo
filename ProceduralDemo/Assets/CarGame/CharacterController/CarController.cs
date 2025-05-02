using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField]
    private InputBridge_CarGame m_Input = null;

    [SerializeField]
    private CarWheelController m_LeftFrontWheel = null;
    [SerializeField]
    private CarWheelController m_RightFrontWheel = null;

    [SerializeField]
    private CarWheelController m_LeftRearWheel = null;
    [SerializeField]
    private CarWheelController m_RightRearWheel = null;

    private void Start()
    {
        m_Input.Accelerate.RegisterOnChanged(OnAccelerateChanged);
        m_Input.Break.RegisterOnChanged(OnBreakChanged);
        m_Input.Steer.RegisterOnChanged(OnSteerChanged);
    }

    private void OnDestroy()
    {
        m_Input.Accelerate.DeregisterOnChanged(OnAccelerateChanged);
        m_Input.Break.DeregisterOnChanged(OnBreakChanged);
        m_Input.Steer.DeregisterOnChanged(OnSteerChanged);
    }

    private void OnAccelerateChanged(float pInput)
    {
        // m_LeftFrontWheel.Input = pInput;
        // m_RightFrontWheel.Input = pInput;
        m_LeftRearWheel.Input = pInput;
        m_RightRearWheel.Input = pInput;
    }

    private void OnBreakChanged(bool pPressed)
    {
        m_LeftFrontWheel.Breaking = pPressed;
        m_RightFrontWheel.Breaking = pPressed;
        m_LeftRearWheel.Breaking = pPressed;
        m_RightRearWheel.Breaking = pPressed;
    }

    private void OnSteerChanged(float pInput)
    {
        float angle = 90.0f + (pInput * 12.5f);
        m_LeftFrontWheel.transform.localRotation = Quaternion.Euler(0.0f, angle, 0.0f);
        m_RightFrontWheel.transform.localRotation = Quaternion.Euler(0.0f, angle, 0.0f);
    }
}
