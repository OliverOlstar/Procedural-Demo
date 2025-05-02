using System.Collections;
using System.Collections.Generic;
using ODev.Update;
using UnityEngine;

public class CarWheelModel : MonoBehaviour
{
    [SerializeField]
    private Updateable m_Updateable = new(Type.Fixed, Priority.ModelController);
    [SerializeField]
    private CarWheelController m_Wheel = null;
    [SerializeField]
    private float m_DistanceOffset = -0.5f;

    private void OnEnable()
    {
        m_Updateable.Register(Tick);
    }

    private void OnDisable()
    {
        m_Updateable.Deregister();
    }

    private void Tick(float deltaTime)
    {
        transform.localPosition = Vector3.down * (m_Wheel.GroundDistance + m_DistanceOffset);
    }
}
