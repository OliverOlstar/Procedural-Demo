using UnityEngine;

public class RotateSelf : MonoBehaviour
{
	[SerializeField]
	private ODev.Update.Updateable m_Updateable = new(ODev.Update.Type.Default, ODev.Update.Priority.Default, 2.0f);
	[SerializeField]
	private Vector3 m_RotateSpeed = new(0.0f, 1.0f, 0.0f);

	private void OnEnable()
	{
		m_Updateable.Register(Tick);
	}

	private void OnDisable()
	{
		m_Updateable.Deregister();
	}

	private void Tick(float pDeltaTime)
	{
		transform.Rotate(m_RotateSpeed * pDeltaTime, Space.Self);
	}
}
