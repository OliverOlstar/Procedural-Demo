using ODev.Update;
using ODev.Util;
using Unity.Mathematics;
using UnityEngine;

public class CarWheelController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody m_Body = null;
    [SerializeField]
    private Updateable m_Updateable = new(Type.Fixed, Priority.CharacterController);

    [Header("Raycast")]
    [SerializeField]
    private float m_Distance = 1.0f;
    [SerializeField]
    private float m_RestDistance = 0.5f;
    [SerializeField]
    private LayerMask m_GroundLayers = new();

    [Header("Spring")]
    [SerializeField]
    private float m_Strength = 100.0f;
    [SerializeField]
    private float m_Dampening = 10.0f;

    [Header("Steering")]
    [SerializeField]
    private float m_TireGripPercent = 0.9f;
    [SerializeField]
    private float m_TireMass = 1.0f;

    [Header("Forward")]
    [SerializeField]
    private float m_Acceleration = 1.0f;
    [SerializeField]
    private float m_MaxSpeed = 10.0f;
    [SerializeField]
    private AnimationCurve m_SpeedCurve = Anim.DefaultAnimationCurve;

    [Header("Breaks")]
    public bool Breaking = false;
    [SerializeField]
    private float m_BreakPercent = 0.7f;
    [SerializeField]
    private float m_MaxBreak = 5.0f;

    [Header("Idle")]
    [SerializeField]
    private float m_IdleDeceleration = 0.1f;
    [SerializeField]
    private float m_MaxIdleDeceleration = 5.0f;

    private readonly RaycastHit[] m_Hits = new RaycastHit[5];
    private Vector3 m_SpringForce;
    private Vector3 m_SteeringForce;
    private Vector3 m_ForwardForce;
    private float m_GroundDistance = 0.0f;

    [Range(-1.0f, 1.0f)]
    public float Input = 0.0f;

    public float GroundDistance => m_GroundDistance;

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
        Vector3 springDirection = transform.up;
        Vector3 steeringDirection = transform.right;
        Vector3 forwardDirection = transform.forward;

        int hitCount = Physics.RaycastNonAlloc(transform.position, -springDirection, m_Hits, m_Distance, m_GroundLayers);
        if (hitCount > 0)
        {
            m_GroundDistance = m_Hits[0].distance;
            for (int i = 1; i < hitCount; i++)
            {
                m_GroundDistance = Mathf.Min(m_GroundDistance, m_Hits[i].distance);
            }

            Vector3 tireWorldVelocity = m_Body.GetPointVelocity(transform.position);

            // Spring
            m_SpringForce = CalculateSpringForce(springDirection, tireWorldVelocity);

            // Steering
            m_SteeringForce = CalculateCounterForce(deltaTime, steeringDirection, tireWorldVelocity, m_TireGripPercent);

            // Acceleration
            if (!Input.IsNearZero())
            {
                float forwardVelocity = Vector3.Dot(forwardDirection, tireWorldVelocity);
                float normalizedForwardVelocity = Mathf.Clamp01(Mathf.Abs(forwardVelocity) / m_MaxSpeed);
                if (normalizedForwardVelocity < 1.0f)
                {
                    float forwardScalar = m_SpeedCurve.Evaluate(normalizedForwardVelocity) * Input;
                    m_ForwardForce = m_Acceleration * forwardScalar * forwardDirection;
                }
                else
                {
                    m_ForwardForce = CalculateCounterForce(deltaTime, forwardDirection, tireWorldVelocity, m_IdleDeceleration, m_MaxIdleDeceleration);
                }
            }
            else if (Breaking)
            {
                m_ForwardForce = CalculateCounterForce(deltaTime, forwardDirection, tireWorldVelocity, m_BreakPercent, m_MaxBreak);
            }
            else
            {
                m_ForwardForce = CalculateCounterForce(deltaTime, forwardDirection, tireWorldVelocity, m_IdleDeceleration, m_MaxIdleDeceleration);
            }

            m_Body.AddForceAtPosition((m_SteeringForce + m_SpringForce + m_ForwardForce) * deltaTime, transform.position, ForceMode.VelocityChange);
        }
        else
        {
            m_GroundDistance = m_Distance;
        }
    }

    private Vector3 CalculateSpringForce(Vector3 springDirection, Vector3 tireWorldVelocity)
    {
        float offset = m_RestDistance - m_GroundDistance;
        float velocity = Vector3.Dot(springDirection, tireWorldVelocity);
        float force = (m_Strength * offset) - (m_Dampening * velocity);
        force = Mathf.Max(force, 0.0f);
        return force * springDirection;
    }

    private Vector3 CalculateCounterForce(float deltaTime, Vector3 direction, Vector3 tireWorldVelocity, float gripPercent, float maxForce = float.PositiveInfinity)
    {
        float steeringVelocity = Vector3.Dot(direction, tireWorldVelocity);
        float desiredVelocityChange = -steeringVelocity * gripPercent;
        float desiredAcceleration = desiredVelocityChange / deltaTime;
        Vector3 force = desiredAcceleration * m_TireMass * direction;
        return Vector3.ClampMagnitude(force, maxForce);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, m_SpringForce);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, m_SteeringForce);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, m_ForwardForce);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, m_Body.GetPointVelocity(transform.position));
    }
}
