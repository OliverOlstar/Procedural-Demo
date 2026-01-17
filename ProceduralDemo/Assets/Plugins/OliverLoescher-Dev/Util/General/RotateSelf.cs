using UnityEngine;
using ODev.Updateables;

public class RotateSelf : MonoBehaviour
{
	[SerializeField]
	private Updateable m_Updateable = new(UpdateableType.Default, UpdateablePriority.Default, 2.0f);
	[SerializeField]
	private Vector3 m_RotateSpeed = new(0.0f, 1.0f, 0.0f);

	private void OnEnable()
	{
		m_Updateable.Register(Tick);
	}

	private void OnDisable()
	{
		m_Updateable.UnRegister();
	}

	private void Tick(float pDeltaTime)
	{
		transform.Rotate(m_RotateSpeed * pDeltaTime, Space.Self);
	}
}
